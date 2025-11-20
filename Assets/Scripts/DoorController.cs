using UnityEngine;
using TMPro;

[RequireComponent(typeof(AudioSource))]
public class DoorController : MonoBehaviour
{
    [Header("Componentes")]
    public Animator anim;
    public AudioClip openSound;

    [Header("Configuración de Seguridad")]
    // Si dejas esto vacío en el Inspector, la puerta se abre sin llave.
    // Si pones "Blue", el jugador necesita una KeyCard con ID "Blue".
    public string requiredKeyID;

    [Header("Panel de Victoria")]
    public GameObject victoryPanel;
    public TextMeshProUGUI escapeText;
    public TextMeshProUGUI victoryText;

    private AudioSource audioSource;
    private bool isOpen = false;

    // Quitamos playerInRange porque ahora usamos Raycast (tecla E), 
    // pero lo dejo si quieres mantener compatibilidad híbrida.
    private bool playerInRange = false;

    void Awake()
    {
        if (anim == null)
            anim = GetComponentInChildren<Animator>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource != null) audioSource.playOnAwake = false;

        if (victoryPanel != null)
            victoryPanel.SetActive(false);
    }

    // --- ESTE ES EL MÉTODO QUE FALTABA Y CAUSABA EL ERROR ---
    public bool InteractDoor(string playerKeyID)
    {
        if (isOpen) return false; // Ya está abierta

        // 1. Verificar si la puerta requiere llave
        if (!string.IsNullOrEmpty(requiredKeyID))
        {
            // 2. Verificar si el ID de la llave del jugador coincide
            if (playerKeyID != requiredKeyID)
            {
                // Sonido de "Acceso Denegado" iría aquí
                Debug.Log($"Acceso denegado. Se requiere tarjeta: {requiredKeyID}");
                return false; // Indica que falló la interacción
            }
        }

        // Si llegamos aquí, o no pide llave, o tenemos la correcta
        OpenDoor();
        return true; // Indica éxito
    }
    // -------------------------------------------------------

    public void OpenDoor()
    {
        if (isOpen) return;

        isOpen = true;

        if (anim != null) anim.SetTrigger("Open");

        if (openSound != null && audioSource != null)
            audioSource.PlayOneShot(openSound);

        // Si es la puerta final, mostramos la victoria
        if (victoryPanel != null)
        {
            Invoke("ShowVictoryPanel", 1.0f); // Pequeño delay para ver la puerta abrirse
        }
    }

    private void ShowVictoryPanel()
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);

            if (escapeText != null)
                escapeText.text = "Escapaste... por ahora. Pero sabes que sea lo que sea que se tragó a tu amigo, sigue respirando detrás de ti.";

            if (victoryText != null)
                victoryText.text = "¡Terminaste la demo! El equipo No-Name está muy orgulloso de ti.";

            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // Mantenemos esto por si quieres que TAMBIÉN se abra al chocar (opcional)
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && string.IsNullOrEmpty(requiredKeyID))
        {
            // Solo se abre al chocar si NO requiere llave
            OpenDoor();
        }
    }

    // Métodos de UI
    public void RestartGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}