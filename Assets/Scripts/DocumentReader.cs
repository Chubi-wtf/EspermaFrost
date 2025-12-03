using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DocumentReader : MonoBehaviour
{
    public string playerTag = "Player";
    public KeyCode interactKey = KeyCode.E;
    private bool playerInRange;

    public Canvas documentCanvas;
    public Image documentImage;
    public Sprite documentSprite;

    public TextMeshProUGUI interactText;

    private bool isOpen = false;

    private void Start()
    {
        if (documentCanvas != null)
            documentCanvas.enabled = false;

        if (interactText != null)
            interactText.enabled = false;
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            if (!isOpen)
            {
                if (interactText != null)
                    interactText.enabled = false;

                OpenDocument();
            }
            else
            {
                CloseDocument();
            }
        }

        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseDocument();
        }
    }

    private void OpenDocument()
    {
        if (documentCanvas == null) return;

        documentCanvas.enabled = true;
        isOpen = true;

        if (documentImage != null && documentSprite != null)
            documentImage.sprite = documentSprite;

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void CloseDocument()
    {
        if (documentCanvas == null) return;

        documentCanvas.enabled = false;
        isOpen = false;

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = true;

            if (interactText != null)
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

            if (isOpen)
                CloseDocument();
        }
    }
}
