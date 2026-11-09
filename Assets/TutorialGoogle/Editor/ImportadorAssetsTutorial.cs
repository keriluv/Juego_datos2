using UnityEditor;
using UnityEngine;

public static class ImportadorAssetsTutorial
{
    private const string Carpeta = "Assets/TutorialGoogle";

    [MenuItem("Tools/Tutorial Google/Importar assets")]
    public static void Importar()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { Carpeta });

        foreach (string guid in guids)
        {
            string ruta = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter imp = AssetImporter.GetAtPath(ruta) as TextureImporter;
            if (imp == null) continue;

            TextureImporterSettings ajustes = new TextureImporterSettings();
            imp.ReadTextureSettings(ajustes);
            ajustes.spriteMeshType = SpriteMeshType.FullRect;
            imp.SetTextureSettings(ajustes);

            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.spritePixelsPerUnit = 100f;
            imp.alphaIsTransparency = true;
            imp.mipmapEnabled = false;
            imp.wrapMode = TextureWrapMode.Clamp;
            imp.textureCompression = TextureImporterCompression.Uncompressed;

            string nombre = System.IO.Path.GetFileNameWithoutExtension(ruta);
            imp.filterMode = nombre == "Dino_Chrome" ? FilterMode.Point : FilterMode.Bilinear;

            if (nombre.StartsWith("Kenney_BotonRedondo"))
                imp.spriteBorder = new Vector4(20f, 20f, 20f, 20f);
            else if (nombre.StartsWith("Kenney_Boton") || nombre.StartsWith("Kenney_Profundo"))
                imp.spriteBorder = new Vector4(24f, 24f, 24f, 24f);
            else if (nombre.StartsWith("Kenney_Input"))
                imp.spriteBorder = new Vector4(18f, 18f, 18f, 18f);
            else if (nombre.StartsWith("Kenney_Slider"))
                imp.spriteBorder = new Vector4(12f, 12f, 12f, 12f);
            else
                imp.spriteBorder = Vector4.zero;

            imp.SaveAndReimport();
        }

        AssetDatabase.Refresh();
        Debug.Log("TUTORIAL_IMPORT_OK: sprites configurados (" + guids.Length + ")");
    }
}
