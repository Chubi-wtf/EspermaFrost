using UnityEngine;
using UnityEngine.UI;

public class Health_UI : MonoBehaviour
{
    [Header("REFERENCIAS")]
    public PlayerMovement player;
    public Slider healthSlider, healthSlider2;  
    public Image fillImage, fillImage2;       

    [Header("SPRITES SEGÚN VIDA")]
    public Sprite healthySprite, healthySprite2;  
    public Sprite mediumSprite, mediumSprite2;    
    public Sprite lowSprite, lowSprite2;       

    private void Start()
    {
        if (player == null)
            Debug.LogError("HealthUI no tiene asignado el PlayerMovement.");

        healthSlider.maxValue = player.maxHealth;
        healthSlider2.maxValue = player.maxHealth;
    }

    private void Update()
    {
        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        healthSlider.value = player.currentHealth;
        healthSlider2.value = player.currentHealth;

        float lifePercent = player.currentHealth / player.maxHealth;

        if (lifePercent >= 0.66f)
        {
            fillImage.sprite = healthySprite;
            fillImage2.sprite = healthySprite2;
        }
        else if (lifePercent >= 0.33f)
        {
            fillImage.sprite = mediumSprite;
            fillImage2.sprite = mediumSprite2;
        }
        else
        {
            fillImage.sprite = lowSprite;
            fillImage2.sprite = lowSprite2;
        }
    }
}
