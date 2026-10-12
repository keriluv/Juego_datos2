using System;
using UnityEngine;

public class PeligroData : IComparable<PeligroData>
{
    public string nombre;
    public int nivelPeligro;
    public GameObject objeto;

    public PeligroData(string nombre, int nivelPeligro, GameObject objeto = null)
    {
        this.nombre = nombre;
        this.nivelPeligro = nivelPeligro;
        this.objeto = objeto;
    }

    public int CompareTo(PeligroData other)
    {
        if (other == null) return 1;
        return this.nivelPeligro.CompareTo(other.nivelPeligro);
    }
}