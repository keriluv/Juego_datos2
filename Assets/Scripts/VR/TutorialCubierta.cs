using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Letrero de madera en la cubierta. Avanza cuando el jugador agarra, apunta y dispara.
/// </summary>
public class TutorialCubierta : MonoBehaviour
{
    private Text texto;
    private int paso;
    private bool listo;
    private NavalCannon[] canones;

    public bool Completado => paso >= 3;

    public void Colocar(Transform barco, NavalCannon[] canonesDelJugador)
    {
        canones = canonesDelJugador;
        for (int i = 0; i < canones.Length; i++)
        {
            canones[i].AlAgarrar += AlAgarrar;
            canones[i].AlDisparar += AlDisparar;
        }

        GameObject tabla = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(tabla.GetComponent<Collider>());
        tabla.name = "LetreroTutorial";
        tabla.transform.SetParent(barco, false);
        tabla.transform.localPosition = new Vector3(0f, 1.25f, 1.35f);
        tabla.transform.localRotation = Quaternion.Euler(-18f, 0f, 0f);
        tabla.transform.localScale = new Vector3(1.15f, 0.72f, 0.06f);
        tabla.GetComponent<Renderer>().material = NavalCinematicDirector.Lit(new Color(0.45f, 0.26f, 0.12f));

        GameObject lienzoGo = new GameObject("TextoTutorial");
        lienzoGo.transform.SetParent(tabla.transform, false);
        lienzoGo.transform.localPosition = new Vector3(0f, 0f, -0.55f);
        lienzoGo.transform.localScale = Vector3.one * 0.01f;
        Canvas lienzo = lienzoGo.AddComponent<Canvas>();
        lienzo.renderMode = RenderMode.WorldSpace;
        RectTransform rect = lienzoGo.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(110f, 68f);

        GameObject textoGo = new GameObject("Linea");
        textoGo.transform.SetParent(lienzoGo.transform, false);
        texto = textoGo.AddComponent<Text>();
        texto.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        texto.fontSize = 15;
        texto.alignment = TextAnchor.MiddleCenter;
        texto.color = new Color(0.96f, 0.9f, 0.72f);
        texto.horizontalOverflow = HorizontalWrapMode.Wrap;
        RectTransform textoRect = textoGo.GetComponent<RectTransform>();
        textoRect.anchorMin = Vector2.zero;
        textoRect.anchorMax = Vector2.one;
        textoRect.offsetMin = new Vector2(6f, 4f);
        textoRect.offsetMax = new Vector2(-6f, -4f);
        Mostrar();
    }

    private void Update()
    {
        if (paso != 1 || canones == null)
            return;

        for (int i = 0; i < canones.Length; i++)
        {
            if (!canones[i].Agarrado)
                continue;
            if (ApuntaAUnEnemigo(canones[i]))
            {
                paso = 2;
                Mostrar();
                return;
            }
        }
    }

    private void AlAgarrar()
    {
        if (paso == 0)
        {
            paso = 1;
            Mostrar();
        }
    }

    private void AlDisparar()
    {
        if (paso == 2)
        {
            paso = 3;
            listo = true;
            Mostrar();
        }
    }

    private static bool ApuntaAUnEnemigo(NavalCannon canon)
    {
        NavalHull[] cascos = FindObjectsByType<NavalHull>(FindObjectsSortMode.None);
        Vector3 adelante = canon.Boca.forward;
        for (int i = 0; i < cascos.Length; i++)
        {
            if (cascos[i].EsJugador || cascos[i].Hundido)
                continue;

            Vector3 hacia = cascos[i].transform.position - canon.Boca.position;
            if (hacia.sqrMagnitude < 0.2f)
                continue;
            if (Vector3.Angle(adelante, hacia) < 18f)
                return true;
        }

        return false;
    }

    private void Mostrar()
    {
        if (texto == null)
            return;

        if (paso == 0)
            texto.text = "Paso 1\nSostén el cañón";
        else if (paso == 1)
            texto.text = "Paso 2\nApunta a los barcos enemigos";
        else if (paso == 2)
            texto.text = "Paso 3\nPresiona el gatillo para disparar";
        else
            texto.text = listo ? "Hunde el barco de prueba" : "Listo";
    }

    public void AnunciarOleada(int numero)
    {
        if (texto == null)
            return;
        texto.text = numero > 0 ? "Oleada " + numero : "Mar en calma";
    }
}
