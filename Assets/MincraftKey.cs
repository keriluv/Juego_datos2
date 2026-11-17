using UnityEngine;

public class MincraftKey : MonoBehaviour
{
    private bool isGot;
    [SerializeField] private GameObject cylinder;
    [SerializeField] private MinecraftManager minecraftManager;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (isGot) return;

        isGot = true;
        if (minecraftManager != null)
            minecraftManager.addcount();
        if (cylinder != null)
            cylinder.SetActive(false);
    }
}
