using UnityEngine;
using TMPro;

public class RecordHUD : MonoBehaviour
{
    private TextMeshProUGUI _text;

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        Refresh();
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameOver += OnGameOver;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameOver -= OnGameOver;
    }

    private void OnGameOver(float _) => Refresh();

    private void Refresh()
    {
        if (_text == null || RecordManager.Instance == null) return;
        float best = RecordManager.Instance.BestTime;
        _text.text = best > 0f
            ? string.Format("Рекорд: {0:00}:{1:00}", Mathf.FloorToInt(best / 60f), Mathf.FloorToInt(best % 60f))
            : "Рекорд: --:--";
    }
}
