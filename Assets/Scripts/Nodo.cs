using System;

public class NodoABB<T> where T : IComparable<T>
{
    public T Valor;
    public NodoABB<T> Izquierdo;
    public NodoABB<T> Derecho;

    public NodoABB(T valor)
    {
        Valor = valor;
    }
}