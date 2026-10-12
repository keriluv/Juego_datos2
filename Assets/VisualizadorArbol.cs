using UnityEngine;
using TMPro;

public class VisualizadorArbol : MonoBehaviour
{
    [SerializeField] private GameObject prefabNodo;
    [SerializeField] private RectTransform contenedorArbol;
    [SerializeField] private GameObject prefabLinea;

    private GameObject nodoVisualActual;

    [SerializeField] private float separacionX=60f;
    [SerializeField] private float separacionY=30f;

    public void MostrarArbol(ArbolABB<PeligroData> arbol)
    {
        foreach (Transform hijo in contenedorArbol)
        {
            Destroy(hijo.gameObject);
        }

        if (arbol.Raiz != null)
        {
            CrearNodo(arbol.Raiz, 0, 0);
        }
    }

    private GameObject CrearNodo(NodoABB<PeligroData> nodo, int nivel, float posicion)
    {
        if (nodo == null)
            return null;


        float x = posicion * separacionX;
        float y = -nivel * separacionY;

        GameObject nuevoNodo = Instantiate(
            prefabNodo,
            contenedorArbol
        );

        RectTransform rect = nuevoNodo.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(x, y);

        TMP_Text texto = nuevoNodo.GetComponentInChildren<TMP_Text>();

        if (texto != null)
        {
            texto.text = nodo.Valor.nombre + "\nNivel: " + nodo.Valor.nivelPeligro;
        }

        if (nodo.Izquierdo != null)
        {
            GameObject hijoIzquierdo = CrearNodo(
                nodo.Izquierdo,
                nivel + 1,
                posicion - 1f
            );

            CrearLinea(nuevoNodo, hijoIzquierdo);
        }

        if (nodo.Derecho != null)
        {
            GameObject hijoDerecho = CrearNodo(
                nodo.Derecho,
                nivel + 1,
                posicion + 1f
            );

            CrearLinea(nuevoNodo, hijoDerecho);
        }

        return nuevoNodo;
    }
    private void CrearLinea(GameObject padre, GameObject hijo)
    {
        GameObject linea = Instantiate(
            prefabLinea,
            contenedorArbol
        );

        linea.transform.SetAsFirstSibling();

        RectTransform rectLinea = linea.GetComponent<RectTransform>();

        RectTransform rectPadre = padre.GetComponent<RectTransform>();
        RectTransform rectHijo = hijo.GetComponent<RectTransform>();

        Vector2 posicionPadre = rectPadre.anchoredPosition;
        Vector2 posicionHijo = rectHijo.anchoredPosition;

        Vector2 direccion = posicionHijo - posicionPadre;

        float distancia = direccion.magnitude;

        rectLinea.anchoredPosition = posicionPadre + direccion / 2f;

        rectLinea.sizeDelta = new Vector2(
            5f,
            distancia
        );

        float angulo = Mathf.Atan2(
            direccion.y,
            direccion.x
        ) * Mathf.Rad2Deg;

        rectLinea.rotation = Quaternion.Euler(
            0,
            0,
            angulo - 90f
        );
    }
}