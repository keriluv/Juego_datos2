using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class AsignarFlotaHolandesa
{
    private const string RutaModelo = "Assets/recurso de pirata/dutch-ship-medium/dutch-ship-medium.obj";
    private const string RutaEscena = "Assets/Scenes/EscenaVR.unity";

    static AsignarFlotaHolandesa()
    {
        EditorApplication.delayCall += Asignar;
    }

    private static void Asignar()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        GameObject modelo = AssetDatabase.LoadAssetAtPath<GameObject>(RutaModelo);
        if (modelo == null)
            return;

        Scene escena = SceneManager.GetSceneByPath(RutaEscena);
        bool abierta = escena.IsValid() && escena.isLoaded;
        if (!abierta)
            escena = EditorSceneManager.OpenScene(RutaEscena, OpenSceneMode.Additive);

        bool cambio = false;
        foreach (GameObject raiz in escena.GetRootGameObjects())
        {
            VRGameManager director = raiz.GetComponent<VRGameManager>();
            if (director == null)
                continue;

            SerializedObject serializado = new SerializedObject(director);
            SerializedProperty campo = serializado.FindProperty("modeloBarco");
            if (campo != null && campo.objectReferenceValue != modelo)
            {
                campo.objectReferenceValue = modelo;
                serializado.ApplyModifiedPropertiesWithoutUndo();
                cambio = true;
            }
        }

        if (cambio)
        {
            EditorSceneManager.SaveScene(escena);
            Debug.Log("EscenaVR: Modelo Barco quedó en dutch-ship-medium.");
        }

        if (!abierta)
            EditorSceneManager.CloseScene(escena, true);
    }
}
