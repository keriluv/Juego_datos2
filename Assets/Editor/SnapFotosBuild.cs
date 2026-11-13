using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Temporal: asigna las fotos nuevas a perfiles, historias, mapa y spotlight.
/// Se ejecuta si existe Library/SnapFotosBuild.txt.
/// </summary>
[InitializeOnLoad]
public static class SnapFotosBuild
{
    private const string MarkerPath = "Library/SnapFotosBuild.txt";
    private const string ScenePath = "Assets/LevelSnapchat/Scenes/LevelSnapchat.unity";
    private const string SpritesDir = "Assets/LevelSnapchat/Sprites/";

    static SnapFotosBuild()
    {
        EditorApplication.update += Tick;
    }

    private static void Tick()
    {
        if (!File.Exists(MarkerPath)) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;

        EditorApplication.update -= Tick;
        File.Delete(MarkerPath);

        try { Build(); }
        catch (System.Exception e) { Debug.LogError("SNAP_FOTOS_ERROR " + e); }
    }

    private static void Build()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != ScenePath) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        AsegurarSprite("Perfil_Andy");
        AsegurarSprite("Perfil_Dani");
        AsegurarSprite("Perfil_Raul");
        AsegurarSprite("Perfil_Keren");

        Sprite andy = Load("Perfil_Andy");
        Sprite dani = Load("Perfil_Dani");
        Sprite raul = Load("Perfil_Raul");
        Sprite keren = Load("Perfil_Keren");
        Sprite nico = Load("nico");
        Sprite gaby = Load("gaby");
        Sprite generico = Load("Avatar");

        if (andy == null || dani == null || raul == null || keren == null ||
            nico == null || gaby == null || generico == null)
        {
            Debug.LogError("SNAP_FOTOS_ERROR faltan sprites");
            return;
        }

        Canvas canvas = FindAppCanvas();
        if (canvas == null) { Debug.LogError("SNAP_FOTOS_ERROR sin canvas"); return; }

        Transform raiz = canvas.transform;

        // Perfil de la cabecera del chat (por defecto Nicole)
        Asignar(raiz, "Main/Header/Chat_Cabecera/Avatar/Interior", andy);

        // Pines del mapa
        Asignar(raiz, "Main/Body/Mapa_Panel/Mapa_Lienzo/Pin_Nicole/Burbuja/Avatar", andy);
        Asignar(raiz, "Main/Body/Mapa_Panel/Mapa_Lienzo/Pin_Daniel/Burbuja/Avatar", dani);

        // Autores de las historias
        Asignar(raiz, "Main/Body/Historias_Panel/Historia_PLAYA/Avatar/Interior", andy);
        Asignar(raiz, "Main/Body/Historias_Panel/Historia_FUEGO/Avatar/Interior", dani);

        // Autores del spotlight
        Asignar(raiz, "Main/Body/Spotlight_Panel/Post_0/Avatar/Interior", andy);
        Asignar(raiz, "Main/Body/Spotlight_Panel/Post_1/Avatar/Interior", dani);
        Asignar(raiz, "Main/Body/Spotlight_Panel/Post_4/Avatar/Interior", raul);
        Asignar(raiz, "Main/Body/Spotlight_Panel/Post_5/Avatar/Interior", keren);

        // Contenido de las historias
        FotoHistoria(raiz, "Historia_PLAYA", nico);
        FotoHistoria(raiz, "Historia_FUEGO", gaby);

        Titulo(raiz, "Historia_PLAYA", "NICOLE");
        Titulo(raiz, "Historia_FUEGO", "HALLOWEEN");

        SnapWebApp app = Object.FindFirstObjectByType<SnapWebApp>();
        if (app == null) { Debug.LogError("SNAP_FOTOS_ERROR sin SnapWebApp"); return; }

        Transform cabecera = raiz.Find("Main/Header/Chat_Cabecera/Avatar/Interior");
        app.chatFoto = cabecera != null ? cabecera.GetComponent<Image>() : null;
        app.avatarGenerico = generico;
        app.fotos = new Sprite[7];
        app.fotos[0] = andy;
        app.fotos[1] = dani;
        app.fotos[4] = raul;
        app.fotos[5] = keren;

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        Debug.Log("SNAP_FOTOS_OK perfiles=4 historias=2");
    }

    private static void Asignar(Transform raiz, string ruta, Sprite sprite)
    {
        Transform t = raiz.Find(ruta);
        if (t == null) { Debug.LogWarning("SNAP_FOTOS_WARN falta " + ruta); return; }

        Image img = t.GetComponent<Image>();
        if (img == null) { Debug.LogWarning("SNAP_FOTOS_WARN sin Image " + ruta); return; }

        img.sprite = sprite;
        img.type = Image.Type.Simple;
        img.color = Color.white;
        img.preserveAspect = true;
        img.raycastTarget = false;
    }

    private static void FotoHistoria(Transform raiz, string nombre, Sprite sprite)
    {
        Transform t = raiz.Find("Main/Body/Historias_Panel/" + nombre + "/Foto");
        if (t == null) { Debug.LogWarning("SNAP_FOTOS_WARN falta foto " + nombre); return; }

        RectTransform rt = t as RectTransform;
        Image img = t.GetComponent<Image>();
        if (rt == null || img == null) return;

        float escala = Mathf.Max(452f / sprite.rect.width, 956f / sprite.rect.height);
        rt.sizeDelta = new Vector2(sprite.rect.width * escala, sprite.rect.height * escala);

        img.sprite = sprite;
        img.type = Image.Type.Simple;
        img.color = Color.white;
        img.preserveAspect = false;
        img.raycastTarget = false;
    }

    private static void Titulo(Transform raiz, string nombre, string texto)
    {
        Transform t = raiz.Find("Main/Body/Historias_Panel/" + nombre + "/Etiqueta");
        if (t == null) return;

        TextMeshProUGUI tmp = t.GetComponent<TextMeshProUGUI>();
        if (tmp != null) tmp.text = texto;
    }

    private static Sprite Load(string name)
    {
        string path = SpritesDir + name + ".png";
        if (!File.Exists(path)) path = SpritesDir + name + ".jpg";

        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite != null) return sprite;

        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
        for (int i = 0; i < assets.Length; i++)
        {
            sprite = assets[i] as Sprite;
            if (sprite != null) return sprite;
        }

        Debug.LogWarning("SNAP_FOTOS_WARN sprite no cargado: " + name);
        return null;
    }

    private static void AsegurarSprite(string nombre)
    {
        string path = SpritesDir + nombre + ".png";
        if (!File.Exists(path)) return;

        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);

        TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti == null) return;

        if (ti.textureType != TextureImporterType.Sprite || !ti.alphaIsTransparency ||
            ti.spriteImportMode != SpriteImportMode.Single)
        {
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = 100f;
            ti.alphaIsTransparency = true;
            ti.mipmapEnabled = false;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.SaveAndReimport();
        }
    }

    private static Canvas FindAppCanvas()
    {
        Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        for (int i = 0; i < canvases.Length; i++)
        {
            CanvasScaler scaler = canvases[i].GetComponent<CanvasScaler>();
            if (scaler == null) continue;
            if (Mathf.Approximately(scaler.referenceResolution.x, 1920f)) return canvases[i];
        }
        return canvases.Length > 0 ? canvases[0] : null;
    }
}
