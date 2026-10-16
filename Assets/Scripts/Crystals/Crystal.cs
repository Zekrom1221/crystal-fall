using UnityEngine;

public class Crystal : MonoBehaviour
{
    public enum CrystalType { Red, Green, Blue }

    [Header("Тип кристалла")]
    public CrystalType type = CrystalType.Red;

    [Header("Тэг игрока")]
    public string playerTag = "Player";

    [Header("Эффект при подборе")]
    public GameObject pickupParticlesPrefab;

    [Header("Звук при подборе (опционально, если нет AudioSource на игроке)")]
    public AudioClip pickupSfx;
    [Range(0f, 1f)] public float sfxVolume = 0.6f;

    private bool _consumed;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_consumed) return;
        if (!other.CompareTag(playerTag)) return;

        var health = other.GetComponent<PlayerHealth>();
        if (health == null) return;

        bool lethal = false;
        switch (type)
        {
            case CrystalType.Red:
                if (!health.IsShielded && !health.IsInvincible && !health.IsDead)
                {
                    health.TakeDamage();
                    lethal = health.IsDead;
                }
                break;
            case CrystalType.Green:
                health.Heal();
                break;
            case CrystalType.Blue:
                health.ActivateShield();
                break;
        }

        SpawnPickupFx(skipSfx: lethal);
        _consumed = true;
        Destroy(gameObject);
    }

    private void SpawnPickupFx(bool skipSfx = false)
    {
        if (pickupParticlesPrefab != null)
        {
            Instantiate(pickupParticlesPrefab, transform.position, Quaternion.identity);
        }
        if (!skipSfx && pickupSfx != null)
        {
            AudioSource.PlayClipAtPoint(pickupSfx, transform.position, sfxVolume);
        }
    }

    private void OnBecameInvisible()
    {
        if (this == null || !gameObject.scene.isLoaded) return;
        Destroy(gameObject);
    }
}
