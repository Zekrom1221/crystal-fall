using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    [Header("UI")]
    public GameObject pausePanel;
    public Button continueButton;
    public Button quitButton;

    public bool IsPaused { get; private set; }
    private int _selectedIndex = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (continueButton != null) continueButton.onClick.AddListener(Resume);
        if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.State.GameOver) return;
            if (IsPaused) Resume();
            else Pause();
            return;
        }

        if (!IsPaused) return;

        if (Keyboard.current.wKey.wasPressedThisFrame)
        {
            _selectedIndex = 0;
            SelectButton();
        }
        else if (Keyboard.current.sKey.wasPressedThisFrame)
        {
            _selectedIndex = 1;
            SelectButton();
        }
        else if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (_selectedIndex == 0) continueButton?.onClick.Invoke();
            else quitButton?.onClick.Invoke();
        }
    }

    private void SelectButton()
    {
        Button target = _selectedIndex == 0 ? continueButton : quitButton;
        if (target != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(target.gameObject);
    }

    public void Pause()
    {
        IsPaused = true;
        _selectedIndex = 0;
        Time.timeScale = 0f;
        if (pausePanel != null) pausePanel.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        SelectButton();
    }

    public void Resume()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        if (pausePanel != null) pausePanel.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
