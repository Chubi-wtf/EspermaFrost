using UnityEngine;
using TMPro;

public class ComputerTerminal : MonoBehaviour
{
    [Header("Detección del jugador")]
    public string playerTag = "Player";
    public KeyCode interactKey = KeyCode.E;
    private bool playerInRange;

    [Header("UI (Canvas)")]
    public Canvas computerCanvas;
    public Canvas loginCanvas;
    public Canvas contentCanvas;
    public TMP_InputField passwordInput;

    [Header("Lógica")]
    public string correctPassword = "1234";

    private void Start()
    {
        if (computerCanvas != null)
            computerCanvas.enabled = false;

        if (loginCanvas != null)
            loginCanvas.enabled = false;

        if (contentCanvas != null)
            contentCanvas.enabled = false;
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(interactKey))
            ToggleComputer();
    }

    private void ToggleComputer()
    {
        if (computerCanvas == null) return;

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
        if (passwordInput == null) return;

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
            playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;

            if (computerCanvas != null && computerCanvas.enabled)
                ToggleComputer();
        }
    }
}
