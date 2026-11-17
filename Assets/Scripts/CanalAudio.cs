using UnityEngine;

/// <summary>
/// Opcional: pon esto en un AudioSource para forzar si cuenta como
/// musica o ambiente/efectos en AjustesJuego.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class CanalAudio : MonoBehaviour
{
    public enum Tipo
    {
        Musica,
        Ambiente
    }

    public Tipo tipo = Tipo.Ambiente;
}
