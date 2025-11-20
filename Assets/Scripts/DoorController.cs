using UnityEngine;
using System.Collections; // Para las corrutinas
using TMPro;              // Para los textos de la UI
using UnityEngine.SceneManagement; // Para reiniciar el nivel

[RequireComponent(typeof(AudioSource))]
public class DoorController : MonoBehaviour
{
    [Header("--- Configuración General ---")]
    public bool requiresKeyCard = false;
    public string requiredKeyCardID = "DEFAULT_ID";

    [Tooltip("Tiempo antes de cerrarse sola. Si es la puerta de victoria, esto se ignora.")]
    public float delayBeforeClose = 3.0f;

    [Header("--- Componentes de la Puerta ---")]
    public Animator anim;
    public AudioClip openSound;
    public AudioClip closeSound;

    [Header("--- Configuración de Victoria (Opcional) ---")]
    [Tooltip("Arrastra aquí el panel de victoria SOLO si esta es la puerta final.")]
    public GameObject victoryPanel;
    public TextMeshProUGUI escapeText;
    public TextMeshProUGUI victoryText;

    // Variables internas
    private AudioSource audioSource;
    private bool isOpen = false;
    private bool isLocked = true;

    void Awake()
    {
        // Inicialización de componentes
        if (anim == null) anim = GetComponentInChildren<Animator>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource != null) audioSource.playOnAwake = false;

        // Configuración inicial de bloqueo
        isLocked = requiresKeyCard;

        // Asegurarnos que el panel de victoria empiece apagado
        if (victoryPanel != null) victoryPanel.SetActive(false);
    }

    // --- LÓGICA DE INTERACCIÓN (Del Script 1) ---
    public bool InteractDoor(string keyCardID)
    {
        if (isOpen) return false; // Ya está abierta o abriéndose

        // 1. Comprobación de Bloqueo
        if (isLocked)
        {
            if (requiresKeyCard && keyCardID == requiredKeyCardID)
            {
                isLocked = false; // Desbloqueo permanente
                Debug.Log($"Puerta desbloqueada con KeyCard: {requiredKeyCardID}");
                // Opcional: Sonido de "Desbloqueo" aquí
            }
            else
            {
                Debug.Log("Acceso denegado. Falta tarjeta correcta.");
                // Opcional: Sonido de "Error" aquí
                return false;
            }
        }

        // 2. Iniciar secuencia de apertura
        StartCoroutine(OpenRoutine());
        return true;
    }

    // --- RUTINA PRINCIPAL (Fusionada) ---
    private IEnumerator OpenRoutine()
    {
        isOpen = true;

        // 1. Sonido
        if (openSound != null && audioSource != null) audioSource.PlayOneShot(openSound);

        // 2. Animación
        if (anim != null) anim.SetTrigger("Open");

        // 3. Desactivar Collider (Para poder pasar)
        Collider doorCollider = GetComponent<Collider>();
        if (doorCollider != null) doorCollider.enabled = false;

        // --- AQUÍ OCURRE LA DECISIÓN (Normal vs Victoria) ---

        // CASO A: ES LA PUERTA FINAL (Tiene panel asignado)
        if (victoryPanel != null)
        {
            yield return new WaitForSeconds(1.0f); // Esperamos un poco para ver la puerta abrirse
            ShowVictoryPanel();
            // Aquí termina la corrutina, la puerta NO se cierra nunca.
        }
        // CASO B: ES UNA PUERTA NORMAL (Se cierra sola)
        else
        {
            if (delayBeforeClose > 0)
            {
                yield return new WaitForSeconds(delayBeforeClose);

                // Lógica de cierre (Script 1 original)
                if (doorCollider != null) doorCollider.enabled = true;
                if (anim != null) anim.SetTrigger("Close");
                if (closeSound != null) audioSource.PlayOneShot(closeSound);

                isOpen = false;
                Debug.Log("La puerta se cerró automáticamente.");
            }
        }
    }

    // --- LÓGICA DE VICTORIA (Del Script 2) ---
    private void ShowVictoryPanel()
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);

            // Textos personalizados
            if (escapeText != null)
                escapeText.text = "Escapaste... por ahora. Pero sabes que sea lo que sea que se tragó a tu amigo, sigue respirando detrás de ti.";

            if (victoryText != null)
                victoryText.text = "¡Terminaste la demo! El equipo No-Name está muy orgulloso de ti.";

            // Pausar juego y liberar mouse
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // --- MÉTODOS UI (Para los botones del Panel) ---
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}