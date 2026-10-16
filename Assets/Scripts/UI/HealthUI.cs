using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [Header("Ссылки на сердечки (по порядку)")]
    public Image[] hearts;

    [Header("Опционально: спрайт полного и пустого сердца")]
    public Sprite fullHeart;
    public Sprite emptyHeart;

    [Header("Откуда брать HP")]
    public PlayerHealth playerHealth;

    private void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += UpdateHearts;
            UpdateHearts(playerHealth.CurrentHP);
        }
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= UpdateHearts;
    }

    private void UpdateHearts(int hp)
    {
        if (hearts == null) return;
        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] == null) continue;

            bool isFull = i < hp;

            if (emptyHeart != null && fullHeart != null)
            {
                hearts[i].sprite = isFull ? fullHeart : emptyHeart;
                hearts[i].enabled = true;
            }
            else
            {
                hearts[i].enabled = isFull;
            }
        }
    }
}
