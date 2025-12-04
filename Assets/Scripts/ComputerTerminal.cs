using UnityEngine;
using TMPro;

public class ComputerTerminal : MonoBehaviour
{
    public string playerTag = "Player";
    public KeyCode interactKey = KeyCode.E;
    private bool playerInRange;

    public Canvas computerCanvas;
    public Canvas loginCanvas;
    public Canvas contentCanvas;
    public TMP_InputField passwordInput;

    public string correctPassword = "1234";

    public TextMeshProUGUI countdownText;
    public int countdownStart = 10;
    private bool countdownRunning = false;

    public Canvas winCanvas;
    public TextMeshProUGUI winText;
    private bool gameEnded = false;

    public TextMeshProUGUI interactText;

    private void Start()
    {
        if (computerCanvas != null)
            computerCanvas.enabled = false;

        if (loginCanvas != null)
            loginCanvas.enabled = false;

        if (contentCanvas != null)
            contentCanvas.enabled = false;

        if (countdownText != null)
        {
            countdownText.text = "";
            countdownText.enabled = false;
        }

        if (winCanvas != null)
            winCanvas.enabled = false;

        if (interactText != null)
            interactText.enabled = false;
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            if (interactText != null)
                interactText.enabled = false;

            ToggleComputer();
        }
    }

    private void ToggleComputer()
    {
        if (computerCanvas == null || gameEnded) return;

        bool opening = !computerCanvas.enabled;
        computerCanvas.enabled = opening;

        if (opening)
        {
            if (loginCanvas != null) loginCanvas.enabled = true;
            if (contentCanvas != null) contentCanvas.enabled = false;

            if (passwordInput != null)
                passwordInput.ActivateInputField();

            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            if (loginCanvas != null) loginCanvas.enabled = false;
            if (contentCanvas != null) contentCanvas.enabled = false;

            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void CheckPassword()
    {
        if (passwordInput == null || gameEnded) return;

        if (passwordInput.text == correctPassword)
        {
            if (loginCanvas != null) loginCanvas.enabled = false;
            if (contentCanvas != null) contentCanvas.enabled = true;
        }
        else
        {
            passwordInput.text = "";
            passwordInput.ActivateInputField();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = true;

            if (!gameEnded && interactText != null)
            {
                interactText.text = "Presiona E para interactuar";
                interactText.enabled = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;

            if (interactText != null)
                interactText.enabled = false;

            if (computerCanvas != null && computerCanvas.enabled)
                ToggleComputer();
        }
    }

    public void StartCountdown()
    {
        if (!countdownRunning && !gameEnded)
        {
            if (computerCanvas != null && computerCanvas.enabled)
                ToggleComputer();

            if (countdownText != null)
                countdownText.enabled = true;

            StartCoroutine(CountdownRoutine());
        }
    }

    private System.Collections.IEnumerator CountdownRoutine()
    {
        countdownRunning = true;

        float currentTime = countdownStart;

        while (currentTime > 0f)
        {
            if (countdownText != null)
                countdownText.text = currentTime.ToString("F2");

            yield return null;
            currentTime -= Time.unscaledDeltaTime;
        }

        if (countdownText != null)
            countdownText.text = "0.00";

        countdownRunning = false;

        gameEnded = true;

        if (winCanvas != null)
            winCanvas.enabled = true;

        if (winText != null)
            winText.text = "¡Has ganado el juego!";

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}