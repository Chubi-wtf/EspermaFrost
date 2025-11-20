using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [Header("Referencias")]
    private Animator animator;
    private PlayerMovement playerMovement;

    [Header("Parámetros de Animación")]
    // Asegúrate que estos nombres sean IDÉNTICOS a los de tu ventana Animator
    private readonly int isMovingHash = Animator.StringToHash("IsMoving");
    private readonly int isCrouchingHash = Animator.StringToHash("IsCrouching");
    private readonly int isRunningHash = Animator.StringToHash("IsRunning");
    private readonly int speedHash = Animator.StringToHash("Speed"); // Corregido a "Speed"

    // Variables para calcular velocidad manual
    private Vector3 lastPosition;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
        lastPosition = transform.position;

        if (animator == null) Debug.LogError("¡Falta el Animator en los hijos!");
    }

    private void Update()
    {
        // 1. Calcular velocidad basada en cambio de posición real
        // Ignoramos la altura (Y) para obtener solo velocidad horizontal
        Vector3 currentPosFlat = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 lastPosFlat = new Vector3(lastPosition.x, 0, lastPosition.z);

        float distanceMoved = Vector3.Distance(currentPosFlat, lastPosFlat);
        float currentSpeed = distanceMoved / Time.deltaTime;

        // Actualizamos la última posición para el siguiente frame
        lastPosition = transform.position;

        // 2. Actualizar Animator
        if (animator != null)
        {
            // "Speed" es mayor a 0.05f para evitar micro-movimientos
            bool isMoving = currentSpeed > 0.05f;

            animator.SetFloat(speedHash, currentSpeed);
            animator.SetBool(isMovingHash, isMoving);

            // Solo actualizamos Crouch y Run si tenemos la referencia al movimiento
            if (playerMovement != null)
            {
                animator.SetBool(isCrouchingHash, playerMovement.isPlayerCrouching);
                animator.SetBool(isRunningHash, playerMovement.isRunning);
            }
        }
    }
}