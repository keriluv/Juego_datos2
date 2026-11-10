using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class AjusteGolpeSnapchat
{
    private const string RutaEscena = "Assets/LevelSnapchat/Scenes/LevelSnapchat.unity";
    private const string ClaveSesion = "Snapchat.AjusteGolpe";

    static AjusteGolpeSnapchat()
    {
        if (!SessionState.GetBool(ClaveSesion, false))
            EditorApplication.delayCall += Aplicar;

        EditorApplication.playModeStateChanged += estado =>
        {
            if (estado == PlayModeStateChange.EnteredEditMode && !SessionState.GetBool(ClaveSesion, false))
                EditorApplication.delayCall += Aplicar;
        };
    }

    [MenuItem("Tools/Snapchat/Ajustar golpe del jugador")]
    public static void AplicarMenu()
    {
        Aplicar(true);
    }

    private static void Aplicar()
    {
        Aplicar(false);
    }

    private static void Aplicar(bool forzar)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (BuildPipeline.isBuildingPlayer) return;
        if (!File.Exists(RutaEscena)) return;

        Scene escena = SceneManager.GetSceneByPath(RutaEscena);
        bool abierta = escena.IsValid() && escena.isLoaded;
        bool aditiva = false;

        if (!abierta)
        {
            escena = EditorSceneManager.OpenScene(RutaEscena, OpenSceneMode.Additive);
            aditiva = true;
        }

        if (!escena.IsValid()) return;

        bool cambiada = false;

        foreach (GameObject raiz in escena.GetRootGameObjects())
        {
            JugadorTutorial[] jugadores = raiz.GetComponentsInChildren<JugadorTutorial>(true);
            foreach (JugadorTutorial jugador in jugadores)
            {
                if (jugador.golpearPlataformaActual) continue;
                jugador.golpearPlataformaActual = true;
                cambiada = true;
            }
        }

        if (cambiada)
        {
            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena);
            Debug.Log("SNAP_GOLPE_OK: golpe sobre la plataforma actual activado.");
        }
        else if (forzar)
        {
            Debug.Log("SNAP_GOLPE: el jugador ya golpea la plataforma actual.");
        }

        if (aditiva)
            EditorSceneManager.CloseScene(escena, true);

        SessionState.SetBool(ClaveSesion, true);
    }
}
