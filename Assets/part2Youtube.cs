using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class part2Youtube : MonoBehaviour
{
    [SerializeField] private Image loadingImage;
    [SerializeField] private float loadingTime = 10f;
    [SerializeField] private float velocidadRotacion = 180f;

    [SerializeField] private float zoomIn = 5f;
    [SerializeField] private Transform player;
    [SerializeField] private float zoomInTime = 2f;

    [SerializeField] private GameObject camera;

    private bool toStart = false;
    private void Start()
    {
        loadingImage.gameObject.SetActive(false);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
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

        Vector3 startPosition = camera.transform.position;

        Vector3 targetPosition = new Vector3(
            player.position.x,
            player.position.y,
            camera.transform.position.z + zoomIn
        );


        while (time < zoomInTime)
        {
            time += Time.deltaTime;

            float progress = time / zoomInTime;

            camera.transform.position = Vector3.Lerp(
                startPosition,
                targetPosition,
                progress
            );

            yield return null;
        }
        camera.transform.position = targetPosition;
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
        SceneManager.LoadScene("LevelYoutubev2");
    }
}
