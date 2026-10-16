using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;

public class StartGameTrigger : MonoBehaviour
{
    [Header("Взаимодействие")]
    public string playerTag = "Player";

    [Header("UI сообщение (один TextMeshProUGUI на всё)")]
    [Tooltip("Используется и для подсказки \"Нажми E\", и для отсчёта 3-2-1-GO.")]
    public TextMeshProUGUI messageText;

    [Header("Тексты")]
    public string interactPromptText = "Нажми E, чтобы начать";
    public string goText = "GO!";

    [Header("Звук отсчёта")]
    public AudioClip countdownSfx;
    [Range(0f, 1f)] public float countdownVolume = 0.18f;

    [Header("Стиль отсчёта")]
    public Color countdownColor = Color.red;
    public float countdownFontSize = 80f;

    [Header("Спавнер")]
    public CrystalSpawner crystalSpawner;

    private bool playerInRange = false;
    private bool gameStarted = false;
    private AudioSource _audioSource;

    private void Start()
    {
        _audioSource = gameObject.AddComponent<AudioSource>();
        HideMessage();
    }

    private void Update()
    {
        if (playerInRange && !gameStarted && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            StartCoroutine(StartCountdown());
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = true;
            if (!gameStarted) ShowPrompt();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;
            if (!gameStarted) HideMessage();
        }
    }

    private IEnumerator StartCountdown()
    {
        gameStarted = true;

        if (messageText != null)
        {
            messageText.color = countdownColor;
            messageText.fontSize = countdownFontSize;
        }

        if (countdownSfx != null) _audioSource.PlayOneShot(countdownSfx, countdownVolume);

        for (int i = 3; i > 0; i--)
        {
            SetMessage(i.ToString());
            yield return new WaitForSeconds(0.75f);
        }

        SetMessage(goText);
        yield return new WaitForSeconds(0.75f);

        HideMessage();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }
        else if (crystalSpawner != null)
        {
            crystalSpawner.StartSpawning();
        }

        var col = GetComponent<BoxCollider2D>();
        if (col != null) col.enabled = false;
    }

    private void ShowPrompt()
    {
        SetMessage(interactPromptText);
    }

    private void SetMessage(string text)
    {
        if (messageText == null) return;
        if (!messageText.gameObject.activeSelf) messageText.gameObject.SetActive(true);
        if (!messageText.enabled) messageText.enabled = true;
        messageText.text = text;
    }

    private void HideMessage()
    {
        if (messageText == null) return;
        messageText.gameObject.SetActive(false);
    }
}
