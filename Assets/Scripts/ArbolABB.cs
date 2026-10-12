using System;
using System.Collections.Generic;

public class ArbolABB<T> where T : IComparable<T>
{
    public NodoABB<T> Raiz { get; private set; }

   
    public void Insertar(T valor)
    {
        Raiz = InsertarRec(Raiz, valor);
    }

    private NodoABB<T> InsertarRec(NodoABB<T> nodo, T valor)
    {
        if (nodo == null)
            return new NodoABB<T>(valor);

        int comparacion = valor.CompareTo(nodo.Valor);
        if (comparacion < 0)
            nodo.Izquierdo = InsertarRec(nodo.Izquierdo, valor);
        else if (comparacion > 0)
            nodo.Derecho = InsertarRec(nodo.Derecho, valor);

        return nodo;
    }

   
    public bool Buscar(T valor)
    {
        return BuscarRec(Raiz, valor);
    }

    private bool BuscarRec(NodoABB<T> nodo, T valor)
    {
        if (nodo == null) return false;

        int comparacion = valor.CompareTo(nodo.Valor);
        if (comparacion == 0) return true;

        if (comparacion < 0)
            return BuscarRec(nodo.Izquierdo, valor);
        else
            return BuscarRec(nodo.Derecho, valor);
    }

   
    public void Eliminar(T valor)
    {
        Raiz = EliminarRec(Raiz, valor);
    }

    private NodoABB<T> EliminarRec(NodoABB<T> nodo, T valor)
    {
        if (nodo == null) return null;

        int comparacion = valor.CompareTo(nodo.Valor);

        if (comparacion < 0)
        {
            nodo.Izquierdo = EliminarRec(nodo.Izquierdo, valor);
        }
        else if (comparacion > 0)
        {
            nodo.Derecho = EliminarRec(nodo.Derecho, valor);
        }
        else
        {
            // Caso 1: nodo sin hijo izquierdo
            if (nodo.Izquierdo == null) return nodo.Derecho;

            // Caso 2: nodo sin hijo derecho
            if (nodo.Derecho == null) return nodo.Izquierdo;

            // Caso 3: nodo con dos hijos -> buscar sucesor (el mínimo del subárbol derecho)
            NodoABB<T> sucesor = ObtenerMinimo(nodo.Derecho);
            nodo.Valor = sucesor.Valor;
            nodo.Derecho = EliminarRec(nodo.Derecho, sucesor.Valor);
        }

        return nodo;
    }

    private NodoABB<T> ObtenerMinimo(NodoABB<T> nodo)
    {
        while (nodo.Izquierdo != null)
            nodo = nodo.Izquierdo;
        return nodo;
    }

  
    public List<T> RecorridoInOrder()
    {
        List<T> resultado = new List<T>();
        InOrderRec(Raiz, resultado);
        return resultado;
    }

    private void InOrderRec(NodoABB<T> nodo, List<T> resultado)
    {
        if (nodo == null) return;
        InOrderRec(nodo.Izquierdo, resultado);
        resultado.Add(nodo.Valor);
        InOrderRec(nodo.Derecho, resultado);
    }
}