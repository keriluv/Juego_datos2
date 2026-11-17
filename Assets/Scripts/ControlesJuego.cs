using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Single source of truth for gameplay/UI gamepad + keyboard bindings.
/// Xbox: A jump, RT attack (edge), left stick move, right stick look, L3 sprint,
/// B back, Start pause, D-pad UI, X interact, Y inventory.
/// </summary>
public static class ControlesJuego
{
    public const float ZonaMuertaStick = 0.2f;
    public const float UmbralGatillo = 0.5f;

    private static float gatilloDerechoAnterior;

    public static Gamepad Mando => Gamepad.current;
    public static Keyboard Teclado => Keyboard.current;
    public static Mouse Raton => Mouse.current;

    public static Vector2 LeerMovimiento(float escala = 1f)
    {
        Vector2 input = Vector2.zero;

        Keyboard teclado = Teclado;
        if (teclado != null)
        {
            if (teclado.wKey.isPressed) input.y += escala;
            if (teclado.sKey.isPressed) input.y -= escala;
            if (teclado.aKey.isPressed) input.x -= escala;
            if (teclado.dKey.isPressed) input.x += escala;
        }

        Gamepad mando = Mando;
        if (mando != null)
        {
            Vector2 stick = AplicarZonaMuerta(mando.leftStick.ReadValue());
            stick *= escala;
            if (Mathf.Abs(stick.x) > Mathf.Abs(input.x)) input.x = stick.x;
            if (Mathf.Abs(stick.y) > Mathf.Abs(input.y)) input.y = stick.y;
        }

        return input;
    }

    public static bool QuiereCorrer()
    {
        Keyboard teclado = Teclado;
        if (teclado != null && (teclado.leftShiftKey.isPressed || teclado.rightShiftKey.isPressed))
            return true;

        Gamepad mando = Mando;
        return mando != null && mando.leftStickButton.isPressed;
    }

    public static bool QuiereSaltar()
    {
        // Callers already gate on PauseManager.IsPaused.
        // Xbox A (buttonSouth) ONLY jumps — never attack.
        Keyboard teclado = Teclado;
        if (teclado != null && teclado.spaceKey.wasPressedThisFrame) return true;

        Gamepad mando = Mando;
        return mando != null && mando.buttonSouth.wasPressedThisFrame;
    }

    public static bool QuiereGolpear()
    {
        // Mouse hold for KB+M (unchanged feel).
        if (Input.GetKey(KeyCode.Mouse0)) return true;
        Mouse raton = Raton;
        if (raton != null && raton.leftButton.isPressed) return true;

        Gamepad mando = Mando;
        if (mando == null)
        {
            gatilloDerechoAnterior = 0f;
            return false;
        }

        // Xbox RT — rising-edge across threshold. A must never attack.
        float valor = mando.rightTrigger.ReadValue();
        bool cruzo = gatilloDerechoAnterior < UmbralGatillo && valor >= UmbralGatillo;
        gatilloDerechoAnterior = valor;
        return cruzo;
    }

    public static Vector2 LeerMiradaStick(float zonaMuerta = ZonaMuertaStick)
    {
        Gamepad mando = Mando;
        if (mando == null) return Vector2.zero;
        return AplicarZonaMuerta(mando.rightStick.ReadValue(), zonaMuerta);
    }

    public static bool QuierePausar()
    {
        Keyboard teclado = Teclado;
        if (teclado != null)
        {
            if (teclado.escapeKey.wasPressedThisFrame) return true;
            if (teclado.pKey.wasPressedThisFrame) return true;
        }

        Gamepad mando = Mando;
        return mando != null && mando.startButton.wasPressedThisFrame;
    }

    public static bool QuiereCancelar()
    {
        // Xbox B only — Escape is reserved for pause toggle.
        Gamepad mando = Mando;
        return mando != null && mando.buttonEast.wasPressedThisFrame;
    }

    public static bool QuiereConfirmarUI()
    {
        Gamepad mando = Mando;
        if (mando != null && mando.buttonSouth.wasPressedThisFrame) return true;

        Keyboard teclado = Teclado;
        return teclado != null && teclado.enterKey.wasPressedThisFrame;
    }

    public static bool QuiereInteractuar()
    {
        Gamepad mando = Mando;
        if (mando != null && mando.buttonWest.wasPressedThisFrame) return true;

        Keyboard teclado = Teclado;
        return teclado != null && teclado.eKey.wasPressedThisFrame;
    }

    public static bool QuiereInventario()
    {
        Gamepad mando = Mando;
        if (mando != null && mando.buttonNorth.wasPressedThisFrame) return true;

        Keyboard teclado = Teclado;
        return teclado != null && teclado.iKey.wasPressedThisFrame;
    }

    public static Vector2 LeerNavegacionUI()
    {
        Gamepad mando = Mando;
        if (mando == null) return Vector2.zero;

        Vector2 stick = AplicarZonaMuerta(mando.leftStick.ReadValue());
        Vector2 dpad = mando.dpad.ReadValue();
        return stick.magnitude > dpad.magnitude ? stick : dpad;
    }

    public static Vector2 AplicarZonaMuerta(Vector2 stick, float zona = ZonaMuertaStick)
    {
        if (stick.magnitude < zona) return Vector2.zero;
        return stick.normalized * ((stick.magnitude - zona) / (1f - zona));
    }
}
