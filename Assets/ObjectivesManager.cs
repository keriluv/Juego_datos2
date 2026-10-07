using System.Linq;
using TMPro;
using UnityEngine;

public class ObjectivesManager : MonoBehaviour
{
    [SerializeField] private string[] objetivos;
    [SerializeField] private TextMeshProUGUI text;
    private int i = 0;
    private int size;

    void Start()
    {
        text.text = "Objetivo: " + objetivos[i];
        size = objetivos.Length - 1;
    }

    // Update is called once per frame
    public void UpdateObjective()
    {
        if (i < objetivos.Length-1)
        {
            i++;
            text.text = "Objetivo: " + objetivos[i];
        }
        
    }
}
