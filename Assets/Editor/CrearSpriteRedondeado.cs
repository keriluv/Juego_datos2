using System.IO;
using UnityEditor;
using UnityEngine;

public static class CrearSpriteRedondeado
{
    [MenuItem("Tools/Crear Sprite Redondeado")]
    static void Crear()
    {
        int size = 64;
        float radio = 24f;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float cx = Mathf.Clamp(x + 0.5f, radio, size - radio);
                float cy = Mathf.Clamp(y + 0.5f, radio, size - radio);
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(cx, cy));
                float a = Mathf.Clamp01(radio - d + 0.5f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }

        string ruta = "Assets/sprites/RectanguloRedondeado.png";
        File.WriteAllBytes(ruta, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(ruta);

        var imp = (TextureImporter)AssetImporter.GetAtPath(ruta);
        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Single;
        imp.spriteBorder = new Vector4(26, 26, 26, 26);
        imp.alphaIsTransparency = true;
        imp.mipmapEnabled = false;
        imp.filterMode = FilterMode.Bilinear;
        imp.SaveAndReimport();

        Debug.Log("Sprite creado en " + ruta);
    }
}