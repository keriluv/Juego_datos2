using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SnapWebApp : MonoBehaviour
{
    [Header("Secciones")]
    public GameObject[] secciones = new GameObject[5];
    public Button[] botonesNav = new Button[5];
    public Image[] fondosNav = new Image[5];
    public TextMeshProUGUI[] textosNav = new TextMeshProUGUI[5];
    public string[] titulos = { "Chat", "Camara", "Historias", "Mapa", "Spotlight" };

    [Header("Cabecera")]
    public TextMeshProUGUI tituloSeccion;
    public TMP_InputField buscador;
    public GameObject chatCabecera;

    [Header("Chat")]
    public Transform listaChats;
    public Image chatAvatar;
    public Image chatFoto;
    public Sprite avatarGenerico;
    public Sprite[] fotos;
    public TextMeshProUGUI chatNombre;
    public TextMeshProUGUI chatEstado;
    public Image chatPunto;
    public ScrollRect chatScroll;
    public RectTransform chatContenido;
    public TMP_InputField chatInput;
    public Button botonEnviar;
    public Button botonLlamar;
    public Button botonCompartir;
    public Button botonNuevo;
    public Button botonCamaraChat;
    public Button botonFlash;
    public Button botonTimer;

    [Header("Camara")]
    public RawImage vistaCamara;
    public TextMeshProUGUI estadoCamara;
    public Button botonCaptura;
    public Image destello;

    [Header("Perfil")]
    public Button botonSalir;

    [Header("Historias")]
    public Button[] botonesHistoria = new Button[0];
    public GestorDesafiosSnap gestorDesafios;

    [Header("Estilo")]
    public Sprite spriteRedondeado;
    public TMP_FontAsset fuente;

    private static readonly Color Amarillo = new Color(1f, 0.988f, 0f, 1f);
    private static readonly Color Tinta = new Color(0.043f, 0.043f, 0.059f, 1f);
    private static readonly Color Gris = new Color(0.478f, 0.478f, 0.522f, 1f);
    private static readonly Color Verde = new Color(0.13f, 0.77f, 0.37f, 1f);
    private static readonly Color Ajeno = new Color(0.91f, 0.15f, 0.17f, 1f);
    private static readonly Color Propio = new Color(0.69f, 0.16f, 0.24f, 1f);

    private static readonly string[] Nombres =
    {
        "Nicole", "Daniel", "Joseph", "Harry", "Raul", "Keren", "Jesus"
    };

    private static readonly string[] Horas =
    {
        "12:04", "11:48", "Ayer", "10:12", "Ayer", "Lun", "Dom"
    };

    private static readonly string[] Avances =
    {
        "Nos vemos luegooo",
        "XD",
        "Viste la historia?",
        "Racha de 7 snaps",
        "Nos vemos manana",
        "Subi una historia",
        "Buenas! Que tal"
    };

    private static readonly int[] NoLeidos = { 2, 0, 1, 3, 0, 0, 0 };
    private static readonly bool[] EnLinea = { true, true, false, true, false, true, false };

    private static readonly Color[] Colores =
    {
        new Color(0.96f, 0.55f, 0.42f, 1f),
        new Color(0.42f, 0.66f, 0.96f, 1f),
        new Color(0.60f, 0.50f, 0.92f, 1f),
        new Color(0.36f, 0.78f, 0.66f, 1f),
        new Color(0.94f, 0.72f, 0.32f, 1f),
        new Color(0.92f, 0.52f, 0.72f, 1f),
        new Color(0.55f, 0.62f, 0.70f, 1f)
    };

    private static readonly string[][] Guiones =
    {
        new[]
        {
            "Hola, Viste mi historia?",
            "Si! Esa foto quedo increible",
            "Jaja te adoro",
            "Nos vemos luegooo"
        },
        new[]
        {
            "Ya voy para la U",
            "Dale me avisas cuando llegues",
            "No",
            "-_-"
        },
        new[]
        {
            "Viste la foto que subi anoche?",
            "Esa foto nocturna quedo increible",
            "Gracias! jhashs",
            ":)"
        },
        new[]
        {
            "Llevamos 7 snaps de racha",
            "No la rompamos ahora!",
            "Tranqui, te mando uno antes de dormir",
            "Hecho, racha asegurada"
        },
        new[]
        {
            "Nos vemos manana en clase",
            "Si, llevo los apuntes de la ultima sesion",
            "Gracias! Me salvas el examen",
            "Nada hombre, para eso estamos"
        },
        new[]
        {
            "Subi una historia nueva",
            "Ya la vi, muy buena la edicion",
            "Me alegra que te guste, probe filtros nuevos",
            "Quedan genial, sigue asi"
        },
        new[]
        {
            "Buenas! Que tal todo?",
            "Todo bien, y tu?",
            "Igual, aqui andamos con el proyecto",
            "Si necesitas una mano avisame"
        }
    };

    private int conversacionActual;
    private WebCamTexture webcam;
    private Coroutine rutinaDestello;
    private bool flashActivo;
    private bool temporizadorActivo;
    private Image fondoPrincipal;
    private Color colorFondoPrincipal;
    private bool fondoPrincipalInicializado;
    private void Start()
    {
        Transform raizVentana = transform.parent;
        Transform panelPrincipal = raizVentana != null ? raizVentana.Find("Main") : null;
        fondoPrincipal = panelPrincipal != null ? panelPrincipal.GetComponent<Image>() : null;
        if (fondoPrincipal != null)
        {
            colorFondoPrincipal = fondoPrincipal.color;
            fondoPrincipalInicializado = true;
        }

        ConfigurarNavegacion();
        ConfigurarConversaciones();
        ConfigurarHistorias();
        PintarBarraLateral();
        AplicarFotosLista();

        if (buscador != null) buscador.onValueChanged.AddListener(FiltrarConversaciones);
        if (botonEnviar != null) botonEnviar.onClick.AddListener(EnviarMensaje);
        if (chatInput != null) chatInput.onSubmit.AddListener(_ => EnviarMensaje());
        if (botonCaptura != null) botonCaptura.onClick.AddListener(Capturar);
        if (botonSalir != null) botonSalir.onClick.AddListener(Salir);
        if (botonLlamar != null) botonLlamar.onClick.AddListener(Llamar);
        if (botonCompartir != null) botonCompartir.onClick.AddListener(CompartirUbicacion);
        if (botonNuevo != null) botonNuevo.onClick.AddListener(NuevoChat);
        if (botonCamaraChat != null) botonCamaraChat.onClick.AddListener(() => MostrarSeccion(1));
        if (botonFlash != null) botonFlash.onClick.AddListener(AlternarFlash);
        if (botonTimer != null) botonTimer.onClick.AddListener(AlternarTemporizador);

        MostrarSeccion(0);
        SeleccionarConversacion(0);
    }

    private void OnDisable()
    {
        DetenerCamara();
    }

    private void ConfigurarNavegacion()
    {
        if (botonesNav == null) return;

        for (int i = 0; i < botonesNav.Length; i++)
        {
            if (botonesNav[i] == null) continue;
            int indice = i;
            botonesNav[i].onClick.AddListener(() => MostrarSeccion(indice));
        }
    }

    public void MostrarSeccion(int indice)
    {
        if (secciones == null || secciones.Length == 0) return;

        if (fondoPrincipalInicializado && fondoPrincipal != null)
            fondoPrincipal.color = indice == 2
                ? new Color(0.025f, 0.035f, 0.065f, 1f)
                : colorFondoPrincipal;

        for (int i = 0; i < secciones.Length; i++)
        {
            if (secciones[i] != null) secciones[i].SetActive(i == indice);

            bool activo = i == indice;
            if (fondosNav != null && i < fondosNav.Length && fondosNav[i] != null)
                fondosNav[i].color = activo ? Amarillo : new Color(1f, 1f, 1f, 0f);
            if (textosNav != null && i < textosNav.Length && textosNav[i] != null)
                textosNav[i].color = activo ? Tinta : Gris;
            if (botonesNav != null && i < botonesNav.Length && botonesNav[i] != null)
            {
                ColorBlock cb = botonesNav[i].colors;
                cb.normalColor = activo ? Amarillo : new Color(1f, 1f, 1f, 0f);
                cb.selectedColor = cb.normalColor;
                botonesNav[i].colors = cb;
            }
        }

        if (tituloSeccion != null && titulos != null && indice < titulos.Length)
        {
            tituloSeccion.text = titulos[indice];
            tituloSeccion.gameObject.SetActive(indice != 0);
        }

        if (chatCabecera != null) chatCabecera.SetActive(indice == 0);

        ActualizarCamara(indice == 1);
    }

    private void ConfigurarHistorias()
    {
        if (botonesHistoria == null) return;

        for (int i = 0; i < botonesHistoria.Length; i++)
        {
            if (botonesHistoria[i] == null) continue;
            int indice = i;
            botonesHistoria[i].onClick.AddListener(() => AbrirHistoria(indice));
        }
    }

    public void AbrirHistoria(int indice)
    {
        if (indice < 0 || indice >= Nombres.Length) return;

        if (RedSnapchat.HayRed && !RedSnapchat.EsAnfitrion) return;

        if (gestorDesafios != null)
        {
            gestorDesafios.IniciarEnHistoria(indice);
            return;
        }

        MostrarSeccion(0);
        SeleccionarConversacion(indice);
        Debug.Log("SNAP_WEB_HISTORIA abierta de " + Nombres[indice]);
    }

    public void Llamar()
    {
        Debug.Log("SNAP_WEB_LLAMADA con " + Nombres[conversacionActual]);
        MostrarSeccion(0);
        AnadirMensaje(false, "Llamada de voz iniciada...");
        AjustarAlturaMensajes();
        StartCoroutine(IrAlFinal());
    }

    public void CompartirUbicacion()
    {
        Debug.Log("SNAP_WEB_UBICACION compartida");
        MostrarSeccion(0);
        AnadirMensaje(true, "He compartido mi ubicacion contigo");
        AjustarAlturaMensajes();
        StartCoroutine(IrAlFinal());
        ActualizarUltimoMensaje("Tu: Ubicacion compartida");
    }

    private void ConfigurarConversaciones()
    {
        if (listaChats == null) return;

        for (int i = 0; i < listaChats.childCount; i++)
        {
            Transform fila = listaChats.GetChild(i);
            Button boton = fila.GetComponent<Button>();
            if (boton == null) continue;

            int indice = i;
            boton.onClick.AddListener(() => SeleccionarConversacion(indice));
        }
    }

    private void AplicarFotosLista()
    {
        if (listaChats == null || fotos == null) return;

        int total = Mathf.Min(listaChats.childCount, fotos.Length);
        for (int i = 0; i < total; i++)
        {
            if (fotos[i] == null) continue;

            Transform interior = listaChats.GetChild(i).Find("Avatar/Interior");
            if (interior == null) continue;

            Image img = interior.GetComponent<Image>();
            if (img == null) continue;

            img.sprite = fotos[i];
            img.color = Color.white;
            img.preserveAspect = true;
        }
    }

    private void PintarBarraLateral()
    {
        if (listaChats == null) return;

        for (int i = 0; i < listaChats.childCount && i < Nombres.Length; i++)
        {
            Transform fila = listaChats.GetChild(i);

            Image avatar = BuscarImagen(fila, "Avatar");
            if (avatar != null) avatar.color = Colores[i];

            Transform insignia = fila.Find("NoLeidos");
            if (insignia != null)
            {
                bool mostrar = NoLeidos[i] > 0;
                insignia.gameObject.SetActive(mostrar);

                TextMeshProUGUI texto = insignia.GetComponentInChildren<TextMeshProUGUI>();
                if (texto != null) texto.text = NoLeidos[i].ToString();
            }

            Transform punto = fila.Find("Punto");
            if (punto != null) punto.gameObject.SetActive(EnLinea[i]);
        }
    }

    public void SeleccionarConversacion(int indice)
    {
        if (indice < 0 || indice >= Nombres.Length) return;

        conversacionActual = indice;

        if (chatNombre != null) chatNombre.text = Nombres[indice];
        if (chatEstado != null) chatEstado.text = EnLinea[indice] ? "En linea" : "Desconectado";
        if (chatPunto != null) chatPunto.color = EnLinea[indice] ? Verde : new Color(0.72f, 0.72f, 0.76f, 1f);
        if (chatAvatar != null) chatAvatar.color = Colores[indice];

        if (chatFoto != null)
        {
            Sprite foto = (fotos != null && indice < fotos.Length) ? fotos[indice] : null;
            chatFoto.sprite = foto != null ? foto : avatarGenerico;
            chatFoto.color = foto != null ? Color.white : Colores[indice];
            chatFoto.preserveAspect = true;
        }

        if (listaChats != null)
        {
            for (int i = 0; i < listaChats.childCount; i++)
            {
                Transform fila = listaChats.GetChild(i);

                if (i == indice)
                {
                    Transform insignia = fila.Find("NoLeidos");
                    if (insignia != null) insignia.gameObject.SetActive(false);
                }

                Button boton = fila.GetComponent<Button>();
                if (boton == null) continue;

                bool sel = i == indice;
                ColorBlock cb = boton.colors;
                cb.normalColor = sel ? new Color(0.945f, 0.945f, 0.957f, 1f) : Color.white;
                cb.selectedColor = cb.normalColor;
                boton.colors = cb;
            }
        }

        ReconstruirMensajes();
    }

    private void ReconstruirMensajes()
    {
        if (chatContenido == null) return;

        for (int i = chatContenido.childCount - 1; i >= 0; i--)
        {
            Destroy(chatContenido.GetChild(i).gameObject);
        }

        alturaMensajes = 0f;

        string[] guion = Guiones[conversacionActual];
        for (int i = 0; i < guion.Length; i++)
        {
            AnadirMensaje(i % 2 == 1, guion[i]);
        }

        AjustarAlturaMensajes();
        StartCoroutine(IrAlFinal());
    }

    private float alturaMensajes;

    private void AnadirMensaje(bool mio, string contenido)
    {
        if (chatContenido == null || fuente == null) return;

        Color acento = mio ? Propio : Ajeno;
        const float anchoTexto = 800f;

        RectTransform raiz = Nuevo(mio ? "Mio" : "Otro", chatContenido);
        raiz.anchorMin = new Vector2(0f, 1f);
        raiz.anchorMax = new Vector2(1f, 1f);
        raiz.pivot = new Vector2(0.5f, 1f);
        raiz.anchoredPosition = new Vector2(0f, -alturaMensajes);

        RectTransform etiquetaRt = Nuevo("Autor", raiz);
        etiquetaRt.anchorMin = new Vector2(0f, 1f);
        etiquetaRt.anchorMax = new Vector2(0f, 1f);
        etiquetaRt.pivot = new Vector2(0f, 1f);
        etiquetaRt.anchoredPosition = Vector2.zero;
        etiquetaRt.sizeDelta = new Vector2(420f, 20f);

        TextMeshProUGUI etiqueta = etiquetaRt.gameObject.AddComponent<TextMeshProUGUI>();
        etiqueta.font = fuente;
        etiqueta.fontSize = 12.5f;
        etiqueta.fontStyle = FontStyles.Bold;
        etiqueta.characterSpacing = 4f;
        etiqueta.color = acento;
        etiqueta.alignment = TextAlignmentOptions.TopLeft;
        etiqueta.textWrappingMode = TextWrappingModes.NoWrap;
        etiqueta.overflowMode = TextOverflowModes.Overflow;
        etiqueta.raycastTarget = false;
        etiqueta.margin = Vector4.zero;
        etiqueta.text = mio ? "ME" : Nombres[conversacionActual].ToUpperInvariant();

        RectTransform textoRt = Nuevo("Texto", raiz);
        textoRt.anchorMin = new Vector2(0f, 1f);
        textoRt.anchorMax = new Vector2(0f, 1f);
        textoRt.pivot = new Vector2(0f, 1f);
        textoRt.anchoredPosition = new Vector2(18f, -26f);
        textoRt.sizeDelta = new Vector2(anchoTexto, 30f);

        TextMeshProUGUI texto = textoRt.gameObject.AddComponent<TextMeshProUGUI>();
        texto.font = fuente;
        texto.fontSize = 19f;
        texto.color = Tinta;
        texto.alignment = TextAlignmentOptions.TopLeft;
        texto.textWrappingMode = TextWrappingModes.Normal;
        texto.overflowMode = TextOverflowModes.Overflow;
        texto.raycastTarget = false;
        texto.margin = Vector4.zero;
        texto.text = contenido;
        texto.ForceMeshUpdate();

        Vector2 preferido = texto.GetPreferredValues(contenido, anchoTexto, 0f);
        float alto = Mathf.Max(28f, preferido.y);
        textoRt.sizeDelta = new Vector2(anchoTexto, alto);

        RectTransform barra = Nuevo("Barra", raiz);
        barra.anchorMin = new Vector2(0f, 1f);
        barra.anchorMax = new Vector2(0f, 1f);
        barra.pivot = new Vector2(0f, 1f);
        barra.anchoredPosition = new Vector2(0f, -24f);
        barra.sizeDelta = new Vector2(3f, alto + 4f);

        Image barraImg = barra.gameObject.AddComponent<Image>();
        barraImg.color = acento;
        barraImg.raycastTarget = false;

        float alturaRaiz = alto + 32f;
        raiz.sizeDelta = new Vector2(0f, alturaRaiz);

        alturaMensajes += alturaRaiz + 18f;
    }

    private static RectTransform Nuevo(string nombre, Transform padre)
    {
        GameObject go = new GameObject(nombre, typeof(RectTransform));
        go.transform.SetParent(padre, false);
        go.layer = 5;
        return (RectTransform)go.transform;
    }

    private void AjustarAlturaMensajes()
    {
        if (chatContenido == null) return;
        chatContenido.sizeDelta = new Vector2(0f, Mathf.Max(alturaMensajes + 16f, 200f));
    }

    private IEnumerator IrAlFinal()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();
        if (chatScroll != null) chatScroll.verticalNormalizedPosition = 0f;
    }

    public void EnviarMensaje()
    {
        if (chatInput == null) return;

        string contenido = chatInput.text != null ? chatInput.text.Trim() : string.Empty;
        if (string.IsNullOrEmpty(contenido)) return;

        AnadirMensaje(true, contenido);
        AjustarAlturaMensajes();
        StartCoroutine(IrAlFinal());

        chatInput.text = string.Empty;
        chatInput.ActivateInputField();

        ActualizarUltimoMensaje("Tu: " + contenido);

        CancelInvoke(nameof(ResponderAutomatico));
        Invoke(nameof(ResponderAutomatico), 1.1f);
    }

    private void ResponderAutomatico()
    {
        string[] respuestas =
        {
            "Jajaja, en serio?",
            "Perfecto, quedamos asi",
            "Te mando un snap ahora",
            "Genial!",
            "Suena bien"
        };

        string respuesta = respuestas[Random.Range(0, respuestas.Length)];
        AnadirMensaje(false, respuesta);
        AjustarAlturaMensajes();
        StartCoroutine(IrAlFinal());
        ActualizarUltimoMensaje(respuesta);
    }

    private void ActualizarUltimoMensaje(string texto)
    {
        if (listaChats == null || conversacionActual >= listaChats.childCount) return;

        Transform fila = listaChats.GetChild(conversacionActual);
        Transform avance = fila.Find("Avance");
        if (avance != null)
        {
            TextMeshProUGUI tmp = avance.GetComponent<TextMeshProUGUI>();
            if (tmp != null) tmp.text = texto;
        }
    }

    private void FiltrarConversaciones(string consulta)
    {
        if (listaChats == null) return;

        string q = string.IsNullOrEmpty(consulta) ? string.Empty : consulta.Trim().ToLowerInvariant();

        for (int i = 0; i < listaChats.childCount; i++)
        {
            Transform fila = listaChats.GetChild(i);
            bool visible = q.Length == 0 || fila.name.ToLowerInvariant().Contains(q);
            fila.gameObject.SetActive(visible);
        }
    }

    private void ActualizarCamara(bool activa)
    {
        if (vistaCamara == null) return;

        if (!activa)
        {
            DetenerCamara();
            return;
        }

        if (WebCamTexture.devices != null && WebCamTexture.devices.Length == 0)
        {
            if (estadoCamara != null) estadoCamara.text = "No se detecto ninguna camara";
            vistaCamara.color = new Color(0.09f, 0.09f, 0.11f, 1f);
            return;
        }

        if (webcam == null)
        {
            string dispositivo = WebCamTexture.devices[0].name;
            webcam = new WebCamTexture(dispositivo, 1280, 720, 30);
        }

        if (!webcam.isPlaying) webcam.Play();

        vistaCamara.texture = webcam;
        vistaCamara.color = Color.white;

        if (estadoCamara != null) estadoCamara.text = "Camara lista";
    }

    private void DetenerCamara()
    {
        if (webcam != null && webcam.isPlaying) webcam.Stop();
    }

    private void Capturar()
    {
        if (temporizadorActivo) StartCoroutine(CapturaTemporizada());
        else CapturarAhora();
    }

    private IEnumerator CapturaTemporizada()
    {
        for (int i = 3; i >= 1; i--)
        {
            if (estadoCamara != null) estadoCamara.text = "Capturando en " + i + "...";
            yield return new WaitForSeconds(1f);
        }

        CapturarAhora();
    }

    private void CapturarAhora()
    {
        StartCoroutine(Destello());

        AnadirMensaje(true, "Snap enviado");
        AjustarAlturaMensajes();
        StartCoroutine(IrAlFinal());
        ActualizarUltimoMensaje("Tu: Snap enviado");

        Debug.Log("SNAP_WEB_CAPTURA snap enviado a " + Nombres[conversacionActual]);
    }

    public void AlternarFlash()
    {
        flashActivo = !flashActivo;
        EtiquetaChip(botonFlash, flashActivo ? "Flash ON" : "Flash");

        if (estadoCamara != null)
            estadoCamara.text = flashActivo ? "Flash activado" : "Camara lista";

        Debug.Log("SNAP_WEB_FLASH " + (flashActivo ? "on" : "off"));
    }

    public void AlternarTemporizador()
    {
        temporizadorActivo = !temporizadorActivo;
        EtiquetaChip(botonTimer, temporizadorActivo ? "Timer 3s" : "Timer");

        if (estadoCamara != null)
            estadoCamara.text = temporizadorActivo ? "Temporizador de 3 s" : "Camara lista";

        Debug.Log("SNAP_WEB_TIMER " + (temporizadorActivo ? "on" : "off"));
    }

    public void NuevoChat()
    {
        if (buscador != null)
        {
            buscador.text = string.Empty;
            buscador.ActivateInputField();
        }

        Debug.Log("SNAP_WEB_NUEVO_CHAT");
    }

    private static void EtiquetaChip(Button boton, string texto)
    {
        if (boton == null) return;

        Transform etiqueta = boton.transform.Find("Texto");
        if (etiqueta == null) return;

        TextMeshProUGUI tmp = etiqueta.GetComponent<TextMeshProUGUI>();
        if (tmp != null) tmp.text = texto;
    }

    private IEnumerator Destello()
    {
        if (destello == null) yield break;

        float intensidad = flashActivo ? 1f : 0.72f;

        destello.gameObject.SetActive(true);
        destello.color = new Color(1f, 1f, 1f, intensidad);

        float t = 0f;
        while (t < 0.35f)
        {
            t += Time.deltaTime;
            destello.color = new Color(1f, 1f, 1f, Mathf.Lerp(intensidad, 0f, t / 0.35f));
            yield return null;
        }

        destello.gameObject.SetActive(false);
    }

    private void Salir()
    {
        Debug.Log("SNAP_WEB_SALIR cerrando aplicacion");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private static Image BuscarImagen(Transform padre, string nombre)
    {
        Transform hijo = padre.Find(nombre);
        if (hijo == null) return null;
        return hijo.GetComponent<Image>();
    }
}
