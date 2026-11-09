using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class AplicarPendienteTutorial
{
    private const string RutaMarca = "Temp/tutorial_pendiente.txt";
    private const string RutaEscena = "Assets/Scenes/TutorialGoogle.unity";

    static AplicarPendienteTutorial()
    {
        if (!File.Exists(RutaMarca)) return;
        EditorApplication.delayCall += Aplicar;
    }

    private static void Aplicar()
    {
        if (!File.Exists(RutaMarca)) return;

        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.delayCall += Aplicar;
            return;
        }

        if (SceneManager.GetActiveScene().path != RutaEscena)
        {
            Debug.LogWarning("TUTORIAL_PENDIENTE: abre " + RutaEscena + " para aplicar los cambios.");
            return;
        }

        ImportadorAssetsTutorial.Importar();
        DecoradorEscenaTutorial.DecorarEscenaAbierta();

        File.Delete(RutaMarca);
        Debug.Log("TUTORIAL_AUTO_OK: cambios aplicados");
    }
}
