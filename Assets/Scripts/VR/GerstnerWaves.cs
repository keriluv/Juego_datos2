using UnityEngine;

/// <summary>
/// Misma fórmula de Gerstner que el shader Juego/AguaGerstner.
/// Si cambias una ola aquí, cámbiala también en el HLSL.
/// </summary>
public static class GerstnerWaves
{
    public struct Ola
    {
        public Vector2 direccion;
        public float amplitud;
        public float frecuencia;
        public float velocidad;
        public float pendiente;
    }

    public static readonly Ola[] Olas =
    {
        new Ola { direccion = new Vector2(1f, 0.2f), amplitud = 0.18f, frecuencia = 0.16f, velocidad = 1.05f, pendiente = 0.40f },
        new Ola { direccion = new Vector2(-0.35f, 1f), amplitud = 0.12f, frecuencia = 0.28f, velocidad = 1.35f, pendiente = 0.30f },
        new Ola { direccion = new Vector2(0.15f, -1f), amplitud = 0.07f, frecuencia = 0.45f, velocidad = 1.70f, pendiente = 0.22f }
    };

    public static float Altura(float x, float z, float tiempo)
    {
        float altura = 0f;
        for (int i = 0; i < Olas.Length; i++)
        {
            Ola ola = Olas[i];
            Vector2 direccion = ola.direccion.normalized;
            float fase = ola.frecuencia * (direccion.x * x + direccion.y * z) + tiempo * ola.velocidad * ola.frecuencia;
            altura += ola.amplitud * Mathf.Sin(fase);
        }

        return altura;
    }

    public static Vector3 Desplazar(Vector3 posicion, float tiempo)
    {
        Vector3 resultado = posicion;
        for (int i = 0; i < Olas.Length; i++)
        {
            Ola ola = Olas[i];
            Vector2 direccion = ola.direccion.normalized;
            float fase = ola.frecuencia * (direccion.x * posicion.x + direccion.y * posicion.z) + tiempo * ola.velocidad * ola.frecuencia;
            float coseno = Mathf.Cos(fase);
            resultado.x += ola.pendiente * ola.amplitud * direccion.x * coseno;
            resultado.z += ola.pendiente * ola.amplitud * direccion.y * coseno;
            resultado.y += ola.amplitud * Mathf.Sin(fase);
        }

        return resultado;
    }
}
