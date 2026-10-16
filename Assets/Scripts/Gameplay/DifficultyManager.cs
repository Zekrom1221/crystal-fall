using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    [Header("Интервал спавна (сек между кристаллами)")]
    public float initialSpawnInterval = 0.9f;
    public float minSpawnInterval = 0.25f;

    [Header("Множитель скорости падения")]
    public float initialFallSpeed = 1f;
    public float maxFallSpeed = 3f;

    [Header("Доля красных кристаллов")]
    [Range(0f, 1f)] public float initialRedWeight = 0.33f;
    [Range(0f, 1f)] public float maxRedWeight = 0.6f;

    [Header("Длительность нарастания (сек)")]
    public float rampDuration = 60f;

    [Header("Минимум секунд между зелёными кристаллами")]
    [Tooltip("Стартовый интервал при gameTime=0")] public float greenInitialMinInterval = 1.5f;
    [Tooltip("Потолок — дальше не растёт")] public float greenMaxMinInterval = 10f;
    [Tooltip("На сколько секунд увеличивается интервал за каждую минуту игры")] public float greenSecondsPerMinute = 1f;

    [Header("Минимум секунд между синими кристаллами")]
    public float blueInitialMinInterval = 1.5f;
    public float blueMaxMinInterval = 10f;
    public float blueSecondsPerMinute = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private float Progress(float gameTime)
    {
        return Mathf.Clamp01(gameTime / rampDuration);
    }

    public float GetCurrentSpawnInterval(float gameTime)
    {
        return Mathf.Lerp(initialSpawnInterval, minSpawnInterval, Progress(gameTime));
    }

    public float GetCurrentFallSpeed(float gameTime)
    {
        return Mathf.Lerp(initialFallSpeed, maxFallSpeed, Progress(gameTime));
    }

    public float GetCurrentRedWeight(float gameTime)
    {
        return Mathf.Lerp(initialRedWeight, maxRedWeight, Progress(gameTime));
    }

    public float GetCurrentMinGreenInterval(float gameTime)
    {
        return ComputeMinInterval(gameTime, greenInitialMinInterval, greenMaxMinInterval, greenSecondsPerMinute);
    }

    public float GetCurrentMinBlueInterval(float gameTime)
    {
        return ComputeMinInterval(gameTime, blueInitialMinInterval, blueMaxMinInterval, blueSecondsPerMinute);
    }

    private static float ComputeMinInterval(float gameTime, float initial, float max, float secondsPerMinute)
    {
        float scaled = gameTime / 60f * secondsPerMinute;
        return Mathf.Clamp(Mathf.Max(initial, scaled), initial, max);
    }
}
