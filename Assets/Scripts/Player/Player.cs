using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float Min_X = -50f;
    [SerializeField] private float Max_X = 150f;

    private PlayerInputActions _playerInputActions;
    private Rigidbody2D _rigidBody2D;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private BoxCollider2D _collider;
    private Vector2 _baseColliderOffset;

    [Header("Шаги")]
    [SerializeField] private AudioClip footstepClip;

    private bool _inputEnabled = true;
    private AudioSource _footstepSource;
    private PlayerHealth _playerHealth;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _rigidBody2D = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _collider = GetComponent<BoxCollider2D>();
        if (_collider != null) _baseColliderOffset = _collider.offset;
        _playerHealth = GetComponent<PlayerHealth>();

        _playerInputActions = new PlayerInputActions();
        _playerInputActions.Enable();

        _footstepSource = gameObject.AddComponent<AudioSource>();
        _footstepSource.loop = true;
        _footstepSource.playOnAwake = false;
        _footstepSource.volume = 0.5f;
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameOver += HandleGameOver;
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameOver -= HandleGameOver;
    }

    private void HandleGameOver(float survivalTime)
    {
        _inputEnabled = false;
        _playerInputActions?.Disable();
        if (_animator != null) _animator.SetBool("IsRunning", false);
        if (_footstepSource != null) _footstepSource.Stop();
    }

    private Vector2 GetMovementVector()
    {
        if (!_inputEnabled) return Vector2.zero;
        return _playerInputActions.Player.Move.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        Vector2 inputVector = GetMovementVector().normalized;
        _rigidBody2D.MovePosition(_rigidBody2D.position + inputVector * (moveSpeed * Time.fixedDeltaTime));

        bool isRunning = Mathf.Abs(inputVector.x) > 0.1f;
        if (_animator != null) _animator.SetBool("IsRunning", isRunning);

        bool isDead = _playerHealth != null && _playerHealth.IsDead;
        if (isRunning && !isDead && footstepClip != null)
        {
            if (!_footstepSource.isPlaying)
            {
                _footstepSource.clip = footstepClip;
                _footstepSource.Play();
            }
        }
        else if (_footstepSource.isPlaying)
        {
            _footstepSource.Stop();
        }

        if (inputVector.x != 0f && _spriteRenderer != null)
        {
            _spriteRenderer.flipX = inputVector.x < 0f;
        }

        if (_collider != null && _spriteRenderer != null)
        {
            float sign = _spriteRenderer.flipX ? -1f : 1f;
            _collider.offset = new Vector2(_baseColliderOffset.x * sign, _baseColliderOffset.y);
        }
    }

    private void LateUpdate()
    {
        Vector2 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, Min_X, Max_X);
        transform.position = pos;
    }
}
