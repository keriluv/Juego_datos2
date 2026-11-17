using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class part2Youtube : MonoBehaviour
{
    [SerializeField] private Image loadingImage;
    [SerializeField] private float loadingTime = 10f;
    [SerializeField] private float velocidadRotacion = 180f;

    [SerializeField] private float zoomIn = 5f;
    [SerializeField] private Transform player;
    [SerializeField] private float zoomInTime = 2f;

    [SerializeField, FormerlySerializedAs("camera")] private GameObject customCamera;
    private Camera camaraVista;

    private bool toStart = false;

    private void Awake()
    {
        if (customCamera != null)
            camaraVista = customCamera.GetComponent<Camera>();
    }

    private Transform CamaraTransform
    {
        get
        {
            if (camaraVista != null)
                return camaraVista.transform;
            return customCamera != null ? customCamera.transform : null;
        }
    }
  
    private void Start()
    {
        loadingImage.gameObject.SetActive(false);
       
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.transform;
            Begin();
        }
    }
    public void Begin()
    {
        if (toStart)
            return;

        toStart = true;

        StartCoroutine(Zoom());
    }


    private IEnumerator Zoom()
    {
        Debug.Log("zoomin in");
        float time = 0f;

        Vector3 startPosition = CamaraTransform.position;

        Vector3 targetPosition = new Vector3(
            player.position.x,
            player.position.y,
            CamaraTransform.position.z + zoomIn
        );


        while (time < zoomInTime)
        {
            time += Time.deltaTime;

            float progress = time / zoomInTime;

            CamaraTransform.position = Vector3.Lerp(
                startPosition,
                targetPosition,
                progress
            );

            yield return null;
        }
        CamaraTransform.position = targetPosition;
        yield return StartCoroutine(Loading());
    }

    private IEnumerator Loading()
    {
        loadingImage.gameObject.SetActive(true);
        Time.timeScale = 0f;
        float time = 0f;

        while (time<loadingTime)
        {
            time += Time.unscaledDeltaTime;
            loadingImage.transform.Rotate(
                0f,
                0f,
                -velocidadRotacion * Time.unscaledDeltaTime
            );

            yield return null;
        }
        Time.timeScale = 1f;

        MultijugadorJuego.Instancia.CargarSiguienteEscena("LevelYoutubev2");
    }
}
