#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;

public class GoogleVentanaStyle : EditorWindow
{
    [MenuItem("Google Dino/Aplicar Estilo Google a la Ventana", priority = 2)]
    public static void AplicarEstilo()
    {
        var escena = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (escena.name != "TutorialGoogle")
        {
            Debug.LogWarning("[Google Style] Abrí primero la escena TutorialGoogle.");
            return;
        }

        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            var go = new GameObject("Canvas");
            canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            go.AddComponent<CanvasScaler>();
            go.AddComponent<GraphicRaycaster>();
        }

        var fondo = GameObject.Find("FondoPagina") ?? new GameObject("FondoPagina");
        fondo.transform.SetParent(canvas.transform, false);
        var fondoImg = fondo.GetComponent<Image>() ?? fondo.AddComponent<Image>();
        fondoImg.color = new Color(0.975f, 0.975f, 0.975f, 1f);
        var fondoRect = fondo.GetComponent<RectTransform>();
        fondoRect.anchorMin = Vector2.zero;
        fondoRect.anchorMax = Vector2.one;
        fondoRect.offsetMin = Vector2.zero;
        fondoRect.offsetMax = Vector2.zero;
        fondoRect.SetSiblingIndex(0);

        var buscador = Object.FindObjectOfType<BuscadorGoogle>();
        if (buscador != null && buscador.campo != null)
        {
            var campoRect = buscador.campo.GetComponent<RectTransform>();
            var campoImg = buscador.campo.GetComponent<Image>() ?? buscador.campo.gameObject.AddComponent<Image>();
            campoImg.color = Color.white;
            var tmp = buscador.campo.GetComponent<TMP_InputField>();
            if (tmp != null)
            {
                tmp.textComponent.color = Color.black;
                tmp.placeholder.color = new Color(0.5f, 0.5f, 0.5f, 0.6f);
            }
            campoRect.anchorMin = new Vector2(0.5f, 1f);
            campoRect.anchorMax = new Vector2(0.5f, 1f);
            campoRect.pivot = new Vector2(0.5f, 1f);
            campoRect.anchoredPosition = new Vector2(0f, -60f);
            campoRect.sizeDelta = new Vector2(640f, 56f);
        }

        var panelLateral = new GameObject("PanelControlesLateral");
        panelLateral.transform.SetParent(canvas.transform, false);
        var panelRect = panelLateral.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1f, 0.5f);
        panelRect.anchorMax = new Vector2(1f, 0.5f);
        panelRect.pivot = new Vector2(1f, 0.5f);
        panelRect.anchoredPosition = new Vector2(-20f, 0f);
        panelRect.sizeDelta = new Vector2(260f, 420f);
        var panelImg = panelLateral.AddComponent<Image>();
        panelImg.color = new Color(1f, 1f, 1f, 0.9f);

        var btnSaltar = CrearBotonRedondo(panelRect, "BtnSaltar", new Vector2(0f, 120f), "SALTAR", Color.white, new Color(0.2f, 0.6f, 1f, 1f));
        btnSaltar.onClick.AddListener(() =>
        {
            var dino = Object.FindObjectOfType<JugadorTutorial>();
            dino?.Saltar();
        });

        var btnAgachar = CrearBotonRedondo(panelRect, "BtnAgachar", new Vector2(0f, 40f), "AGACHAR", Color.white, new Color(0.55f, 0.55f, 0.55f, 1f));
        bool agachando = false;
        btnAgachar.onClick.AddListener(() =>
        {
            agachando = !agachando;
            var dino = Object.FindObjectOfType<JugadorTutorial>();
            dino?.SetAgachar(agachando);
            var imgAgachar = btnAgachar.GetComponent<Image>();
            imgAgachar.color = agachando ? new Color(0.2f, 0.6f, 1f, 1f) : new Color(0.55f, 0.55f, 0.55f, 1f);
        });

        var txtControles = CrearTexto(panelRect, "TxtControles", new Vector2(0f, -60f), new Vector2(220f, 80f), "Click / Espacio\nGatillo RT-R2", 18, TextAlignmentOptions.Center);
        txtControles.color = new Color(0.35f, 0.35f, 0.35f, 1f);

        var interaccion = Object.FindObjectOfType<InteraccionTutorial>();
        if (interaccion != null && interaccion.panelExtensiones != null)
        {
            var extRect = interaccion.panelExtensiones.GetComponent<RectTransform>();
            var extImg = interaccion.panelExtensiones.GetComponent<Image>() ?? interaccion.panelExtensiones.AddComponent<Image>();
            extImg.color = new Color(1f, 1f, 1f, 0.95f);
            foreach (var fila in interaccion.filas)
            {
                if (fila == null) continue;
                var filaImg = fila.GetComponent<Image>() ?? fila.gameObject.AddComponent<Image>();
                filaImg.color = new Color(0.97f, 0.97f, 0.97f, 1f);
            }
        }

        Debug.Log("[Google Style] Estilo aplicado.");
        Selection.activeGameObject = panelLateral;
    }

    private static Button CrearBotonRedondo(Transform padre, string nombre, Vector2 pos, string texto, Color colorTexto, Color colorFondo)
    {
        var go = new GameObject(nombre);
        go.transform.SetParent(padre, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchoredPosition = pos;
        rect.sizeDelta = new Vector2(180f, 50f);
        var img = go.AddComponent<Image>();
        img.color = colorFondo;
        var btn = go.AddComponent<Button>();
        var nav = btn.navigation;
        nav.mode = Navigation.Mode.None;
        btn.navigation = nav;
        var txtGO = new GameObject("Texto");
        txtGO.transform.SetParent(go.transform, false);
        var tRect = txtGO.AddComponent<RectTransform>();
        tRect.anchorMin = Vector2.zero;
        tRect.anchorMax = Vector2.one;
        tRect.offsetMin = Vector2.zero;
        tRect.offsetMax = Vector2.zero;
        var tTmp = txtGO.AddComponent<TextMeshProUGUI>();
        tTmp.text = texto;
        tTmp.fontSize = 18;
        tTmp.alignment = TextAlignmentOptions.Center;
        tTmp.color = colorTexto;
        return btn;
    }

    private static TextMeshProUGUI CrearTexto(Transform padre, string nombre, Vector2 pos, Vector2 tamanio, string texto, float tam, TextAlignmentOptions alineacion)
    {
        var go = new GameObject(nombre);
        go.transform.SetParent(padre, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchoredPosition = pos;
        rect.sizeDelta = tamanio;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = texto;
        tmp.fontSize = tam;
        tmp.alignment = alineacion;
        tmp.color = Color.black;
        return tmp;
    }
}
#endif
