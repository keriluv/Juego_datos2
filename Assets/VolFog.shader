Shader "VolFog"
{
    Properties
    {
        _Color("Fog Ambient Color", Color) = (0.18, 0.22, 0.3, 1)
        _AmbientIntensity("Fog Ambient Extinction", Range(0, 2)) = 0.4
        _MaxDistance("Max distance", float) = 75
        _StepSize("Step size", Range(0.05, 3)) = 0.35
        _NoiseOffset("Noise Jitter Amount", Range(0, 2)) = 1.0
        
        // --- WINDOW RAY & AIR DENSITY (CRITICAL FOR INDOORS) ---
        _BaseDensity("Base Air Dust Density", Range(0.01, 2.0)) = 0.4
        _DensityMultiplier("Density multiplier", Range(0, 10)) = 1.0
        _HeightFalloff("Height Falloff", Range(0, 1)) = 0.15
        _HeightOffset("Height Base Level", float) = 0.0
        
        // --- 3D Texture (Optional - rays will work even without it!) ---
        _FogNoise("Fog noise", 3D) = "white" {}
        _NoiseTiling("Noise tiling", float) = 1.0
        _DensityThreshold("Density threshold", Range(0, 1)) = 0.05
        
        // --- VOLUMETRIC WINDOW BEAMS ---
        [HDR]_LightContribution("Light color / HDR tint", Color) = (1, 0.92, 0.78, 1)
        _RayIntensity("Ray Intensity (God Ray Boost)", Range(0.5, 30)) = 4.5
        _LightScattering("Forward Mie Scatter (g factor)", Range(0, 0.99)) = 0.68
        _SideScattering("Side-View Visibility", Range(0, 1)) = 0.25
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            float4 _Color;
            float _AmbientIntensity;
            float _MaxDistance;
            float _StepSize;
            float _NoiseOffset;

            float _BaseDensity;
            float _DensityMultiplier;
            float _HeightFalloff;
            float _HeightOffset;

            TEXTURE3D(_FogNoise);
            float _DensityThreshold;
            float _NoiseTiling;

            float4 _LightContribution;
            float _RayIntensity;
            float _LightScattering;
            float _SideScattering;

            // FIX #1: Correct Henyey-Greenstein formula: (1.0 - g*g)
            float henyey_greenstein(float cosAngle, float g)
            {
                float g2 = g * g;
                float denom = 1.0 + g2 - 2.0 * g * cosAngle;
                return (1.0 - g2) / (4.0 * PI * pow(max(denom, 0.0001), 1.5));
            }

            // FIX #2: Combined Phase Function (Mie Forward + Isotropic Side View)
            // Allows window rays to be clearly seen from the SIDE across the room!
            float phase_function(float cosAngle, float g, float sideScatter)
            {
                float forwardMie = henyey_greenstein(cosAngle, g);
                float isotropic = 1.0 / (4.0 * PI);
                return lerp(forwardMie, isotropic, sideScatter);
            }
            
            // FIX #3: Robust Density Function (Guarantees rays exist without 3D texture + room height falloff)
            float get_density(float3 worldPos)
            {
                float heightFactor = exp(-max(0.0, worldPos.y - _HeightOffset) * _HeightFalloff);
                float density = _BaseDensity;

                // Optional procedural 3D noise variation if assigned
                float4 noise = _FogNoise.SampleLevel(sampler_TrilinearRepeat, worldPos * 0.01 * _NoiseTiling, 0);
                float turbulence = saturate(dot(noise, noise) - _DensityThreshold);
                density += turbulence * 0.5;

                return density * _DensityMultiplier * heightFactor;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float4 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, IN.texcoord);
                float depth = SampleSceneDepth(IN.texcoord);
                float3 worldPos = ComputeWorldSpacePosition(IN.texcoord, depth, UNITY_MATRIX_I_VP);

                float3 entryPoint = _WorldSpaceCameraPos;
                float3 viewDir = worldPos - _WorldSpaceCameraPos;
                float viewLength = length(viewDir);
                float3 rayDir = normalize(viewDir);

                float2 pixelCoords = IN.texcoord * _BlitTexture_TexelSize.zw;
                float distLimit = min(viewLength, _MaxDistance);

                // FIX #4: Jitter eliminates step-slicing banding artifacts
                float jitter = InterleavedGradientNoise(pixelCoords, (int)(_Time.y / max(HALF_EPS, unity_DeltaTime.x)));
                float distTravelled = jitter * _StepSize * _NoiseOffset;

                // FIX #5: Separate direct volumetric rays from ambient fog
                float3 inScatteredLight = float3(0, 0, 0);
                float transmittance = 1.0;

                // Direction towards the sun: -mainLight.direction
                Light mainLight = GetMainLight();
                float cosAngle = dot(rayDir, -mainLight.direction);
                float phase = phase_function(cosAngle, _LightScattering, _SideScattering);

                while(distTravelled < distLimit)
                {
                    float3 rayPos = entryPoint + rayDir * distTravelled;
                    float density = get_density(rayPos);
                    
                    if (density > 0.001)
                    {
                        // Sample realtime shadow map at this world-space point along the ray
                        Light currentLight = GetMainLight(TransformWorldToShadowCoord(rayPos));
                        
                        // FIX #6: Independent Ray Radiance (_RayIntensity)
                        float3 radiance = currentLight.color.rgb * _LightContribution.rgb * _RayIntensity;
                        inScatteredLight += radiance * phase * currentLight.shadowAttenuation * density * _StepSize * transmittance;

                        // Beer-Lambert transmittance extinction
                        transmittance *= exp(-density * _StepSize);
                        
                        if (transmittance < 0.001) break;
                    }
                    distTravelled += _StepSize;
                }
                
                // Base ambient indoor fog
                float fogFactor = 1.0 - saturate(transmittance);
                float3 ambientFog = _Color.rgb * fogFactor * _AmbientIntensity;

                // Final composite: Attenuated Scene + Ambient Room Fog + Window God Rays
                float3 finalRgb = col.rgb * transmittance + ambientFog + inScatteredLight;

                return float4(finalRgb, col.a);
            }
            ENDHLSL
        }
    }
}