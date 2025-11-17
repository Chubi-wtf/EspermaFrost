using UnityEngine;
using TMPro;

[RequireComponent(typeof(AudioSource))]
public class DoorController : MonoBehaviour
{
    [Header("Componentes")]
    public Animator anim;
    public AudioClip openSound;

    [Header("Panel de Victoria")]
    public GameObject victoryPanel;
    public TextMeshProUGUI escapeText;
    public TextMeshProUGUI victoryText;

    private AudioSource audioSource;
    private bool isOpen = false;

    void Awake()
    {
        // Busca el Animator (si no está asignado)
        if (anim == null)
        {
            anim = GetComponentInChildren<Animator>();
        }

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // Ocultar panel al inicio
        if (victoryPanel != null)
            victoryPanel.SetActive(false);
    }

    // Esta es la función que llamará el Player
    public void InteractDoor()
    {
        // Si ya está abierta, no hacemos nada
        if (isOpen) return;

        // Marcamos como abierta
        isOpen = true;

        // Activamos la animación
        if (anim != null)
        {
            anim.SetTrigger("Open");
        }

        // Reproducimos el sonido
        if (openSound != null)
        {
            audioSource.PlayOneShot(openSound);
        }

        // Mostrar panel de victoria después de un pequeño delay
        Invoke("ShowVictoryPanel", 0f);
    }

    private void ShowVictoryPanel()
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);

            // Configurar textos
            if (escapeText != null)
                escapeText.text = "Escapaste, por ahora...";

            if (victoryText != null)
                victoryText.text = "!Victoria! Gracias por probar esta demo, hecho por el equipo de No-Name";

            // Pausar el juego
            Time.timeScale = 0f;

            // Liberar cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // Método para reiniciar el juego (puedes conectar esto a un botón en el panel)
    public void RestartGame()
    {
        Time.timeScale = 1f;
        // Reiniciar la escena actual
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    // Método para salir del juego (puedes conectar esto a un botón en el panel)
    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}