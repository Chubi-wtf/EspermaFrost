using UnityEngine;
using TMPro; 

public class TerminalController : MonoBehaviour
{
    [Header("Configuración de la Puerta")]
    public DoorController targetDoor;
    public string correctCode = "1234";
    public int codeLength = 4;
    // Esta KeyCardID ya no se usa para abrir, sino como referencia del puzzle
    public string keyCardIDToUnlock = "DEFAULT_ID";

    [Header("Recompensa de Ítem")]
    // El objeto 3D de la KeyCard que se activa al ingresar el código correcto
    public GameObject physicalKeyCardToActivate;

    [Header("Componentes de UI")]
    public GameObject terminalCanvas;
    public TextMeshProUGUI codeDisplay;

    [Header("Sonidos")]
    public AudioClip correctSound;
    public AudioClip incorrectSound;
    public AudioClip keyPressSound;

    private AudioSource audioSource;
    private string currentInput = "";
    private bool isTerminalActive = false;
    private const float CLOSE_DELAY = 1.5f;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void Start()
    {
        if (terminalCanvas != null)
        {
            terminalCanvas.SetActive(false);
        }

        // NUEVO: Aseguramos que la KeyCard esté desactivada al inicio del juego
        if (physicalKeyCardToActivate != null)
        {
            physicalKeyCardToActivate.SetActive(false);
        }
    }

    private void Update()
    {
        if (isTerminalActive)
        {
            // Salir de la terminal con la tecla Escape
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                DeactivateTerminal();
            }
        }
    }

    // ===================================================
    // MÉTODOS PÚBLICOS LLAMADOS DESDE PLAYERINTERACTION
    // ===================================================

    public void ActivateTerminal()
    {
        if (isTerminalActive) return;

        isTerminalActive = true;
        terminalCanvas.SetActive(true);
        currentInput = "";
        UpdateDisplay();

        TogglePlayerControl(false);
    }

    public void DeactivateTerminal()
    {
        isTerminalActive = false;
        terminalCanvas.SetActive(false);

        TogglePlayerControl(true);
    }

    // ===================================================
    // MÉTODOS PÚBLICOS LLAMADOS DESDE LOS BOTONES DE UI
    // ===================================================

    public void EnterDigit(string digit)
    {
        if (isTerminalActive && currentInput.Length < codeLength)
        {
            currentInput += digit;
            PlaySound(keyPressSound);
            UpdateDisplay();

            if (currentInput.Length == codeLength)
            {
                CheckCode();
            }
        }
    }

    public void ClearInput()
    {
        currentInput = "";
        PlaySound(keyPressSound);
        UpdateDisplay();
    }

    // ===================================================
    // LÓGICA INTERNA
    // ===================================================

    private void CheckCode()
    {
        if (currentInput == correctCode)
        {
            Debug.Log("Código CORRECTO. KeyCard liberada.");

            // ** LLAMADA CLAVE: Activa el objeto físico para que el jugador lo recoja **
            ReleaseKeyCard();

            // La puerta NO se abre aquí, sino que espera la KeyCard.

            PlaySound(correctSound);
        }
        else
        {
            Debug.Log("Código INCORRECTO.");
            PlaySound(incorrectSound);
        }

        // Espera y luego cierra la terminal
        Invoke("DeactivateTerminal", CLOSE_DELAY);
    }

    private void ReleaseKeyCard()
    {
        if (physicalKeyCardToActivate != null)
        {
            physicalKeyCardToActivate.SetActive(true);
            // El jugador ahora debe ver la KeyCard y usar 'E' para recogerla.
        }
    }

    private void UpdateDisplay()
    {
        string displayString = "";
        for (int i = 0; i < currentInput.Length; i++)
        {
            displayString += "* ";
        }
        codeDisplay.text = displayString;
    }

    private void TogglePlayerControl(bool isGameMode)
    {
        if (isGameMode)
        {
            Cursor.lockState = CursorLockMode.Locked;
            // Usando Singleton:
            if (PlayerMovement.Instance != null) PlayerMovement.Instance.canMove = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            // Usando Singleton:
            if (PlayerMovement.Instance != null) PlayerMovement.Instance.canMove = false;
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
