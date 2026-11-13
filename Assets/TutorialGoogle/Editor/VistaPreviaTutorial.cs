using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class VistaPreviaTutorial
{
    private const string RutaEscena = "Assets/Scenes/TutorialGoogle.unity";
    private const string CarpetaSalida = @"C:\Users\paaoo\AppData\Local\Temp\opencode";

    public static void RenderizarPorBatch()
    {
        EditorSceneManager.OpenScene(RutaEscena, OpenSceneMode.Single);

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        Camera cam = Camera.main;
        if (canvas == null || cam == null)
        {
            Debug.LogError("TUTORIAL_PREVIEW: falta Canvas o camara");
            return;
        }

        RenderTexture rt = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32);
        rt.antiAliasing = 4;

        cam.targetTexture = rt;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.945f, 0.953f, 0.961f, 1f);
        cam.orthographic = true;
        cam.orthographicSize = 5.4f;
        cam.transform.position = new Vector3(0f, 0f, -10f);
        cam.transform.rotation = Quaternion.identity;
        cam.cullingMask = ~0;

        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = cam;
        canvas.planeDistance = 5f;
        Canvas.ForceUpdateCanvases();

        Guardar(cam, rt, "tutorial_google_1.png");

        GameObject panel = GameObject.Find("Canvas/SafeArea/PanelExtensiones");
        GameObject clippy = GameObject.Find("Canvas/SafeArea/Clippy");
        if (panel != null) panel.SetActive(true);
        if (clippy != null) clippy.SetActive(false);

        Canvas.ForceUpdateCanvases();
        Guardar(cam, rt, "tutorial_google_2.png");

        if (panel != null) panel.SetActive(false);
        if (clippy != null) clippy.SetActive(true);

        GameObject resultados = GameObject.Find("Canvas/SafeArea/PanelResultados");
        GameObject controles = GameObject.Find("Canvas/SafeArea/PanelControles");
        if (resultados != null) resultados.SetActive(true);
        if (controles != null) controles.SetActive(false);

        Canvas.ForceUpdateCanvases();
        Guardar(cam, rt, "tutorial_google_3.png");

        cam.targetTexture = null;
        rt.Release();
        Debug.Log("TUTORIAL_PREVIEW_OK");
    }

    private static void Guardar(Camera cam, RenderTexture rt, string archivo)
    {
        cam.Render();

        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(1920, 1080, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0f, 0f, 1920f, 1080f), 0, 0);
        tex.Apply();
        RenderTexture.active = null;

        File.WriteAllBytes(Path.Combine(CarpetaSalida, archivo), tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
    }
}
