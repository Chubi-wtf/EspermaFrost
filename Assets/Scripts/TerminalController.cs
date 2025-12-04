using UnityEngine;
using TMPro;

public class TerminalController : MonoBehaviour
{
    [Header("Configuración de la Puerta")]
    public DoorController targetDoor;
    public string correctCode = "1234";
    public int codeLength = 4;
    public string keyCardIDToUnlock = "DEFAULT_ID";

    [Header("Recompensa de Ítem")]
    // [MODIFICADO] Ahora referenciamos la CAJA que cubre la tarjeta
    public GameObject securityBoxToDestroy;

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

        // [MODIFICADO] Nos aseguramos que la caja empiece CERRADA (visible)
        if (securityBoxToDestroy != null)
        {
            securityBoxToDestroy.SetActive(true);
        }
    }

    private void Update()
    {
        if (isTerminalActive)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                DeactivateTerminal();
            }
        }
    }

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

    public void EnterDigit(string digit)
    {
        if (isTerminalActive && currentInput.Length < codeLength)
        {
            currentInput += digit;
            PlaySound(keyPressSound);
            UpdateDisplay();
        }
    }

    public void OnConfirmPressed()
    {
        if (!isTerminalActive) return;

        if (currentInput.Length < codeLength)
        {
            Debug.Log("Código incompleto");
            return;
        }

        CheckCode();
    }

    public void ClearInput()
    {
        currentInput = "";
        PlaySound(keyPressSound);
        UpdateDisplay();
    }

    private void CheckCode()
    {
        if (currentInput == correctCode)
        {
            Debug.Log("Código CORRECTO. Caja de seguridad abierta.");

            // [MODIFICADO] Llamamos a la función que abre la caja
            OpenSecurityBox();

            PlaySound(correctSound);
            Invoke("DeactivateTerminal", CLOSE_DELAY);
        }
        else
        {
            Debug.Log("Código INCORRECTO.");
            PlaySound(incorrectSound);
            currentInput = "";
            UpdateDisplay();
        }
    }

    // [MODIFICADO] Nueva función para eliminar la cubierta
    private void OpenSecurityBox()
    {
        if (securityBoxToDestroy != null)
        {
            // Desactivamos la caja para que se vea la tarjeta que estaba dentro
            securityBoxToDestroy.SetActive(false);

            // Opcional: Si quieres un efecto más genial, podrías destruir el objeto
            // Destroy(securityBoxToDestroy);
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
            if (PlayerMovement.Instance != null) PlayerMovement.Instance.canMove = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
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
