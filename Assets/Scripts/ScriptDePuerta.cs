using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class DoorController : MonoBehaviour
{
    [Header("Componentes")]
    public Animator anim;
    public AudioClip openSound;

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
    }
}