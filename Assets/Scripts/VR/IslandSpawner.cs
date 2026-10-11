using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Coloca islas bajas en poligonos alrededor del jugador para llenar el horizonte.
/// No usa colisiones: los barcos rodean por dentro del archipielago sin trabarse.
/// Va en el objeto Archipielago.
/// </summary>
public class IslandSpawner : MonoBehaviour
{
    [SerializeField] private int cantidad = 7;
    [SerializeField] private float radioMinimo = 48f;
    [SerializeField] private float radioMaximo = 74f;

    private Vector3[] posiciones;
    private static Mesh monticulo;
    private static Mesh roca;

    public void Generar(int cantidadDeseada)
    {
        cantidad = Mathf.Clamp(cantidadDeseada, 4, 8);
        posiciones = new Vector3[cantidad];

        if (monticulo == null)
            monticulo = CrearMonticulo();
        if (roca == null)
            roca = CrearRoca();

        for (int i = 0; i < cantidad; i++)
        {
            float angulo = (i / (float)cantidad) * Mathf.PI * 2f + 0.35f;
            float radio = Random.Range(radioMinimo, radioMaximo);
            Vector3 posicion = new Vector3(Mathf.Cos(angulo) * radio, -0.2f, Mathf.Sin(angulo) * radio);
            posiciones[i] = posicion;
            ConstruirIsla(i, posicion, Random.Range(7f, 13f));
        }
    }

    public float AnguloDeIsla(int indice)
    {
        if (posiciones == null || posiciones.Length == 0)
            return indice * 1.4f;

        Vector3 posicion = posiciones[indice % posiciones.Length];
        return Mathf.Atan2(posicion.z, posicion.x);
    }

    public Vector3 PuntoDeAtaque(int indice)
    {
        float angulo = AnguloDeIsla(indice) + Random.Range(-0.25f, 0.25f);
        float radio = Random.Range(18f, 30f);
        return new Vector3(Mathf.Cos(angulo) * radio, 0.35f, Mathf.Sin(angulo) * radio);
    }

    private void ConstruirIsla(int indice, Vector3 posicion, float escala)
    {
        GameObject isla = new GameObject("Isla " + (indice + 1));
        isla.transform.SetParent(transform, false);
        isla.transform.position = posicion;
        isla.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        GameObject baseIsla = new GameObject("Base");
        baseIsla.transform.SetParent(isla.transform, false);
        MeshFilter filtro = baseIsla.AddComponent<MeshFilter>();
        MeshRenderer render = baseIsla.AddComponent<MeshRenderer>();
        filtro.sharedMesh = monticulo;
        render.sharedMaterial = MaterialBarco.Crear(Color.Lerp(new Color(0.72f, 0.62f, 0.38f), new Color(0.46f, 0.58f, 0.32f), indice % 2 == 0 ? 0.35f : 0.8f));
        render.shadowCastingMode = ShadowCastingMode.Off;
        render.receiveShadows = false;
        baseIsla.transform.localScale = new Vector3(escala, escala * 0.42f, escala * Random.Range(0.85f, 1.15f));

        int rocas = 3 + (indice % 3);
        for (int r = 0; r < rocas; r++)
        {
            float giro = (r / (float)rocas) * Mathf.PI * 2f;
            Vector3 local = new Vector3(Mathf.Cos(giro), 0.15f, Mathf.Sin(giro)) * (escala * 0.28f);
            Pieza(isla.transform, roca, local, Quaternion.Euler(Random.Range(-12f, 12f), giro * Mathf.Rad2Deg, Random.Range(-8f, 8f)),
                new Vector3(Random.Range(0.7f, 1.8f), Random.Range(0.5f, 1.5f), Random.Range(0.7f, 1.6f)),
                new Color(0.38f, 0.34f, 0.3f));
        }

        if (indice % 2 == 0)
            Palmera(isla.transform, new Vector3(0.4f, escala * 0.22f, -0.2f));
    }

    private static void Palmera(Transform padre, Vector3 local)
    {
        Transform tronco = Primitiva(padre, PrimitiveType.Cylinder, local, Quaternion.identity, new Vector3(0.18f, 1.5f, 0.18f), new Color(0.42f, 0.26f, 0.12f));
        Vector3 copa = tronco.localPosition + Vector3.up * 1.45f;
        for (int i = 0; i < 6; i++)
        {
            float angulo = i * 60f;
            Quaternion rotacion = Quaternion.Euler(28f, angulo, 0f);
            Transform hoja = Primitiva(padre, PrimitiveType.Cube, copa + rotacion * new Vector3(0f, 0.15f, 0.7f), rotacion, new Vector3(0.22f, 0.05f, 1.35f), new Color(0.18f, 0.48f, 0.22f));
            hoja.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
        }
    }

    private static Transform Pieza(Transform padre, Mesh malla, Vector3 local, Quaternion rotacion, Vector3 escala, Color color)
    {
        GameObject pieza = new GameObject("Roca");
        pieza.transform.SetParent(padre, false);
        pieza.transform.localPosition = local;
        pieza.transform.localRotation = rotacion;
        pieza.transform.localScale = escala;
        MeshFilter filtro = pieza.AddComponent<MeshFilter>();
        MeshRenderer render = pieza.AddComponent<MeshRenderer>();
        filtro.sharedMesh = malla;
        render.sharedMaterial = MaterialBarco.Crear(color);
        render.shadowCastingMode = ShadowCastingMode.Off;
        render.receiveShadows = false;
        return pieza.transform;
    }

    private static Transform Primitiva(Transform padre, PrimitiveType tipo, Vector3 local, Quaternion rotacion, Vector3 escala, Color color)
    {
        GameObject pieza = GameObject.CreatePrimitive(tipo);
        Collider colision = pieza.GetComponent<Collider>();
        if (colision != null)
            Destroy(colision);
        pieza.transform.SetParent(padre, false);
        pieza.transform.localPosition = local;
        pieza.transform.localRotation = rotacion;
        pieza.transform.localScale = escala;
        Renderer render = pieza.GetComponent<Renderer>();
        render.sharedMaterial = MaterialBarco.Crear(color);
        render.shadowCastingMode = ShadowCastingMode.Off;
        render.receiveShadows = false;
        return pieza.transform;
    }

    private static Mesh CrearMonticulo()
    {
        const int lados = 8;
        Mesh malla = new Mesh();
        Vector3[] vertices = new Vector3[lados * 2 + 2];
        vertices[0] = new Vector3(0f, 0.15f, 0f);
        vertices[1] = new Vector3(0f, 1f, 0f);
        for (int i = 0; i < lados; i++)
        {
            float angulo = i / (float)lados * Mathf.PI * 2f;
            float radio = i % 2 == 0 ? 1f : 0.82f;
            vertices[2 + i] = new Vector3(Mathf.Cos(angulo) * radio, 0f, Mathf.Sin(angulo) * radio);
            vertices[2 + lados + i] = new Vector3(Mathf.Cos(angulo) * 0.22f, 0.72f, Mathf.Sin(angulo) * 0.22f);
        }

        int[] triangulos = new int[lados * 12];
        int t = 0;
        for (int i = 0; i < lados; i++)
        {
            int siguiente = (i + 1) % lados;
            int baseA = 2 + i;
            int baseB = 2 + siguiente;
            int cimaA = 2 + lados + i;
            int cimaB = 2 + lados + siguiente;
            triangulos[t++] = 0;
            triangulos[t++] = baseB;
            triangulos[t++] = baseA;
            triangulos[t++] = baseA;
            triangulos[t++] = baseB;
            triangulos[t++] = cimaB;
            triangulos[t++] = baseA;
            triangulos[t++] = cimaB;
            triangulos[t++] = cimaA;
            triangulos[t++] = cimaA;
            triangulos[t++] = cimaB;
            triangulos[t++] = 1;
        }

        malla.vertices = vertices;
        malla.triangles = triangulos;
        malla.RecalculateNormals();
        return malla;
    }

    private static Mesh CrearRoca()
    {
        const int lados = 6;
        Mesh malla = new Mesh();
        Vector3[] vertices = new Vector3[lados + 2];
        vertices[0] = new Vector3(0f, -0.45f, 0f);
        vertices[1] = new Vector3(0f, 0.7f, 0f);
        for (int i = 0; i < lados; i++)
        {
            float angulo = i / (float)lados * Mathf.PI * 2f;
            float radio = 0.45f + (i % 3) * 0.12f;
            vertices[2 + i] = new Vector3(Mathf.Cos(angulo) * radio, (i % 2) * 0.12f, Mathf.Sin(angulo) * radio);
        }

        int[] triangulos = new int[lados * 6];
        int t = 0;
        for (int i = 0; i < lados; i++)
        {
            int siguiente = 2 + (i + 1) % lados;
            int actual = 2 + i;
            triangulos[t++] = 0;
            triangulos[t++] = siguiente;
            triangulos[t++] = actual;
            triangulos[t++] = actual;
            triangulos[t++] = siguiente;
            triangulos[t++] = 1;
        }

        malla.vertices = vertices;
        malla.triangles = triangulos;
        malla.RecalculateNormals();
        return malla;
    }
}
