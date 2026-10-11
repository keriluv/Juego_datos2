using UnityEngine.SceneManagement;

/// <summary>
/// Punto único para entrar a la batalla naval desde cualquier botón del juego.
/// La escena tiene que estar en File > Build Profiles.
/// </summary>
public static class CargadorBatallaVR
{
    public const string NombreEscena = "EscenaVR";

    public static void Cargar()
    {
        SceneManager.LoadScene(NombreEscena);
    }
}
