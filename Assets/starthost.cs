using Unity.Netcode;
using UnityEngine;

public class starthost : MonoBehaviour
{
    void Start()
    {
        var manager = NetworkManager.Singleton;

        if (manager == null)
        {
            Debug.LogError("No existe NetworkManager");
            return;
        }

        if (manager.IsListening)
        {
            Debug.LogWarning("La red ya está iniciada");
            return;
        }

        bool success = manager.StartHost();


        Debug.Log("Host iniciado: " + success);
    }
}
