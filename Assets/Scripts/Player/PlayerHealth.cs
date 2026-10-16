using UnityEngine;
using System;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Здоровье")]
    public int maxHP = 3;
    public float invincibilityDuration = 0.5f;

    [Header("Смерть")]
    [Tooltip("Фиксированная мировая Y-координата игрока в момент смерти.")]
    public float deathY = -3.8f;

    [Header("Щит")]
    public float shieldDuration = 5f;
    public GameObject shieldAura;
    public float shieldBaseY = 0.249f;
    [Tooltip("Сдвиг щита по X, когда игрок стоит. Компенсирует асимметрию спрайта с посохом.")]
    public float idleShieldOffsetX = 0.15f;

    [Header("Визуал урона")]
    public SpriteRenderer spriteRenderer;
    public float blinkInterval = 0.08f;
    public Color damageFlashColor = new Color(1f, 0.3f, 0.3f, 1f);

    [Header("Камера")]
    public CameraShaker cameraShaker;
    public float damageShakeDuration = 0.2f;
    public float damageShakeMagnitude = 0.15f;

    [Header("Звуки (опционально)")]
    public AudioSource audioSource;
    public AudioClip damageSfx;
    public AudioClip healSfx;
    public AudioClip shieldSfx;
    public AudioClip deathSfx;

    public int CurrentHP { get; private set; }
    public bool IsShielded { get; private set; }
    public float ShieldRemainingTime { get; private set; }
    public bool IsInvincible { get; private set; }
    public bool IsDead { get; private set; }

    public event Action<int> OnHealthChanged;
    public event Action<bool, float> OnShieldChanged;
    public event Action OnDeath;

    private Coroutine _blinkRoutine;
    private Color _originalColor = Color.white;
    private Animator _animator;

    private void Awake()
    {
        CurrentHP = maxHP;
        if (spriteRenderer != null) _originalColor = spriteRenderer.color;
        if (shieldAura != null) shieldAura.SetActive(false);
        _animator = GetComponent<Animator>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
    }

    private void Start()
    {
        OnHealthChanged?.Invoke(CurrentHP);
    }

    private void Update()
    {
        if (IsShielded)
        {
            ShieldRemainingTime -= Time.deltaTime;
            OnShieldChanged?.Invoke(true, ShieldRemainingTime);
            if (ShieldRemainingTime <= 0f)
            {
                DeactivateShield();
            }
        }
    }

    private void LateUpdate()
    {
        if (!IsShielded || shieldAura == null) return;

        bool isRunning = _animator != null && _animator.GetBool("IsRunning");
        float offsetX = 0f;
        if (!isRunning && spriteRenderer != null)
        {
            offsetX = spriteRenderer.flipX ? idleShieldOffsetX : -idleShieldOffsetX;
        }
        shieldAura.transform.localPosition = new Vector3(offsetX, shieldBaseY, 0f);
    }

    public void TakeDamage()
    {
        if (IsDead || IsShielded || IsInvincible) return;

        CurrentHP = Mathf.Max(0, CurrentHP - 1);
        OnHealthChanged?.Invoke(CurrentHP);

        PlaySfx(damageSfx);
        if (cameraShaker != null) cameraShaker.Shake(damageShakeDuration, damageShakeMagnitude);

        if (CurrentHP == 0)
        {
            Die();
        }
        else
        {
            StartInvincibility();
        }
    }

    public void Heal()
    {
        if (IsDead || CurrentHP >= maxHP) return;
        CurrentHP++;
        OnHealthChanged?.Invoke(CurrentHP);
        PlaySfx(healSfx);
    }

    public void ActivateShield()
    {
        if (IsDead) return;

        PlaySfx(shieldSfx);
        IsShielded = true;
        ShieldRemainingTime = shieldDuration;
        if (shieldAura != null) shieldAura.SetActive(true);

        OnShieldChanged?.Invoke(true, ShieldRemainingTime);
    }

    private void DeactivateShield()
    {
        IsShielded = false;
        ShieldRemainingTime = 0f;
        if (shieldAura != null) shieldAura.SetActive(false);
        OnShieldChanged?.Invoke(false, 0f);
    }

    private void StartInvincibility()
    {
        if (_blinkRoutine != null) StopCoroutine(_blinkRoutine);
        _blinkRoutine = StartCoroutine(InvincibilityRoutine());
    }

    private IEnumerator InvincibilityRoutine()
    {
        IsInvincible = true;
        float elapsed = 0f;
        bool flashed = false;
        while (elapsed < invincibilityDuration)
        {
            if (spriteRenderer != null)
                spriteRenderer.color = flashed ? _originalColor : damageFlashColor;
            flashed = !flashed;
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }
        if (spriteRenderer != null) spriteRenderer.color = _originalColor;
        IsInvincible = false;
    }

    private void Die()
    {
        IsDead = true;
        if (deathSfx != null)
            AudioSource.PlayClipAtPoint(deathSfx, transform.position, 0.35f);
        DeactivateShield();
        if (_blinkRoutine != null) StopCoroutine(_blinkRoutine);
        if (spriteRenderer != null) spriteRenderer.color = _originalColor;

        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
        var pos = transform.position;
        pos.y = deathY;
        transform.position = pos;

        if (_animator != null) _animator.SetTrigger("Die");
        OnDeath?.Invoke();
    }

    private void PlaySfx(AudioClip clip, float volume = 0.6f)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip, volume);
    }
}
