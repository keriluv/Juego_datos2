using System.IO;
using System.Linq;
using System.Reflection;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class MontarMultijugadorJuego
{
    private const string RutaCapsule = "Assets/Capsule.prefab";
    private const string CarpetaResources = "Assets/Resources";
    private const string RutaPrefab = "Assets/Resources/JugadorRedJuego.prefab";

    [MenuItem("Herramientas/Preparar multijugador del juego")]
    public static void Ejecutar()
    {
        try
        {
            if (!AssetDatabase.IsValidFolder(CarpetaResources))
                AssetDatabase.CreateFolder("Assets", "Resources");

            GameObject origen = AssetDatabase.LoadAssetAtPath<GameObject>(RutaCapsule);

            if (origen == null)
                throw new System.Exception("No se encontró " + RutaCapsule);

            if (AssetDatabase.LoadAssetAtPath<GameObject>(RutaPrefab) != null)
                AssetDatabase.DeleteAsset(RutaPrefab);

            GameObject instancia = (GameObject)PrefabUtility.InstantiatePrefab(origen);
            instancia.name = "JugadorRedJuego";

            PrefabUtility.UnpackPrefabInstance(
                instancia, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

            var eventSystem = instancia.GetComponent<UnityEngine.EventSystems.EventSystem>();
            if (eventSystem != null)
                Object.DestroyImmediate(eventSystem);

            if (instancia.GetComponent<NetworkObject>() == null)
                instancia.AddComponent<NetworkObject>();

            NetworkTransform transformRed = instancia.GetComponent<NetworkTransform>();

            if (transformRed == null)
                transformRed = instancia.AddComponent<NetworkTransform>();

            transformRed.AuthorityMode = NetworkTransform.AuthorityModes.Owner;
            transformRed.Interpolate = true;
            transformRed.InLocalSpace = false;

            try { instancia.tag = "Player"; } catch { }

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instancia, RutaPrefab);
            Object.DestroyImmediate(instancia);

            NetworkObject red = prefab.GetComponent<NetworkObject>();
            MethodInfo validar = typeof(NetworkObject).GetMethod(
                "OnValidate", BindingFlags.Instance | BindingFlags.NonPublic);

            if (validar != null) validar.Invoke(red, null);

            EditorUtility.SetDirty(red);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(RutaPrefab, ImportAssetOptions.ForceUpdate);

            Debug.Log("MONTAJE_JUEGO_ONLINE_OK " + RutaPrefab);
        }
        catch (System.Exception e)
        {
            Debug.LogError("MONTAJE_JUEGO_ONLINE_ERROR " + e);
            throw;
        }
    }

    [MenuItem("Herramientas/Compilar juego con multijugador")]
    public static void Construir()
    {
        string carpeta = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "BuildMultijugador"));
        Directory.CreateDirectory(carpeta);
        string salida = Path.Combine(carpeta, "JuegoOnline.exe");

        string[] escenas = EditorBuildSettings.scenes
            .Where(escena => escena.enabled)
            .Select(escena => escena.path)
            .ToArray();

        if (escenas.Length == 0)
            throw new System.Exception("No hay escenas activas en Build Settings.");

        FullScreenMode modoAnterior = PlayerSettings.fullScreenMode;
        int anchoAnterior = PlayerSettings.defaultScreenWidth;
        int altoAnterior = PlayerSettings.defaultScreenHeight;
        bool redimensionableAnterior = PlayerSettings.resizableWindow;

        try
        {
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.defaultScreenWidth = 1100;
            PlayerSettings.defaultScreenHeight = 750;
            PlayerSettings.resizableWindow = true;

            var opciones = new BuildPlayerOptions
            {
                scenes = escenas,
                locationPathName = salida,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None,
            };

            BuildReport informe = BuildPipeline.BuildPlayer(opciones);

            if (informe.summary.result == BuildResult.Succeeded)
                Debug.Log("MONTAJE_JUEGO_ONLINE_BUILD_OK " + salida);
            else
                Debug.LogError("MONTAJE_JUEGO_ONLINE_BUILD_ERROR " + informe.summary.result);
        }
        finally
        {
            PlayerSettings.fullScreenMode = modoAnterior;
            PlayerSettings.defaultScreenWidth = anchoAnterior;
            PlayerSettings.defaultScreenHeight = altoAnterior;
            PlayerSettings.resizableWindow = redimensionableAnterior;
        }
    }
}
