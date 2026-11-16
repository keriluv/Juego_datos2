using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Le pone a cada video la foto de su usuario y un botón "Seguir" / "Siguiendo" al lado del nombre.
// - Mira el texto de Texto_Usuario: cuando cambia el usuario, cambia la foto y el estado del botón.
// - Si un usuario no tiene foto asignada, se le crea una foto genérica de color (nunca un cuadro blanco).
// - El botón se crea solo, no hay que dibujarlo. Seguir -> rojo; Siguiendo -> gris.
public class PerfilUsuarioFeed : MonoBehaviour
{
    [Serializable]
    public class Perfil
    {
        public string usuario;   // igual que en la lista de usuarios del feed, por ejemplo @RaulArmy777
        public Sprite foto;      // su foto de perfil
    }

    [Header("Elementos de la escena")]
    [SerializeField] TMP_Text textoUsuario;        // Texto_Usuario (el nombre del usuario)
    [SerializeField] RectTransform iconoUsuario;   // Icono_Usuario (el círculo donde va la foto)
    [SerializeField] GameObject fotoAnterior;      // opcional: el cuadro blanco viejo, se oculta

    [Header("Foto de cada usuario")]
    [SerializeField] Perfil[] perfiles;

    [Header("Botón Seguir")]
    [SerializeField] string textoSeguir = "Seguir";
    [SerializeField] string textoSiguiendo = "Siguiendo";
    [SerializeField] Color colorSeguir = new Color32(254, 44, 85, 255);
    [SerializeField] Color colorSiguiendo = new Color32(88, 88, 94, 235);
    [SerializeField] float altoBoton = 34f;       // alto del botón
    [SerializeField] float tamanoFuente = 18f;    // tamaño de la letra (que combine con el nombre)
    [SerializeField] float margen = 14f;          // separación entre el nombre y el botón

    Image fotoPerfil;
    AspectRatioFitter ajuste;
    RectTransform boton;
    Image fondoBoton;
    TMP_Text textoBoton;

    string ultimoTexto;
    string claveActual = "";
    float pop;

    readonly HashSet<string> siguiendo = new HashSet<string>();
    readonly Dictionary<string, Sprite> avatares = new Dictionary<string, Sprite>();
    static Sprite pastilla;

    void Awake()
    {
        if (fotoAnterior != null) fotoAnterior.SetActive(false);
        CrearFoto();
        CrearBoton();
    }

    void Update()
    {
        if (textoUsuario == null || boton == null) return;

        // si cambió el usuario (cambió el video), actualiza foto y botón
        if (textoUsuario.text != ultimoTexto) Refrescar();

        // pequeño "apretón" del botón al hacer clic
        if (pop > 0f)
        {
            pop = Mathf.MoveTowards(pop, 0f, Time.deltaTime * 6f);
            boton.localScale = Vector3.one * (1f - 0.12f * Mathf.Sin(pop * Mathf.PI));
        }
    }

    // ---------- creación de los elementos ----------

    void CrearFoto()
    {
        if (iconoUsuario == null) return;

        var go = new GameObject("FotoPerfil", typeof(RectTransform), typeof(Image), typeof(AspectRatioFitter));
        go.layer = iconoUsuario.gameObject.layer;
        go.transform.SetParent(iconoUsuario, false);

        fotoPerfil = go.GetComponent<Image>();
        fotoPerfil.raycastTarget = false;

        // la foto llena el círculo sin deformarse (se recorta lo que sobra)
        ajuste = go.GetComponent<AspectRatioFitter>();
        ajuste.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        ajuste.aspectRatio = 1f;
    }

    void CrearBoton()
    {
        if (textoUsuario == null) return;
        if (pastilla == null) pastilla = CrearPastilla();

        var go = new GameObject("BotonSeguir", typeof(RectTransform), typeof(Image), typeof(Button));
        go.layer = textoUsuario.gameObject.layer;
        boton = (RectTransform)go.transform;
        boton.SetParent(textoUsuario.transform.parent, false);
        boton.anchorMin = boton.anchorMax = new Vector2(0.5f, 0.5f);
        boton.pivot = new Vector2(0f, 0.5f);

        fondoBoton = go.GetComponent<Image>();
        fondoBoton.sprite = pastilla;
        fondoBoton.type = Image.Type.Sliced;
        fondoBoton.pixelsPerUnitMultiplier = 64f / Mathf.Max(1f, altoBoton);   // extremos totalmente redondos

        var b = go.GetComponent<Button>();
        b.transition = Selectable.Transition.None;
        b.targetGraphic = fondoBoton;
        b.onClick.AddListener(Alternar);

        var tgo = new GameObject("Texto", typeof(RectTransform), typeof(TextMeshProUGUI));
        tgo.layer = go.layer;
        var trt = (RectTransform)tgo.transform;
        trt.SetParent(boton, false);
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;

        textoBoton = tgo.GetComponent<TextMeshProUGUI>();
        textoBoton.alignment = TextAlignmentOptions.Center;
        textoBoton.fontSize = tamanoFuente;
        textoBoton.fontStyle = FontStyles.Bold;
        textoBoton.color = Color.white;
        textoBoton.raycastTarget = false;

        go.SetActive(false);
    }

    // ---------- lógica ----------

    void Refrescar()
    {
        ultimoTexto = textoUsuario.text;
        claveActual = Normalizar(ultimoTexto);

        if (fotoPerfil != null)
        {
            Sprite s = BuscarFoto(claveActual);
            fotoPerfil.sprite = s;
            fotoPerfil.color = Color.white;
            if (ajuste != null && s != null) ajuste.aspectRatio = s.rect.width / Mathf.Max(1f, s.rect.height);
        }

        bool hayUsuario = claveActual.Length > 0;
        boton.gameObject.SetActive(hayUsuario);
        if (!hayUsuario) return;

        ActualizarVisual();
        Colocar();
    }

    void Alternar()
    {
        if (claveActual.Length == 0) return;

        if (!siguiendo.Add(claveActual)) siguiendo.Remove(claveActual);
        ActualizarVisual();
        pop = 1f;
    }

    void ActualizarVisual()
    {
        bool sigue = siguiendo.Contains(claveActual);
        textoBoton.text = sigue ? textoSiguiendo : textoSeguir;
        fondoBoton.color = sigue ? colorSiguiendo : colorSeguir;

        float ancho = textoBoton.GetPreferredValues(textoBoton.text).x + altoBoton * 0.9f;
        boton.sizeDelta = new Vector2(ancho, altoBoton);
    }

    // Pone el botón justo a la derecha del nombre, sea largo o corto
    void Colocar()
    {
        textoUsuario.ForceMeshUpdate();
        Bounds b = textoUsuario.textBounds;
        Vector3 derecha = textoUsuario.rectTransform.TransformPoint(new Vector3(b.max.x, b.center.y, 0f));
        float escala = textoUsuario.rectTransform.lossyScale.x;
        boton.position = new Vector3(derecha.x + margen * escala, derecha.y, derecha.z);
    }

    Sprite BuscarFoto(string clave)
    {
        if (perfiles != null)
        {
            foreach (var p in perfiles)
                if (p != null && p.foto != null && Normalizar(p.usuario) == clave)
                    return p.foto;
        }
        return AvatarGenerico(clave);
    }

    static string Normalizar(string s)
    {
        return string.IsNullOrEmpty(s) ? "" : s.Trim().TrimStart('@').ToLowerInvariant();
    }

    // ---------- imágenes creadas por código ----------

    // Foto genérica: fondo de color (según el nombre) con la silueta de una persona
    Sprite AvatarGenerico(string clave)
    {
        Sprite s;
        if (avatares.TryGetValue(clave, out s)) return s;

        int h = 7;
        foreach (char c in clave) h = h * 31 + c;
        float tono = (((h % 360) + 360) % 360) / 360f;
        Color fondo = Color.HSVToRGB(tono, 0.45f, 0.85f);
        Color silueta = Color.HSVToRGB(tono, 0.2f, 1f);

        const int N = 128;
        var tex = new Texture2D(N, N, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        var px = new Color32[N * N];

        for (int y = 0; y < N; y++)
        {
            for (int x = 0; x < N; x++)
            {
                float fx = x + 0.5f, fy = y + 0.5f;

                float dx = fx - 64f, dy = fy - 80f;
                bool cabeza = dx * dx + dy * dy < 24f * 24f;

                float ex = (fx - 64f) / 46f, ey = (fy - 8f) / 46f;
                bool hombros = ex * ex + ey * ey < 1f;

                px[y * N + x] = (cabeza || hombros) ? silueta : fondo;
            }
        }
        tex.SetPixels32(px);
        tex.Apply();

        s = Sprite.Create(tex, new Rect(0f, 0f, N, N), new Vector2(0.5f, 0.5f), 100f);
        avatares[clave] = s;
        return s;
    }

    // Pastilla blanca (rectángulo con extremos redondos) que se estira sin deformarse
    static Sprite CrearPastilla()
    {
        const int W = 128, H = 64;
        float r = H / 2f;

        var tex = new Texture2D(W, H, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        var px = new Color32[W * H];

        for (int y = 0; y < H; y++)
        {
            for (int x = 0; x < W; x++)
            {
                float cx = Mathf.Clamp(x + 0.5f, r, W - r);
                float dx = (x + 0.5f) - cx;
                float dy = (y + 0.5f) - r;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float a = Mathf.Clamp01(r - dist + 0.5f);
                px[y * W + x] = new Color32(255, 255, 255, (byte)(a * 255f));
            }
        }
        tex.SetPixels32(px);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0f, 0f, W, H), new Vector2(0.5f, 0.5f), 100f,
                             0, SpriteMeshType.FullRect, new Vector4(r, 0f, r, 0f));
    }
}
