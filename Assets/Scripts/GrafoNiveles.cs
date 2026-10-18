using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Nivel
{
    public int id;
    public string nombre;
    public string escena;
    public bool desbloqueado;

    public Nivel(int id, string nombre, string escena, bool desbloqueado = false)
    {
        this.id = id;
        this.nombre = nombre;
        this.escena = escena;
        this.desbloqueado = desbloqueado;
    }
}

public class Arista
{
    public int destino;
    public float peso;

    public Arista(int destino, float peso)
    {
        this.destino = destino;
        this.peso = peso;
    }
}

public class GrafoNiveles
{
    private readonly Dictionary<int, Nivel> niveles = new Dictionary<int, Nivel>();
    private readonly Dictionary<int, List<Arista>> adyacencia = new Dictionary<int, List<Arista>>();

    public Nivel Obtener(int id) { return niveles[id]; }

    public void AgregarNivel(Nivel n)
    {
        niveles[n.id] = n;
        if (!adyacencia.ContainsKey(n.id))
            adyacencia[n.id] = new List<Arista>();
    }

    public void AgregarCamino(int origen, int destino, float peso = 1f, bool bidireccional = false)
    {
        adyacencia[origen].Add(new Arista(destino, peso));
        if (bidireccional)
            adyacencia[destino].Add(new Arista(origen, peso));
    }

    public List<Nivel> Vecinos(int id)
    {
        var lista = new List<Nivel>();
        foreach (var a in adyacencia[id])
            lista.Add(niveles[a.destino]);
        return lista;
    }

    public List<int> BFS(int inicio)
    {
        var orden = new List<int>();
        var visitados = new HashSet<int> { inicio };
        var cola = new Queue<int>();
        cola.Enqueue(inicio);

        while (cola.Count > 0)
        {
            int actual = cola.Dequeue();
            orden.Add(actual);
            foreach (var a in adyacencia[actual])
            {
                if (visitados.Add(a.destino))
                    cola.Enqueue(a.destino);
            }
        }
        return orden;
    }

    public List<int> DFS(int inicio)
    {
        var orden = new List<int>();
        DFSRec(inicio, new HashSet<int>(), orden);
        return orden;
    }

    private void DFSRec(int actual, HashSet<int> visitados, List<int> orden)
    {
        visitados.Add(actual);
        orden.Add(actual);
        foreach (var a in adyacencia[actual])
        {
            if (!visitados.Contains(a.destino))
                DFSRec(a.destino, visitados, orden);
        }
    }

    public List<int> CaminoMasCorto(int origen, int destino, out float costo)
    {
        var dist = new Dictionary<int, float>();
        var previo = new Dictionary<int, int>();
        var visitados = new HashSet<int>();

        foreach (var id in niveles.Keys)
            dist[id] = float.PositiveInfinity;
        dist[origen] = 0f;

        while (true)
        {
            int actual = -1;
            float mejor = float.PositiveInfinity;
            foreach (var kv in dist)
            {
                if (!visitados.Contains(kv.Key) && kv.Value < mejor)
                {
                    mejor = kv.Value;
                    actual = kv.Key;
                }
            }
            if (actual == -1 || actual == destino) break;

            visitados.Add(actual);
            foreach (var a in adyacencia[actual])
            {
                float nuevo = dist[actual] + a.peso;
                if (nuevo < dist[a.destino])
                {
                    dist[a.destino] = nuevo;
                    previo[a.destino] = actual;
                }
            }
        }

        costo = dist[destino];
        var camino = new List<int>();
        if (float.IsPositiveInfinity(costo)) return camino;

        for (int c = destino; ; c = previo[c])
        {
            camino.Insert(0, c);
            if (c == origen) break;
        }
        return camino;
    }

    public void CompletarNivel(int id)
    {
        foreach (var a in adyacencia[id])
            niveles[a.destino].desbloqueado = true;
    }
}