using UnityEditor;
using UnityEngine;

/// <summary>
/// Si Unity vuelve a dejar un material del barco o del pirata en el shader
/// Standard (rosa en URP), lo pasa a Lit y guarda el asset antes de que Git
/// se lleve una referencia vacía.
/// </summary>
[InitializeOnLoad]
public static class RepararMaterialesUrp
{
    private const string ShaderUrp = "Universal Render Pipeline/Lit";

    static RepararMaterialesUrp()
    {
        EditorApplication.delayCall += Reparar;
    }

    private static void Reparar()
    {
        Shader lit = Shader.Find(ShaderUrp);
        if (lit == null)
            return;

        string[] carpetas =
        {
            "Assets/Stylized_Pirate_Ship",
            "Assets/Pirate/Materials",
            "Assets/recurso de pirata"
        };

        bool cambio = false;
        foreach (string carpeta in carpetas)
        {
            if (!AssetDatabase.IsValidFolder(carpeta))
                continue;

            string[] guids = AssetDatabase.FindAssets("t:Material", new[] { carpeta });
            foreach (string guid in guids)
            {
                string ruta = AssetDatabase.GUIDToAssetPath(guid);
                Material material = AssetDatabase.LoadAssetAtPath<Material>(ruta);
                if (material == null || material.shader == null)
                    continue;
                if (material.shader == lit)
                    continue;
                if (!EsShaderRoto(material.shader.name))
                    continue;

                Texture albedo = material.GetTexture("_BaseMap");
                if (albedo == null)
                    albedo = material.GetTexture("_MainTex");
                Texture normal = material.GetTexture("_BumpMap");
                Texture metal = material.GetTexture("_MetallicGlossMap");
                Texture oclusion = material.GetTexture("_OcclusionMap");
                Texture emision = material.GetTexture("_EmissionMap");
                Color color = material.HasProperty("_Color") ? material.GetColor("_Color") : Color.white;

                material.shader = lit;
                if (albedo != null)
                {
                    material.SetTexture("_BaseMap", albedo);
                    material.SetTexture("_MainTex", albedo);
                }
                if (normal != null)
                    material.SetTexture("_BumpMap", normal);
                if (metal != null)
                    material.SetTexture("_MetallicGlossMap", metal);
                if (oclusion != null)
                    material.SetTexture("_OcclusionMap", oclusion);
                if (emision != null)
                    material.SetTexture("_EmissionMap", emision);
                if (material.HasProperty("_BaseColor"))
                    material.SetColor("_BaseColor", color);
                EditorUtility.SetDirty(material);
                cambio = true;
                Debug.Log("[VR-QA] Material pasado a URP Lit: " + ruta);
            }
        }

        if (!cambio)
            return;

        AssetDatabase.SaveAssets();
        Debug.Log("[VR-QA] Materiales URP guardados con AssetDatabase.SaveAssets.");
    }

    private static bool EsShaderRoto(string nombre)
    {
        if (string.IsNullOrEmpty(nombre))
            return true;
        return nombre == "Standard"
            || nombre == "Hidden/InternalErrorShader"
            || nombre.StartsWith("Legacy Shaders/")
            || nombre == "Standard (Specular setup)";
    }
}
