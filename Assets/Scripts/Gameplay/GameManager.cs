using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum State { Idle, Playing, GameOver }

    [Header("Ссылки")]
    public CrystalSpawner crystalSpawner;
    public PlayerHealth playerHealth;
    public GameOverUI gameOverUI;

    public State CurrentState { get; private set; } = State.Idle;
    public float SurvivalTime { get; private set; }

    public event Action OnGameStart;
    public event Action<float> OnGameOver;

    private float _gameStartTime;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        if (playerHealth != null)
            playerHealth.OnDeath += HandlePlayerDeath;
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnDeath -= HandlePlayerDeath;
    }

    private void Update()
    {
        if (CurrentState == State.Playing)
        {
            SurvivalTime = Time.time - _gameStartTime;
        }
    }

    public void StartGame()
    {
        if (CurrentState != State.Idle) return;

        CurrentState = State.Playing;
        _gameStartTime = Time.time;
        SurvivalTime = 0f;

        if (crystalSpawner != null) crystalSpawner.StartSpawning();

        OnGameStart?.Invoke();
    }

    private void HandlePlayerDeath()
    {
        EndGame();
    }

    public void EndGame()
    {
        if (CurrentState == State.GameOver) return;

        CurrentState = State.GameOver;

        if (crystalSpawner != null) crystalSpawner.StopSpawning();

        if (gameOverUI != null) gameOverUI.Show(SurvivalTime);

        OnGameOver?.Invoke(SurvivalTime);
    }

    public void RestartScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
