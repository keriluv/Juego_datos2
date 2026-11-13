using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class TemasEscape
{
    private const float PixelsPorUnidad = 100f;

    private static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();
    private static readonly string[] carpetas = { "playa", "halloween", "ciudad" };

    public static bool Existe(int nivel, string archivo)
    {
        return File.Exists(Ruta(nivel, archivo));
    }

    public static Sprite Obtener(int nivel, string archivo)
    {
        return CargarSprite(nivel, archivo, false, FilterMode.Point);
    }

    public static Sprite Obtener(int nivel, string archivo, bool repetir)
    {
        return CargarSprite(nivel, archivo, repetir, FilterMode.Point);
    }

    public static Sprite ObtenerSuave(int nivel, string archivo)
    {
        return CargarSprite(nivel, archivo, false, FilterMode.Bilinear);
    }

    public static Sprite ObtenerSuave(int nivel, string archivo, bool repetir)
    {
        return CargarSprite(nivel, archivo, repetir, FilterMode.Bilinear);
    }

    public static Texture2D Textura(int nivel, string archivo, bool repetir)
    {
        Sprite sprite = CargarSprite(nivel, archivo, repetir, FilterMode.Point);
        return sprite != null ? sprite.texture : null;
    }

    public static Texture2D TexturaSuave(int nivel, string archivo, bool repetir)
    {
        Sprite sprite = CargarSprite(nivel, archivo, repetir, FilterMode.Bilinear);
        return sprite != null ? sprite.texture : null;
    }

    public static Sprite Degradado(Color arriba, Color abajo)
    {
        string clave = "g|" + ColorUtility.ToHtmlStringRGBA(arriba) + "|" + ColorUtility.ToHtmlStringRGBA(abajo);

        Sprite guardado;
        if (cache.TryGetValue(clave, out guardado))
        {
            if (Valido(guardado)) return guardado;
            cache.Remove(clave);
        }

        const int alto = 128;
        Texture2D textura = new Texture2D(1, alto, TextureFormat.RGBA32, false);
        textura.filterMode = FilterMode.Bilinear;
        textura.wrapMode = TextureWrapMode.Clamp;
        textura.anisoLevel = 0;

        for (int i = 0; i < alto; i++)
        {
            float t = i / (alto - 1f);
            textura.SetPixel(0, i, Color.Lerp(abajo, arriba, t));
        }

        textura.Apply(false, false);
        Sprite sprite = CrearSprite(textura);
        cache[clave] = sprite;
        return sprite;
    }

    public static Sprite Corazon()
    {
        const string clave = "corazon";

        Sprite guardado;
        if (cache.TryGetValue(clave, out guardado))
        {
            if (Valido(guardado)) return guardado;
            cache.Remove(clave);
        }

        string[] filas =
        {
            ".XX...XX...",
            "XXXX.XXXX..",
            "XXXXXXXXXXX",
            "XXXXXXXXXXX",
            "XXXXXXXXXXX",
            ".XXXXXXXXX.",
            "..XXXXXXX..",
            "...XXXXX...",
            "....XXX....",
            ".....X....."
        };

        return CrearDesdeFilas(clave, filas);
    }

    public static Sprite Caramelo()
    {
        const string clave = "caramelo";

        Sprite guardado;
        if (cache.TryGetValue(clave, out guardado))
        {
            if (Valido(guardado)) return guardado;
            cache.Remove(clave);
        }

        string[] filas =
        {
            "X.........X",
            "XX.XXXXX.XX",
            ".XXXXXXXXX.",
            "..XXXXXXX..",
            "..XXXXXXX..",
            ".XXXXXXXXX.",
            "XX.XXXXX.XX",
            "X.........X"
        };

        return CrearDesdeFilas(clave, filas);
    }

    private static Sprite CrearDesdeFilas(string clave, string[] filas)
    {
        int ancho = filas[0].Length;
        int alto = filas.Length;
        Texture2D textura = new Texture2D(ancho, alto, TextureFormat.RGBA32, false);
        textura.filterMode = FilterMode.Point;
        textura.wrapMode = TextureWrapMode.Clamp;
        textura.anisoLevel = 0;

        for (int y = 0; y < alto; y++)
        {
            string fila = filas[alto - 1 - y];

            for (int x = 0; x < ancho; x++)
                textura.SetPixel(x, y, fila[x] == 'X' ? Color.white : Color.clear);
        }

        textura.Apply(false, false);
        Sprite sprite = CrearSprite(textura);
        cache[clave] = sprite;
        return sprite;
    }

    private static Sprite CargarSprite(int nivel, string archivo, bool repetir, FilterMode filtro)
    {
        string clave = "s|" + nivel + "|" + archivo + "|" + repetir + "|" + filtro;

        Sprite guardado;
        if (cache.TryGetValue(clave, out guardado))
        {
            if (Valido(guardado)) return guardado;
            cache.Remove(clave);
        }

        Texture2D textura = Cargar(nivel, archivo, repetir, filtro);
        Sprite sprite = textura != null ? CrearSprite(textura) : null;
        cache[clave] = sprite;
        return sprite;
    }

    private static Texture2D Cargar(int nivel, string archivo, bool repetir, FilterMode filtro)
    {
        string ruta = Ruta(nivel, archivo);
        if (!File.Exists(ruta)) return null;

        try
        {
            byte[] bytes = File.ReadAllBytes(ruta);
            Texture2D textura = new Texture2D(2, 2, TextureFormat.RGBA32, false);

            if (!textura.LoadImage(bytes))
            {
                Object.Destroy(textura);
                return null;
            }

            textura.filterMode = filtro;
            textura.wrapMode = repetir ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
            textura.anisoLevel = 0;
            textura.Apply(false, false);
            return textura;
        }
        catch
        {
            return null;
        }
    }

    private static Sprite CrearSprite(Texture2D textura)
    {
        return UnityEngine.Sprite.Create(textura, new Rect(0f, 0f, textura.width, textura.height),
            new Vector2(0.5f, 0.5f), PixelsPorUnidad, 0, SpriteMeshType.FullRect);
    }

    private static bool Valido(Sprite sprite)
    {
        return sprite != null && sprite.texture != null;
    }

    private static string Ruta(int nivel, string archivo)
    {
        int indice = Mathf.Clamp(nivel, 0, carpetas.Length - 1);
        return Path.Combine(Application.streamingAssetsPath, "Temas", carpetas[indice], archivo + ".png");
    }
}
