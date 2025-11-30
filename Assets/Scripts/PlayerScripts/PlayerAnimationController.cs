using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [Header("Referencias")]
    private Animator animator;
    private PlayerMovement playerMovement;

    [Header("Par�metros de Animaci�n")]
    private readonly int isMovingHash = Animator.StringToHash("IsMoving");
    private readonly int isCrouchingHash = Animator.StringToHash("IsCrouching");
    private readonly int isRunningHash = Animator.StringToHash("IsRunning");
    private readonly int moveSpeedHash = Animator.StringToHash("moveSpeed");

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        playerMovement = GetComponent<PlayerMovement>();

        if (animator == null)
        {
            Debug.LogError("PlayerAnimationController: No se encontr� el Animator");
        }
    }


        private void Update()
    {
        UpdateAnimationParameters();

        // DEBUG TEMPORAL
        if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log($"Crouching: {animator.GetBool("IsCrouching")}, " +
                      $"Moving: {animator.GetBool("IsMoving")}, " +
                      $"Running: {animator.GetBool("IsRunning")}");
        }
    }


    private void UpdateAnimationParameters()
    {
        if (animator == null || playerMovement == null) return;

        // Calcular velocidad de movimiento
        Vector3 horizontalVelocity = new Vector3(
            playerMovement.GetComponent<Rigidbody>().linearVelocity.x,
            0f,
            playerMovement.GetComponent<Rigidbody>().linearVelocity.z
        );
        float currentSpeed = horizontalVelocity.magnitude;

        // Actualizar par�metros del Animator
        //animator.SetBool(isMovingHash, currentSpeed > 0.1f);
        //animator.SetBool(isCrouchingHash, playerMovement.isPlayerCrouching);
        //animator.SetBool(isRunningHash, playerMovement.isRunning && !playerMovement.isPlayerCrouching);    // Cerrado Temporal
        //animator.SetFloat(moveSpeedHash, currentSpeed);

        // Debug opcional
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log($"Moving: {currentSpeed > 0.1f}, Crouching: {playerMovement.isPlayerCrouching}, Running: {playerMovement.isRunning}");
        }
    }

    // M�todo para forzar una animaci�n (�til para transiciones especiales)
    public void SetCrouchingState(bool crouching)
    {
        if (animator != null)
        {
            animator.SetBool(isCrouchingHash, crouching);
        }
    }
}