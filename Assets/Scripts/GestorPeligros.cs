using UnityEngine;

public class GestorPeligros : MonoBehaviour
{
    private ArbolABB<PeligroData> arbolPeligros = new ArbolABB<PeligroData>();
    [SerializeField] private VisualizadorArbol visualizador;

    void Start()
    {
        GameObject[] peligros = GameObject.FindGameObjectsWithTag("Item");

        foreach (GameObject p in peligros)
        {
            PeligroInfo info = p.GetComponent<PeligroInfo>();
            int nivel = info != null ? info.nivelPeligro : 1;

            arbolPeligros.Insertar(new PeligroData(p.name, nivel, p));
            Debug.Log(p.name + " xx" + nivel + "");
        }

        foreach (var dato in arbolPeligros.RecorridoInOrder())
        {
            Debug.Log(dato.nombre + " - nivel de peligro: " + dato.nivelPeligro);
        }

        Debug.Log("La raiz es: " + arbolPeligros.Raiz.Valor.nombre + "y nivel " + arbolPeligros.Raiz.Valor.nivelPeligro);
        visualizador.MostrarArbol(arbolPeligros);
    }

    public ArbolABB<PeligroData> getArbol()
    {
        return arbolPeligros;
    }
}