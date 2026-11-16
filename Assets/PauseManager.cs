using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    private bool isPaused = false;
    private PauseButton selectedPauseButton;
    [SerializeField] private GameObject pauseScreen;
    [SerializeField] private GameObject xboxMenu;
    [SerializeField] private GameObject configMenu;
    [SerializeField] private GameObject mapMenu;
    [SerializeField] private GameObject invMenu;
    [SerializeField] private GameObject exitMenu;

    [SerializeField] private GameObject map;

    [SerializeField] private GameObject hover;
    private RectTransform target;

    void Start()
    {
        pauseScreen.SetActive(false);
        map.SetActive(false);
    }
    void Update()
    {
        if (target != null)
        {
            RectTransform hoverRect = hover.GetComponent<RectTransform>();

            Vector3 offset = new Vector3(0f, -40f, 0f);

            Vector3 targetPosition = target.position + offset;

            hoverRect.position = Vector3.Lerp(
                hoverRect.position,
                targetPosition,
                Time.unscaledDeltaTime * 10f
            );
        }
        if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
        {
            Debug.Log("se persiono p");
            isPaused = !isPaused;

            pauseScreen.SetActive(isPaused);

            Time.timeScale = isPaused ? 0f : 1f;
            Cursor.visible = isPaused;
            Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;

        }
    }

    public void CambiarxboxMenu()
    {
        xboxMenu.SetActive(true);
        configMenu.SetActive(false );
        mapMenu.SetActive(false);
        exitMenu.SetActive(false);
        invMenu.SetActive(false);
        map.SetActive(false);
    }

    public void CambiarconfigMenu()
    {
        configMenu.SetActive(true);
        xboxMenu.SetActive(false) ;
        mapMenu.SetActive(false);
        exitMenu.SetActive(false);
        invMenu.SetActive(false);
        map.SetActive(false);
    }

    public void CambiarmapMenu()
    {
        map.SetActive(true);
        mapMenu.SetActive(true);
        exitMenu.SetActive(false);
        invMenu.SetActive(false);
        configMenu.SetActive(false ) ;
        xboxMenu.SetActive(false);
    }

    public void CambiarinvMenu()
    {
        invMenu.SetActive(true);
        configMenu.SetActive(false );
        exitMenu.SetActive(false );
        mapMenu.SetActive(false );
        xboxMenu.SetActive (false);
        map.SetActive(false);
    }

    public void CambiarexitMenu()
    {
        exitMenu.SetActive (true);
        xboxMenu.SetActive(false);
        mapMenu.SetActive(false);
        configMenu.SetActive(false );
        invMenu.SetActive(false );
        map.SetActive(false);
    }

    public void SelectedButton(RectTransform valor)
    {
        if (valor == null) return;

        target = valor;

        Button btn = valor.GetComponent<Button>();
        if (btn != null)
        {
            btn.Select();
        }
        PauseButton button = valor.GetComponent<PauseButton>();
        if (button != null)
        {
            changeSprite(button);
        }
    }

    public void changeSprite(PauseButton button)
    {
        if (button == null) return;

        if (selectedPauseButton != null)
        {
            selectedPauseButton.SetSelected(false);
        }

        selectedPauseButton = button;
        selectedPauseButton.SetSelected(true);
    }

}
