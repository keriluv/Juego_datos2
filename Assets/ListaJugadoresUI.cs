using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ListaJugadoresUI : MonoBehaviour
{
    [SerializeField] private Transform contenedor;      
    [SerializeField] private GameObject prefabFila;     
    [SerializeField] private string formato = "Player {0}: {1}";
    [SerializeField] private Color colorLocal = new Color(0f, 0f, 0f);
    [SerializeField] private Color colorOther = new Color(0.486f, 0.486f, 0.486f);

    private readonly List<GameObject> filas = new List<GameObject>();

    private void OnEnable()
    {
        SkinJugador.AlCambiarLista += Refrescar;
        Refrescar();
    }

    private void OnDisable()
    {
        SkinJugador.AlCambiarLista -= Refrescar;
    }

    public void Refrescar()
    {
        if (contenedor == null || prefabFila == null) return;

        foreach (GameObject fila in filas)
            if (fila != null) Destroy(fila);
        filas.Clear();

        // Orden estable: el host (ClientId 0) siempre es Player 1
        List<SkinJugador> jugadores = new List<SkinJugador>(SkinJugador.Todos);
        jugadores.Sort((a, b) => a.OwnerClientId.CompareTo(b.OwnerClientId));

        for (int i = 0; i < jugadores.Count; i++)
        {
            GameObject fila = Instantiate(prefabFila, contenedor);
            fila.SetActive(true);

            TMP_Text texto = fila.GetComponentInChildren<TMP_Text>();
            if (texto != null)
            {
                texto.text = string.Format(formato, i + 1, jugadores[i].NombreSkin);
                texto.color = jugadores[i].OwnerClientId == Unity.Netcode.NetworkManager.Singleton.LocalClientId ? colorLocal :colorOther;

                texto.ForceMeshUpdate();
            }

            filas.Add(fila);
        }
    }
}
