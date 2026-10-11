using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Materiales URP compartidos. Reutilizar el mismo material baja los draw calls en VR.
/// </summary>
public static class MaterialBarco
{
    private static Shader shader;
    private static Shader shaderSinLuz;
    private static readonly Dictionary<int, Material> cache = new Dictionary<int, Material>();

    public static Material Crear(Color color)
    {
        int clave = Clave(color);
        if (cache.TryGetValue(clave, out Material existente))
            return existente;

        if (shader == null)
        {
            shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Standard");
        }

        Material material = new Material(shader);
        Pintar(material, color);
        if (material.HasProperty("_Smoothness"))
            material.SetFloat("_Smoothness", 0.16f);
        cache[clave] = material;
        return material;
    }

    public static Material CrearSinLuz(Color color)
    {
        int clave = Clave(color) ^ 0x51A7;
        if (cache.TryGetValue(clave, out Material existente))
            return existente;

        if (shaderSinLuz == null)
        {
            shaderSinLuz = Shader.Find("Universal Render Pipeline/Unlit");
            if (shaderSinLuz == null)
                shaderSinLuz = Shader.Find("Unlit/Color");
        }

        Material material = new Material(shaderSinLuz);
        Pintar(material, color);
        cache[clave] = material;
        return material;
    }

    private static void Pintar(Material material, Color color)
    {
        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Color"))
            material.SetColor("_Color", color);
        material.color = color;
    }

    private static int Clave(Color color)
    {
        return ((int)(color.r * 255f) << 24)
            | ((int)(color.g * 255f) << 16)
            | ((int)(color.b * 255f) << 8)
            | (int)(color.a * 255f);
    }
}
