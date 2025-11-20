using UnityEngine;
using System.Collections; // Necesario para la corrutina de abrir/cerrar

[RequireComponent(typeof(AudioSource))]
public class DoorController : MonoBehaviour
{
    [Header("Configuración de KeyCard")]
    public bool requiresKeyCard = false; // Necesita KeyCard?
    public string requiredKeyCardID = "DEFAULT_ID"; // ID que debe tener la tarjeta para abrir
    public float delayBeforeClose; // Tiempo que la puerta permanece abierta antes de cerrarse

    [Header("Componentes")]
    public Animator anim;
    public AudioClip openSound;
    public AudioClip closeSound; // Añadido un sonido de cierre para un loop completo

    private AudioSource audioSource;
    private bool isOpen = false;
    private bool isLocked = true; // Si requiere tarjeta, está inicialmente bloqueada

    void Awake()
    {
        if (anim == null)
        {
            anim = GetComponentInChildren<Animator>();
        }

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // Determina si está bloqueada al inicio
        if (requiresKeyCard)
        {
            isLocked = true;
        }
        else
        {
            isLocked = false;
        }
    }

    // Método llamado por el jugador. Devuelve true si la acción fue exitosa.
    public bool InteractDoor(string keyCardID)
    {
        if (isOpen)
        {
            // Si ya está abierta, la cerramos (o no hacemos nada, dependiendo del diseño)
            return false;
        }

        // 1. COMPROBACIÓN DE BLOQUEO
        if (isLocked)
        {
            // Si está bloqueada, chequeamos si la KeyCard coincide
            if (requiresKeyCard && keyCardID == requiredKeyCardID)
            {
                isLocked = false; // Desbloqueada permanentemente después del uso exitoso
                Debug.Log($"Puerta desbloqueada con KeyCard: {requiredKeyCardID}");
            }
            else
            {
                Debug.Log("La puerta está bloqueada y requiere una KeyCard.");
                return false; // No tiene la KeyCard correcta
            }
        }

        // 2. ABRIR (Ahora que sabemos que está desbloqueada)
        StartCoroutine(OpenAndCloseRoutine());

        return true; // Interacción exitosa
    }

    // Implementación sin animaciones (Desactivar/Activar)
    private IEnumerator OpenAndCloseRoutine()
    {
        isOpen = true;

        // ** SIMULACIÓN DE APERTURA (sin Animación)**

        // 1. Sonido de Apertura
        if (openSound != null)
        {
            audioSource.PlayOneShot(openSound);
        }

        // 2. DESACTIVAR el Collider de la puerta (simulando que se abre y deja pasar)
        Collider doorCollider = GetComponent<Collider>();
        if (doorCollider != null) doorCollider.enabled = false;

        // 3. Simulación de animación (Si no tienes animador)
        if (anim != null)
        {
            anim.SetTrigger("Open"); // Si tienes animador, úsalo.
        }

        // Si la puerta es temporal, esperamos y la cerramos.
        if (delayBeforeClose > 0)
        {
            yield return new WaitForSeconds(delayBeforeClose);

            // Si no se ha vuelto a abrir, la cerramos
            if (isOpen)
            {
                // ** SIMULACIÓN DE CIERRE **
                if (doorCollider != null) doorCollider.enabled = true;

                if (anim != null)
                {
                    anim.SetTrigger("Close");
                }

                if (closeSound != null)
                {
                    audioSource.PlayOneShot(closeSound);
                }

                isOpen = false;
                Debug.Log("La puerta se cerró automáticamente.");
            }
        }
    }
}