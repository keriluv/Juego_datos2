using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Reserva de humo, destello y salpicadura para no instanciar efectos en cada disparo.
/// </summary>
public class NavalVfxPool : MonoBehaviour
{
    private readonly Dictionary<string, Queue<GameObject>> reservas = new Dictionary<string, Queue<GameObject>>();
    private readonly Dictionary<string, GameObject> moldes = new Dictionary<string, GameObject>();

    public const string Humo = "humo";
    public const string Destello = "destello";
    public const string Salpicadura = "salpicadura";
    public const string Estela = "estela";

    private void Awake()
    {
        moldes[Humo] = CrearHumo();
        moldes[Destello] = CrearDestello();
        moldes[Salpicadura] = CrearSalpicadura();
        moldes[Estela] = CrearEstela();
    }

    public GameObject Obtener(string id, Vector3 posicion, Quaternion rotacion)
    {
        if (!reservas.TryGetValue(id, out Queue<GameObject> cola))
        {
            cola = new Queue<GameObject>();
            reservas[id] = cola;
        }

        GameObject instancia = null;
        while (cola.Count > 0 && instancia == null)
            instancia = cola.Dequeue();

        if (instancia == null && moldes.TryGetValue(id, out GameObject molde))
            instancia = Instantiate(molde);

        if (instancia == null)
            return null;

        instancia.transform.SetPositionAndRotation(posicion, rotacion);
        instancia.SetActive(true);
        ParticleSystem[] sistemas = instancia.GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < sistemas.Length; i++)
            sistemas[i].Play(true);

        Light luz = instancia.GetComponentInChildren<Light>();
        if (luz != null)
            StartCoroutine(ApagarLuz(luz));

        return instancia;
    }

    public void Liberar(string id, GameObject instancia, float segundos)
    {
        if (instancia == null)
            return;
        StartCoroutine(Devolver(id, instancia, segundos));
    }

    public void Disparo(Vector3 posicion, Quaternion rotacion)
    {
        Liberar(Humo, Obtener(Humo, posicion, rotacion), 3.2f);
        Liberar(Destello, Obtener(Destello, posicion, rotacion), 0.35f);
    }

    public void ImpactoAgua(Vector3 posicion)
    {
        Liberar(Salpicadura, Obtener(Salpicadura, posicion, Quaternion.identity), 2.2f);
    }

    public GameObject CrearEstelaSeguidora(Transform popa)
    {
        GameObject estela = Obtener(Estela, popa.position, popa.rotation);
        if (estela == null)
            return null;
        estela.transform.SetParent(popa, false);
        estela.transform.localPosition = Vector3.zero;
        return estela;
    }

    private IEnumerator Devolver(string id, GameObject instancia, float segundos)
    {
        yield return new WaitForSeconds(segundos);
        if (instancia == null)
            yield break;

        ParticleSystem[] sistemas = instancia.GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < sistemas.Length; i++)
            sistemas[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        instancia.SetActive(false);
        instancia.transform.SetParent(transform, false);
        reservas[id].Enqueue(instancia);
    }

    private static IEnumerator ApagarLuz(Light luz)
    {
        luz.enabled = true;
        luz.intensity = 4.5f;
        float t = 0f;
        while (t < 0.12f)
        {
            t += Time.deltaTime;
            luz.intensity = Mathf.Lerp(4.5f, 0f, t / 0.12f);
            yield return null;
        }

        luz.enabled = false;
    }

    private static GameObject CrearHumo()
    {
        GameObject raiz = new GameObject("HumoCanon");
        raiz.SetActive(false);
        ParticleSystem sistema = raiz.AddComponent<ParticleSystem>();
        var principal = sistema.main;
        principal.playOnAwake = false;
        principal.loop = false;
        principal.duration = 0.4f;
        principal.startLifetime = new ParticleSystem.MinMaxCurve(1.6f, 2.8f);
        principal.startSpeed = new ParticleSystem.MinMaxCurve(0.6f, 1.8f);
        principal.startSize = new ParticleSystem.MinMaxCurve(0.35f, 0.7f);
        principal.startColor = new ParticleSystem.MinMaxGradient(new Color(0.93f, 0.94f, 0.95f, 0.85f), new Color(0.75f, 0.78f, 0.82f, 0.45f));
        principal.gravityModifier = -0.05f;
        principal.maxParticles = 40;
        var emision = sistema.emission;
        emision.SetBursts(new[] { new ParticleSystem.Burst(0f, 22) });
        var forma = sistema.shape;
        forma.shapeType = ParticleSystemShapeType.Cone;
        forma.angle = 18f;
        forma.radius = 0.08f;
        var limite = sistema.limitVelocityOverLifetime;
        limite.enabled = true;
        limite.drag = 2.4f;
        var tamano = sistema.sizeOverLifetime;
        tamano.enabled = true;
        tamano.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 0.4f, 1f, 2.4f));
        var color = sistema.colorOverLifetime;
        color.enabled = true;
        Gradient gradiente = new Gradient();
        gradiente.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(0.8f, 0.82f, 0.85f), 1f) },
            new[] { new GradientAlphaKey(0.8f, 0f), new GradientAlphaKey(0f, 1f) });
        color.color = gradiente;
        AsignarMaterial(sistema, new Color(1f, 1f, 1f, 0.8f));
        return raiz;
    }

    private static GameObject CrearDestello()
    {
        GameObject raiz = new GameObject("Destello");
        raiz.SetActive(false);
        ParticleSystem sistema = raiz.AddComponent<ParticleSystem>();
        var principal = sistema.main;
        principal.playOnAwake = false;
        principal.loop = false;
        principal.duration = 0.08f;
        principal.startLifetime = 0.12f;
        principal.startSpeed = 4f;
        principal.startSize = 0.22f;
        principal.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.72f, 0.2f), new Color(1f, 0.35f, 0.08f));
        principal.maxParticles = 16;
        var emision = sistema.emission;
        emision.SetBursts(new[] { new ParticleSystem.Burst(0f, 10) });
        AsignarMaterial(sistema, new Color(1f, 0.6f, 0.15f));

        GameObject luzGo = new GameObject("Flash");
        luzGo.transform.SetParent(raiz.transform, false);
        Light luz = luzGo.AddComponent<Light>();
        luz.type = LightType.Point;
        luz.color = new Color(1f, 0.62f, 0.22f);
        luz.range = 6f;
        luz.intensity = 0f;
        luz.shadows = LightShadows.None;
        return raiz;
    }

    private static GameObject CrearSalpicadura()
    {
        GameObject raiz = new GameObject("Salpicadura");
        raiz.SetActive(false);
        ParticleSystem sistema = raiz.AddComponent<ParticleSystem>();
        var principal = sistema.main;
        principal.playOnAwake = false;
        principal.loop = false;
        principal.duration = 0.2f;
        principal.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 0.9f);
        principal.startSpeed = new ParticleSystem.MinMaxCurve(2.5f, 5.5f);
        principal.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.2f);
        principal.startColor = new ParticleSystem.MinMaxGradient(new Color(0.75f, 0.92f, 1f), Color.white);
        principal.gravityModifier = 1.3f;
        principal.maxParticles = 36;
        var emision = sistema.emission;
        emision.SetBursts(new[] { new ParticleSystem.Burst(0f, 24) });
        var forma = sistema.shape;
        forma.shapeType = ParticleSystemShapeType.Circle;
        forma.radius = 0.15f;
        forma.arc = 360f;
        AsignarMaterial(sistema, new Color(0.7f, 0.9f, 1f));
        return raiz;
    }

    private static GameObject CrearEstela()
    {
        GameObject raiz = new GameObject("Estela");
        raiz.SetActive(false);
        ParticleSystem sistema = raiz.AddComponent<ParticleSystem>();
        var principal = sistema.main;
        principal.playOnAwake = true;
        principal.loop = true;
        principal.startLifetime = 1.4f;
        principal.startSpeed = 0.15f;
        principal.startSize = 0.45f;
        principal.startColor = new Color(0.9f, 0.95f, 1f, 0.55f);
        principal.maxParticles = 40;
        principal.simulationSpace = ParticleSystemSimulationSpace.World;
        var emision = sistema.emission;
        emision.rateOverTime = 10f;
        var forma = sistema.shape;
        forma.shapeType = ParticleSystemShapeType.Box;
        forma.scale = new Vector3(0.8f, 0.05f, 0.3f);
        AsignarMaterial(sistema, Color.white);
        return raiz;
    }

    private static void AsignarMaterial(ParticleSystem sistema, Color color)
    {
        ParticleSystemRenderer render = sistema.GetComponent<ParticleSystemRenderer>();
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null)
            shader = Shader.Find("Sprites/Default");
        if (shader == null)
            return;

        Material material = new Material(shader);
        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", color);
        material.color = color;
        render.material = material;
    }
}
