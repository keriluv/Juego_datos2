using UnityEngine;

/// <summary>Mantiene la etiqueta del jugador mirando a la camara.</summary>
public class MirarCamara : MonoBehaviour
{
    private void LateUpdate()
    {
        Camera camara = Camera.main;
        if (camara == null) return;

        Vector3 direccion = transform.position - camara.transform.position;
        if (direccion.sqrMagnitude < 0.001f) return;

        transform.rotation = Quaternion.LookRotation(direccion, Vector3.up);
    }
}
