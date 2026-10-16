using UnityEngine;
using System.Collections;

public class CrystalSpawner : MonoBehaviour
{
    [Header("Кристаллы (префабы)")]
    public GameObject redCrystal;
    public GameObject greenCrystal;
    public GameObject blueCrystal;

    [Header("За кем следит спавнер")]
    public Transform followTarget;

    [Header("Зона спавна")]
    [Range(3f, 20f)] public float spawnWidth = 10f;
    [Tooltip("На сколько единиц выше камеры спавнить кристаллы")]
    public float heightAboveCamera = 1.5f;

    [Header("Фолбэк, если DifficultyManager не назначен")]
    [Range(0.3f, 3f)] public float fallbackSpawnInterval = 0.9f;
    [Range(1f, 6f)] public float fallbackFallSpeed = 1f;

    [Header("Доли зелёного/синего от не-красной части")]
    [Tooltip("При progress=0: (1-redWeight) делится между green и blue в этой пропорции")]
    [Range(0f, 1f)] public float greenShareOfNonRed = 0.55f;

    [Header("Фолбэк cooldown'ов (если DifficultyManager не назначен)")]
    [Tooltip("Минимум секунд между зелёными. Лишние заменяются на синий (или красный при щите).")]
    public float fallbackMinGreenInterval = 1.5f;
    [Tooltip("Минимум секунд между синими. Лишние заменяются на зелёный (или красный при щите).")]
    public float fallbackMinBlueInterval = 1.5f;

    [Header("Опционально")]
    public DifficultyManager difficultyManager;

    private bool _isSpawning;
    private Camera _mainCamera;
    private float _spawnStartTime;
    private float _lastGreenSpawnTime = -999f;
    private float _lastBlueSpawnTime = -999f;

    private void Awake()
    {
        _mainCamera = Camera.main;
        if (difficultyManager == null) difficultyManager = DifficultyManager.Instance;
    }

    public void StartSpawning()
    {
        if (_isSpawning) return;
        _isSpawning = true;
        _spawnStartTime = Time.time;
        StartCoroutine(SpawnRoutine());
    }

    public void StopSpawning()
    {
        _isSpawning = false;
    }

    private IEnumerator SpawnRoutine()
    {
        while (_isSpawning)
        {
            SpawnOne();
            float interval = GetSpawnInterval();
            yield return new WaitForSeconds(interval);
        }
    }

    private void SpawnOne()
    {
        float gameTime = Time.time - _spawnStartTime;

        float centerX = followTarget != null ? followTarget.position.x : transform.position.x;
        float randomX = centerX + Random.Range(-spawnWidth / 2f, spawnWidth / 2f);

        float spawnY = GetSpawnY();
        Vector2 spawnPosition = new Vector2(randomX, spawnY);

        GameObject prefab = PickCrystalPrefab(gameTime);
        if (prefab == null) return;

        GameObject newCrystal = Instantiate(prefab, spawnPosition, Quaternion.identity);
        ApplyFallSpeed(newCrystal, gameTime);
    }

    private float GetSpawnY()
    {
        if (_mainCamera != null)
        {
            float camTop = _mainCamera.transform.position.y + _mainCamera.orthographicSize;
            return camTop + heightAboveCamera;
        }
        return transform.position.y;
    }

    private float GetSpawnInterval()
    {
        if (difficultyManager != null)
        {
            float gameTime = Time.time - _spawnStartTime;
            return difficultyManager.GetCurrentSpawnInterval(gameTime);
        }
        return fallbackSpawnInterval;
    }

    private GameObject PickCrystalPrefab(float gameTime)
    {
        float redWeight = difficultyManager != null
            ? difficultyManager.GetCurrentRedWeight(gameTime)
            : 0.33f;

        float nonRed = 1f - redWeight;
        float greenWeight = nonRed * greenShareOfNonRed;

        bool shielded = IsPlayerShielded();
        float r = Random.value;

        GameObject pick;
        if (r < redWeight)
            pick = redCrystal;
        else if (r < redWeight + greenWeight)
            pick = greenCrystal;
        else
            pick = shielded ? greenCrystal : blueCrystal;

        float minGreen = difficultyManager != null
            ? difficultyManager.GetCurrentMinGreenInterval(gameTime)
            : fallbackMinGreenInterval;
        float minBlue = difficultyManager != null
            ? difficultyManager.GetCurrentMinBlueInterval(gameTime)
            : fallbackMinBlueInterval;

        if (pick == greenCrystal)
        {
            if (Time.time - _lastGreenSpawnTime < minGreen)
                pick = shielded ? redCrystal : blueCrystal;
            else
                _lastGreenSpawnTime = Time.time;
        }

        if (pick == blueCrystal)
        {
            if (Time.time - _lastBlueSpawnTime < minBlue)
                pick = redCrystal;
            else
                _lastBlueSpawnTime = Time.time;
        }

        return pick;
    }

    private bool IsPlayerShielded()
    {
        var gm = GameManager.Instance;
        return gm != null && gm.playerHealth != null && gm.playerHealth.IsShielded;
    }

    private void ApplyFallSpeed(GameObject crystal, float gameTime)
    {
        var rb = crystal.GetComponent<Rigidbody2D>();
        if (rb == null) return;

        float multiplier = difficultyManager != null
            ? difficultyManager.GetCurrentFallSpeed(gameTime)
            : fallbackFallSpeed;

        rb.gravityScale = multiplier;
    }
}
