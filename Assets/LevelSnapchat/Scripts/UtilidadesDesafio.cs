using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public static class UtilidadesDesafio
{
    private static bool gatilloAnterior;
    private static float stickAnterior;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ReiniciarEstado()
    {
        gatilloAnterior = false;
        stickAnterior = 0f;
    }

    public static void ReiniciarEntrada()
    {
        gatilloAnterior = false;
        stickAnterior = 0f;
    }

    public static bool GolpePresionado()
    {
        bool click = Input.GetMouseButtonDown(0);
        bool gatillo = false;

        try
        {
            gatillo = Input.GetAxis("Fire3") > 0.5f;
        }
        catch
        {
            gatillo = false;
        }

#if ENABLE_INPUT_SYSTEM
        Mouse raton = Mouse.current;
        if (raton != null && raton.leftButton.wasPressedThisFrame)
            click = true;

        Gamepad mando = Gamepad.current;
        if (mando != null &&
            (mando.rightTrigger.wasPressedThisFrame || mando.leftTrigger.wasPressedThisFrame))
            gatillo = true;
#endif

        bool borde = gatillo && !gatilloAnterior;
        gatilloAnterior = gatillo;

        return click || borde;
    }

    public static int DireccionPresionada()
    {
        int dir = 0;

        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) dir -= 1;
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) dir += 1;

#if ENABLE_INPUT_SYSTEM
        Keyboard teclado = Keyboard.current;
        if (teclado != null)
        {
            if (teclado.aKey.wasPressedThisFrame || teclado.leftArrowKey.wasPressedThisFrame) dir -= 1;
            if (teclado.dKey.wasPressedThisFrame || teclado.rightArrowKey.wasPressedThisFrame) dir += 1;
        }

        Gamepad mando = Gamepad.current;
        if (mando != null)
        {
            if (mando.dpad.left.wasPressedThisFrame) dir -= 1;
            if (mando.dpad.right.wasPressedThisFrame) dir += 1;

            float eje = mando.leftStick.ReadValue().x;
            if (eje < -0.6f && stickAnterior >= -0.6f) dir -= 1;
            else if (eje > 0.6f && stickAnterior <= 0.6f) dir += 1;
            stickAnterior = eje;
        }
#endif

        return Mathf.Clamp(dir, -1, 1);
    }

    public static RectTransform CrearRect(string nombre, Transform padre)
    {
        GameObject go = new GameObject(nombre, typeof(RectTransform));
        go.layer = 5;
        RectTransform rt = (RectTransform)go.transform;
        rt.SetParent(padre, false);
        return rt;
    }

    public static Image CrearPanel(string nombre, Transform padre, Sprite sprite, Color color, bool sliced = true)
    {
        RectTransform rt = CrearRect(nombre, padre);
        Image img = rt.gameObject.AddComponent<Image>();
        img.sprite = sprite;
        img.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    public static TextMeshProUGUI CrearTexto(string nombre, Transform padre, TMP_FontAsset fuente,
        string texto, float tamano, Color color, TextAlignmentOptions alineacion = TextAlignmentOptions.Center)
    {
        RectTransform rt = CrearRect(nombre, padre);
        TextMeshProUGUI tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
        tmp.font = fuente;
        tmp.text = texto;
        tmp.fontSize = tamano;
        tmp.color = color;
        tmp.alignment = alineacion;
        tmp.raycastTarget = false;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.margin = Vector4.zero;
        return tmp;
    }

    public static void Colocar(RectTransform rt, Vector2 ancla, Vector2 pos, Vector2 tam)
    {
        rt.anchorMin = ancla;
        rt.anchorMax = ancla;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = tam;
    }

    public static void Estirar(RectTransform rt, float margen = 0f)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = new Vector2(margen, margen);
        rt.offsetMax = new Vector2(-margen, -margen);
    }

    public static Button CrearBoton(string nombre, Transform padre, Sprite sprite, TMP_FontAsset fuente,
        string texto, Color color, Vector2 pos, Vector2 tam, float tamanoTexto = 20f)
    {
        Image img = CrearPanel(nombre, padre, sprite, color);
        img.raycastTarget = true;
        Colocar(img.rectTransform, new Vector2(0.5f, 0.5f), pos, tam);

        Button boton = img.gameObject.AddComponent<Button>();
        boton.targetGraphic = img;

        ColorBlock cb = boton.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(1f, 1f, 1f, 0.82f);
        cb.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
        cb.selectedColor = Color.white;
        cb.disabledColor = new Color(1f, 1f, 1f, 0.4f);
        cb.colorMultiplier = 1f;
        boton.colors = cb;

        TextMeshProUGUI tmp = CrearTexto("Texto", img.transform, fuente, texto, tamanoTexto, Color.white);
        Estirar(tmp.rectTransform, 4f);

        return boton;
    }
}

public class ContextoDesafio
{
    public SnapWebApp snap;
    public RectTransform raiz;
    public RectTransform historia;
    public RectTransform mundo;
    public JugadorTutorial jugador;
    public RectTransform jugadorRT;
    public VinetaHalloween vineta;
}
