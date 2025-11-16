using UnityEngine;
using UnityEngine.UI;

public class Health_UI : MonoBehaviour
{
    [Header("REFERENCIAS")]
    public PlayerMovement player; // PlayerMovement del jugador
    public Slider healthSlider;   // Slider de vida
    public Image fillImage;       // Imagen del Fill del slider

    [Header("SPRITES SEGÚN VIDA")]
    public Sprite healthySprite;   // Vida alta
    public Sprite mediumSprite;    // Vida media
    public Sprite lowSprite;       // Vida baja

    private void Start()
    {
        if (player == null)
            Debug.LogError("HealthUI no tiene asignado el PlayerMovement.");

        healthSlider.maxValue = player.maxHealth;
    }

    private void Update()
    {
        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        // Actualiza valor del slider
        healthSlider.value = player.currentHealth;

        float lifePercent = player.currentHealth / player.maxHealth;

        // Cambia sprite según la vida
        if (lifePercent >= 0.66f)
        {
            fillImage.sprite = healthySprite;
        }
        else if (lifePercent >= 0.33f)
        {
            fillImage.sprite = mediumSprite;
        }
        else
        {
            fillImage.sprite = lowSprite;
        }
    }
}
