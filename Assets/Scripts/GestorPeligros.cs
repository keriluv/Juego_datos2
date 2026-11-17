
using UnityEngine;

public class GestorPeligros : MonoBehaviour
{
    public static GestorPeligros Instance { get; private set; }

    private ArbolABB<PeligroData> arbolPeligros = new ArbolABB<PeligroData>();

    [SerializeField] private VisualizadorArbol visualizador;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }


    public ArbolABB<PeligroData> GetArbol()
    {
        return arbolPeligros;
    }
    public void añadirItem(GameObject p)
    {
        PeligroInfo info = p.GetComponent<PeligroInfo>();
        int nivel = info != null ? info.nivelPeligro : 1;

        arbolPeligros.Insertar(
            new PeligroData(p.name, nivel, p)
        );

        Debug.Log(p.name + " x" + nivel);

        if (arbolPeligros.Raiz != null)
        {
            Debug.Log(
                "La raiz es: " + arbolPeligros.Raiz.Valor.nombre +
                " y nivel " + arbolPeligros.Raiz.Valor.nivelPeligro
            );
        }

        if (visualizador != null)
        {
            visualizador.MostrarArbol(arbolPeligros);
        }
    }

}
