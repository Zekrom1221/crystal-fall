using UnityEngine;
using TMPro;

public class TimerUI : MonoBehaviour
{
    public TextMeshProUGUI timerText;

    private void Update()
    {
        if (timerText == null) return;
        if (GameManager.Instance == null) return;

        float t = GameManager.Instance.SurvivalTime;
        int minutes = Mathf.FloorToInt(t / 60f);
        int seconds = Mathf.FloorToInt(t % 60f);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
