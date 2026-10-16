using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class GameOverUI : MonoBehaviour
{
    [Header("Корневая панель")]
    public GameObject panel;

    [Header("Текст итогового времени")]
    public TextMeshProUGUI finalTimeText;

    [Header("Рекорд")]
    public TextMeshProUGUI recordText;
    public TextMeshProUGUI newRecordText;

    [Header("Подсказка рестарта")]
    public TextMeshProUGUI restartHintText;

    [Header("Кнопка перезапуска (не используется)")]
    public Button restartButton;

    private bool _isShowing;

    public void Show(float survivalTime)
    {
        if (panel != null && !panel.activeSelf) panel.SetActive(true);
        if (restartButton != null) restartButton.gameObject.SetActive(false);

        if (finalTimeText != null)
        {
            int minutes = Mathf.FloorToInt(survivalTime / 60f);
            int seconds = Mathf.FloorToInt(survivalTime % 60f);
            finalTimeText.text = string.Format("Время выживания: {0:00}:{1:00}", minutes, seconds);
        }

        bool isNew = RecordManager.Instance != null && RecordManager.Instance.TrySetRecord(survivalTime);

        if (recordText != null && RecordManager.Instance != null)
        {
            float best = RecordManager.Instance.BestTime;
            recordText.text = string.Format("Рекорд: {0:00}:{1:00}",
                Mathf.FloorToInt(best / 60f), Mathf.FloorToInt(best % 60f));
        }

        if (newRecordText != null)
            newRecordText.gameObject.SetActive(isNew);

        if (restartHintText != null)
            restartHintText.text = "Нажми E, чтобы начать заново";

        _isShowing = true;
    }

    public void Hide()
    {
        if (panel != null) panel.SetActive(false);
        if (newRecordText != null) newRecordText.gameObject.SetActive(false);
        _isShowing = false;
    }

    private void Update()
    {
        if (_isShowing && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (GameManager.Instance != null)
                GameManager.Instance.RestartScene();
        }
    }
}
