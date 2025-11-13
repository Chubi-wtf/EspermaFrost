using UnityEngine;
using UnityEngine.UI;

public class UI_ItemsReactionScript : MonoBehaviour
{ 
    [Header("REFERENCIAS UI")]
    public Slider adrenalineSlider;
    public GameObject adrenalineMeterPanel;

    [Header("CONFIGURACIÓN")]
    public float fadeInSpeed = 2f;
    public float fadeOutSpeed = 1.5f;
    public float fadeOutDelay = 0.5f;

    private CanvasGroup canvasGroup;
    private PlayerMovement playerMovement;
    private bool isVisible = false;
    private float fadeOutTimer = 0f;

    private void Start()
    {
        // Obtener referencias usando el método no obsoleto
        canvasGroup = adrenalineMeterPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = adrenalineMeterPanel.AddComponent<CanvasGroup>();
        }

        // Usar FindFirstObjectByType en lugar del método obsoleto
        playerMovement = FindFirstObjectByType<PlayerMovement>();

        if (playerMovement == null)
        {
            Debug.LogError("No se encontró PlayerMovement en la escena!");
            return;
        }

        // Configurar estado inicial
        if (adrenalineMeterPanel != null)
        {
            adrenalineMeterPanel.SetActive(true);
        }

        if (adrenalineSlider != null)
        {
            adrenalineSlider.maxValue = playerMovement.maxStamina;
            adrenalineSlider.value = playerMovement.currentStamina;
        }

        // Ocultar inicialmente
        HideMeterImmediate();
    }

    private void Update()
    {
        if (playerMovement == null) return;

        // Actualizar valor del slider con la estamina actual
        if (adrenalineSlider != null)
        {
            adrenalineSlider.value = playerMovement.currentStamina;
        }

        // Mostrar u ocultar el medidor según el estado de carrera
        HandleMeterVisibility();

        // Manejar el fade out después de dejar de correr
        HandleFadeOut();
    }

    private void HandleMeterVisibility()
    {
        // Ahora podemos acceder a las variables porque son públicas
        bool shouldBeVisible = playerMovement.isRunning && !playerMovement.isStaminaEmpty;

        if (shouldBeVisible && !isVisible)
        {
            // Empezar a mostrar el medidor
            ShowMeter();
        }
        else if (!shouldBeVisible && isVisible)
        {
            // Programar el ocultamiento después del delay
            fadeOutTimer = fadeOutDelay;
        }
    }

    private void HandleFadeOut()
    {
        if (fadeOutTimer > 0)
        {
            fadeOutTimer -= Time.deltaTime;

            if (fadeOutTimer <= 0 && isVisible)
            {
                // Empezar a ocultar el medidor
                HideMeter();
            }
        }
    }

    private void ShowMeter()
    {
        isVisible = true;
        fadeOutTimer = 0f;

        if (adrenalineMeterPanel != null)
        {
            adrenalineMeterPanel.SetActive(true);
        }

        StopAllCoroutines();
        StartCoroutine(FadeIn());
    }

    private void HideMeter()
    {
        isVisible = false;
        StopAllCoroutines();
        StartCoroutine(FadeOut());
    }

    private void HideMeterImmediate()
    {
        isVisible = false;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
        if (adrenalineMeterPanel != null)
        {
            adrenalineMeterPanel.SetActive(false);
        }
    }

    private System.Collections.IEnumerator FadeIn()
    {
        if (canvasGroup == null) yield break;

        canvasGroup.alpha = 0f;

        while (canvasGroup.alpha < 1f)
        {
            canvasGroup.alpha += fadeInSpeed * Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    private System.Collections.IEnumerator FadeOut()
    {
        if (canvasGroup == null) yield break;

        float startAlpha = canvasGroup.alpha;

        while (canvasGroup.alpha > 0f)
        {
            canvasGroup.alpha -= fadeOutSpeed * Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = 0f;

        if (adrenalineMeterPanel != null)
        {
            adrenalineMeterPanel.SetActive(false);
        }
    }

    // Método para forzar la visualización
    public void ForceShowMeter()
    {
        ShowMeter();
    }

    // Método para forzar el ocultamiento
    public void ForceHideMeter()
    {
        HideMeterImmediate();
    }
}