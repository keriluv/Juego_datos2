using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public static class CreadorEscenaDino
{
    private const string Ruta = "Assets/Scenes/JuegoDinoChrome.unity";

    [InitializeOnLoadMethod]
    private static void CrearAlImportar()
    {
        EditorApplication.delayCall += () =>
        {
            if (!File.Exists(Ruta) && !EditorApplication.isPlayingOrWillChangePlaymode) Crear();
        };
    }

    [MenuItem("Tools/Tutorial Google/Crear continuación Dino")]
    public static void Crear()
    {
        Scene escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject camaraGo = new GameObject("Main Camera"); camaraGo.tag = "MainCamera";
        Camera camara = camaraGo.AddComponent<Camera>(); camara.clearFlags = CameraClearFlags.SolidColor; camara.backgroundColor = new Color(0.98f, 0.98f, 0.97f); camara.orthographic = true; camara.orthographicSize = 5;
        camaraGo.AddComponent<AudioListener>();
        GameObject juego = new GameObject("Juego Dino Chrome"); juego.AddComponent<JuegoDinoChrome>();
        EditorSceneManager.SaveScene(escena, Ruta);

        EditorBuildSettingsScene[] escenas = EditorBuildSettings.scenes;
        bool existe = false;
        foreach (EditorBuildSettingsScene item in escenas) if (item.path == Ruta) existe = true;
        if (!existe)
        {
            var nuevas = new EditorBuildSettingsScene[escenas.Length + 1];
            escenas.CopyTo(nuevas, 0); nuevas[escenas.Length] = new EditorBuildSettingsScene(Ruta, true); EditorBuildSettings.scenes = nuevas;
        }

        ActualizarEnlaceTutorial();
        AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        Debug.Log("DINO_CONTINUACION_OK: escena creada e integrada al tutorial.");
    }

    [MenuItem("Tools/Tutorial Google/Preparar Dino para editar en escena")]
    public static void PrepararParaEditar()
    {
        if (!File.Exists(Ruta))
        {
            Debug.LogError("No existe la escena del Dino: " + Ruta);
            return;
        }

        Scene escena = EditorSceneManager.OpenScene(Ruta, OpenSceneMode.Single);
        JuegoDinoChrome juego = Object.FindObjectOfType<JuegoDinoChrome>();
        if (juego == null)
        {
            Debug.LogError("No se encontró JuegoDinoChrome en la escena.");
            return;
        }

        juego.GenerarVistaEditable();
        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena);
        Debug.Log("DINO_EDITABLE_OK: Canvas, dino, suelo, nubes y HUD guardados en la escena.");
    }

    private static void ActualizarEnlaceTutorial()
    {
        string tutorial = "Assets/Scenes/TutorialGoogle.unity";
        if (File.Exists(tutorial))
        {
            Scene t = EditorSceneManager.OpenScene(tutorial, OpenSceneMode.Single);
            foreach (GameObject root in t.GetRootGameObjects())
            {
                Transform found = root.transform.Find("SafeArea/BotonSaltar");
                if (found == null) found = root.transform.Find("Canvas/SafeArea/BotonSaltar");
                if (found == null) continue;
                BotonSaltarTutorial seguir = found.GetComponent<BotonSaltarTutorial>();
                if (seguir == null) seguir = found.gameObject.AddComponent<BotonSaltarTutorial>();
                seguir.escena = "JuegoDinoChrome";
                TextMeshProUGUI texto = found.GetComponentInChildren<TextMeshProUGUI>();
                if (texto != null) texto.text = "Continuar: Dino Chrome";
                EditorSceneManager.MarkSceneDirty(t); EditorSceneManager.SaveScene(t); break;
            }
        }
    }
}
