using System.Collections;
using UnityEngine;

/// <summary>
/// Bala de cañón. Ignora al barco que la disparó y daña al primer barco de otro equipo.
/// </summary>
public class ProyectilCanon : MonoBehaviour
{
    private float dano = 30f;
    private float vidaSegundos = 5f;
    private GameObject efectoExplosion;
    private GameObject efectoAgua;
    private AudioClip sonidoImpacto;
    private int equipoDueno = -1;
    private bool yaImpacto;

    public void Lanzar(Vector3 velocidad, int equipo, GameObject barcoDueno, float danoDisparo, GameObject explosion, AudioClip impacto)
    {
        equipoDueno = equipo;
        dano = danoDisparo;
        efectoExplosion = explosion;
        sonidoImpacto = impacto;

        Rigidbody cuerpo = GetComponent<Rigidbody>();
        cuerpo.linearVelocity = velocidad;

        if (barcoDueno != null)
        {
            Collider propio = GetComponent<Collider>();
            foreach (Collider otro in barcoDueno.GetComponentsInChildren<Collider>())
                Physics.IgnoreCollision(propio, otro, true);
        }

        StartCoroutine(Expirar());
    }

    public void DefinirEfectoAgua(GameObject salpicadura)
    {
        efectoAgua = salpicadura;
    }

    private void OnCollisionEnter(Collision choque)
    {
        if (yaImpacto)
            return;

        BarcoSalud barco = choque.collider.GetComponentInParent<BarcoSalud>();
        if (barco != null)
            barco.RecibirDanio(dano, equipoDueno);

        yaImpacto = true;
        Vector3 punto = choque.contactCount > 0 ? choque.GetContact(0).point : transform.position;

        GameObject prefabEfecto = barco != null ? efectoExplosion : efectoAgua;
        if (prefabEfecto != null)
        {
            GameObject efecto = Instantiate(prefabEfecto, punto, Quaternion.identity);
            efecto.SetActive(true);
            ParticleSystem sistema = efecto.GetComponent<ParticleSystem>();
            if (sistema != null)
                sistema.Play(true);
            Destroy(efecto, 3f);
        }

        if (sonidoImpacto != null)
            AudioSource.PlayClipAtPoint(sonidoImpacto, punto, 0.8f);

        Destroy(gameObject);
    }

    private IEnumerator Expirar()
    {
        yield return new WaitForSeconds(vidaSegundos);
        if (this != null)
            Destroy(gameObject);
    }
}
