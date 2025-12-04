using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class DoorController : MonoBehaviour
{
    [Header("--- Configuración General ---")]
    public bool requiresKeyCard = false;
    public string requiredKeyCardID = "DEFAULT_ID";

    [Tooltip("Tiempo antes de cerrarse sola (solo puertas normales).")]
    public float delayBeforeClose = 3.0f;

    [Header("--- Componentes de la Puerta ---")]
    public Animator anim;
    public AudioClip openSound;
    public AudioClip closeSound;

    private AudioSource audioSource;
    private bool isOpen = false;
    private bool isLocked = true;

    void Awake()
    {
        if (anim == null) anim = GetComponentInChildren<Animator>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource != null) audioSource.playOnAwake = false;

        isLocked = requiresKeyCard;
    }

    public bool InteractDoor(string keyCardID)
    {
        if (isOpen) return false;

        if (isLocked)
        {
            if (requiresKeyCard && keyCardID == requiredKeyCardID)
            {
                isLocked = false;
                Debug.Log($"Puerta desbloqueada con KeyCard: {requiredKeyCardID}");
            }
            else
            {
                Debug.Log("Acceso denegado. Falta tarjeta correcta.");
                return false;
            }
        }

        StartCoroutine(OpenRoutine());
        return true;
    }

    private IEnumerator OpenRoutine()
    {
        isOpen = true;

        if (openSound != null && audioSource != null) audioSource.PlayOneShot(openSound);

        if (anim != null) anim.SetTrigger("Open");

        Collider doorCollider = GetComponent<Collider>();
        if (doorCollider != null) doorCollider.enabled = false;

        // Si es una puerta que requiere tarjeta (puerta final), cambiamos de escena
        if (requiresKeyCard)
        {
            yield return new WaitForSeconds(1.0f);
            SceneManager.LoadScene("Escena2");
            yield break;
        }

        // Puertas normales: se cierran solas
        if (delayBeforeClose > 0)
        {
            yield return new WaitForSeconds(delayBeforeClose);

            if (doorCollider != null) doorCollider.enabled = true;
            if (anim != null) anim.SetTrigger("Close");
            if (closeSound != null && audioSource != null) audioSource.PlayOneShot(closeSound);

            isOpen = false;
            Debug.Log("La puerta se cerró automáticamente.");
        }
    }
}