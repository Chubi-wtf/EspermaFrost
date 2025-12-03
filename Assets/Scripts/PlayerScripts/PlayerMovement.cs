using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement Instance;

    #region Variables de Vida y Estado

    [Header("VARIABLES DE VIDA")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("VARIABLES DE STAMINA")]
    public float maxStamina = 100f;
    public float currentStamina;
    public float staminaDrainRate = 15f;
    public float staminaRegenRate = 10f;
    public float regenDelay = 1.5f;
    public float runCooldown = 2f;

    [Header("ESTADO DEL JUGADOR")]
    private Coroutine adrenalineCoroutine;
    public bool isPlayerCrouching = false;
    public bool isRunning = false;
    public bool isStaminaEmpty = false;
    private float timeSinceLastRun = 0f;
    private bool isAdrenalineActive = false;
    public bool canMove = true;

    #endregion

    #region Configuración de Knockback y Empuje

    [Header("CONFIGURACIÓN DE KNOCKBACK")]
    public float knockbackForce = 15f;
    public float knockbackDuration = 0.15f;
    [Tooltip("Duración del empuje cuando el enemigo te golpea por colisión")]
    public float pushDuration = 0.3f;

    #endregion

    #region Variables de Movimiento

    [Header("VARIABLES DE MOVIMIENTO")]
    public float mouseSensitivity;
    public float movementSpeed = 5f;
    public float runSpeed = 10f;

    [Header("VARIABLES EFECTO DE REDUCCIÓN DE VELOCIDAD")]
    public float slowDuration = 2f;
    public float slowMultiplier = 0.5f;
    private float originalMovementSpeed;
    private float originalRunSpeed;
    public bool isSlowed = false;

    #endregion

    #region Configuración de Agacharse

    [Header("CONFIGURACIÓN DE AGACHARSE")]
    public float crouchHeight = 1f;
    public float crouchSpeedMultiplier = 0.5f;
    public float crouchTransitionSpeed = 10f;
    private float originalHeight;
    private Vector3 originalCenter;
    private Coroutine crouchCoroutine;
    private Vector3 originalCamPos;

    #endregion

    #region Variables de Ruido

    [Header("VARIABLES DE RUIDO")]
    public SphereCollider noiseCollider;
    public float baseNoiseRadius, walkNoiseRadius, runNoiseRadius;

    #endregion

    #region Referencias y Componentes

    [Header("DEATH COLLIDER")]
    public CapsuleCollider DeathCollision;

    private Transform cam;
    private float horizontalRotation, verticalRotation;
    private float keyboardX;
    private float keyboardY;
    private float currentSpeed;
    Rigidbody rb;
    CapsuleCollider cc;
    public Animator animator;

    #endregion

    #region Métodos de Unity

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }

        Cursor.lockState = CursorLockMode.Locked;
        cam = GetComponentInChildren<Camera>().transform;
        rb = GetComponent<Rigidbody>();

        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        // Configuración de Collider y Agachado
        cc = GetComponent<CapsuleCollider>();
        originalHeight = cc.height;
        originalCenter = cc.center;
        originalCamPos = cam.localPosition;

        // Configuración de Stats
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        originalMovementSpeed = movementSpeed;
        originalRunSpeed = runSpeed;

        // Configuración de Ruido (Seguridad)
        if (noiseCollider != null)
        {
            if (baseNoiseRadius == 0) baseNoiseRadius = 5f;
            if (walkNoiseRadius == 0) walkNoiseRadius = 15f;
            if (runNoiseRadius == 0) runNoiseRadius = 30f;

            noiseCollider.radius = baseNoiseRadius;
            noiseCollider.isTrigger = true;
        }
    }

    private void Update()
    {
        Movement();
        HandleStamina();

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            ToggleCrouch();
        }

        UpdateNoiseRadius(currentSpeed);
        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        Vector3 moveInput = new Vector3(keyboardX, 0, keyboardY);
        if (moveInput.magnitude > 1) moveInput.Normalize();

        Vector3 targetMove = transform.TransformDirection(moveInput) * currentSpeed;

#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = new Vector3(targetMove.x, rb.linearVelocity.y, targetMove.z);
#else
        rb.velocity = new Vector3(targetMove.x, rb.velocity.y, targetMove.z);
#endif
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Lógica de choque si es necesaria
        }
    }

    #endregion

    #region Sistema de Animación

    void UpdateAnimations()
    {
        if (animator == null) return;

        float velocityMagnitude = 0f;
#if UNITY_6000_0_OR_NEWER
        velocityMagnitude = rb.linearVelocity.magnitude;
#else
        velocityMagnitude = rb.velocity.magnitude;
#endif

        animator.SetFloat("Speed", velocityMagnitude);
        animator.SetBool("IsCrouching", isPlayerCrouching);

        bool isMovingParams = (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0);
        animator.SetBool("IsRunning", isRunning && isMovingParams);
        animator.SetBool("IsMoving", isMovingParams);
    }

    #endregion

    #region Sistema de Stamina

    private void HandleStamina()
    {
        if (isAdrenalineActive)
        {
            currentStamina = maxStamina;
            isStaminaEmpty = false;
            return;
        }

        if (isRunning)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;
            currentStamina = Mathf.Max(currentStamina, 0f);
            timeSinceLastRun = 0f;

            if (currentStamina <= 0f)
            {
                isStaminaEmpty = true;
                isRunning = false;
                timeSinceLastRun = -runCooldown;
                Debug.Log("¡Estamina agotada!");
            }
        }
        else if (currentStamina < maxStamina)
        {
            timeSinceLastRun += Time.deltaTime;
            if (timeSinceLastRun >= Mathf.Max(regenDelay, 0f))
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
                currentStamina = Mathf.Min(currentStamina, maxStamina);
                if (currentStamina > 0f && timeSinceLastRun >= runCooldown)
                {
                    isStaminaEmpty = false;
                }
            }
        }
    }

    #endregion

    #region Sistema de Ruido

    private void UpdateNoiseRadius(float currentSpeed)
    {
        if (noiseCollider == null) return;

        if (isPlayerCrouching)
        {
            noiseCollider.radius = 0f;
            return;
        }

        if (Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0)
        {
            noiseCollider.radius = baseNoiseRadius;
            return;
        }

        if (currentSpeed == runSpeed)
            noiseCollider.radius = runNoiseRadius;
        else
            noiseCollider.radius = walkNoiseRadius;
    }

    #endregion

    #region Sistema de Movimiento

    private void Movement()
    {
        if (!canMove) return;

        float targetSpeed = movementSpeed;
        isRunning = false;

        if (isPlayerCrouching)
        {
            targetSpeed *= crouchSpeedMultiplier;
        }
        else if (Input.GetKey(KeyCode.LeftShift) && !isSlowed && !isStaminaEmpty)
        {
            if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
            {
                targetSpeed = runSpeed;
                isRunning = true;
            }
        }

        if (isStaminaEmpty && isRunning)
        {
            targetSpeed = movementSpeed;
            isRunning = false;
        }

        currentSpeed = targetSpeed;

        keyboardX = Input.GetAxis("Horizontal");
        keyboardY = Input.GetAxis("Vertical");

        float mouseX = Input.GetAxis("Mouse X") * Time.deltaTime * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * Time.deltaTime * mouseSensitivity;

        horizontalRotation += mouseX;
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -60, 60);

        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, horizontalRotation, transform.localEulerAngles.z);
        cam.localEulerAngles = new Vector3(verticalRotation, cam.localEulerAngles.y, cam.localEulerAngles.z);
    }

    #endregion

    #region Sistema de Agachado

    private void ToggleCrouch()
    {
        isPlayerCrouching = !isPlayerCrouching;

        if (crouchCoroutine != null) StopCoroutine(crouchCoroutine);
        crouchCoroutine = StartCoroutine(SmoothCrouch());
    }

    private IEnumerator SmoothCrouch()
    {
        float targetHeight = isPlayerCrouching ? crouchHeight : originalHeight;
        float heightDifference = originalHeight - targetHeight;
        Vector3 targetCenter = originalCenter - new Vector3(0, heightDifference / 2, 0);
        Vector3 targetCamPos = isPlayerCrouching ?
            originalCamPos - new Vector3(0, heightDifference, 0) :
            originalCamPos;

        float currentHeight = cc.height;
        Vector3 currentCenter = cc.center;
        Vector3 currentCamPos = cam.localPosition;

        float timeElapsed = 0;

        while (timeElapsed < 1)
        {
            cc.height = Mathf.Lerp(currentHeight, targetHeight, timeElapsed);
            cc.center = Vector3.Lerp(currentCenter, targetCenter, timeElapsed);
            cam.localPosition = Vector3.Lerp(currentCamPos, targetCamPos, timeElapsed);

            timeElapsed += Time.deltaTime * crouchTransitionSpeed;
            yield return null;
        }

        cc.height = targetHeight;
        cc.center = targetCenter;
        cam.localPosition = targetCamPos;
    }

    #endregion

    #region Sistema de Daño y Knockback

    public void TakeDamage(float damageAmount, Vector3 attackerPosition)
    {
        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(currentHealth, 0f);

        Debug.Log("Vida actual: " + currentHealth);

        if (currentHealth <= 0) Die();
        else
        {
            ApplyKnockback(attackerPosition);
            ApplySlowEffect();
        }
    }

    private void ApplyKnockback(Vector3 attackerPosition)
    {
        Vector3 knockbackDirection = transform.position - attackerPosition;
        knockbackDirection.y = 0;
        knockbackDirection.Normalize();
        StopCoroutine("KnockbackRoutine");
        StartCoroutine(KnockbackRoutine(knockbackDirection));
    }

    private IEnumerator KnockbackRoutine(Vector3 direction)
    {
        float startTime = Time.time;
        Vector3 continuousForce = direction * knockbackForce;
        while (Time.time < startTime + knockbackDuration)
        {
            rb.AddForce(continuousForce, ForceMode.Force);
            yield return null;
        }
    }

    /// <summary>
    /// Empuje personalizado para cuando el enemigo golpea por colisión
    /// </summary>
    public void ApplyCustomKnockback(Vector3 direction, float force)
    {
        StopCoroutine("CustomPushRoutine");
        StartCoroutine(CustomPushRoutine(direction, force));
    }

    private IEnumerator CustomPushRoutine(Vector3 direction, float force)
    {
        float startTime = Time.time;
        Vector3 pushForce = direction * force;

        while (Time.time < startTime + pushDuration)
        {
            rb.AddForce(pushForce, ForceMode.Force);
            yield return null;
        }
    }

    #endregion

    #region Sistema de Efectos

    public void ApplySlowEffect()
    {
        if (isSlowed) StopCoroutine("SlowDown");
        StartCoroutine("SlowDown");
    }

    IEnumerator SlowDown()
    {
        isSlowed = true;
        movementSpeed = originalMovementSpeed * slowMultiplier;
        runSpeed = originalRunSpeed * slowMultiplier;
        yield return new WaitForSeconds(slowDuration);
        movementSpeed = originalMovementSpeed;
        runSpeed = originalRunSpeed;
        isSlowed = false;
    }

    #endregion

    #region Métodos Públicos de Utilidad

    public float Heal(float amount)
    {
        float startHealth = currentHealth;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        return currentHealth - startHealth;
    }

    public void AddStamina(float amount)
    {
        if (currentStamina < maxStamina)
        {
            currentStamina = Mathf.Min(currentStamina + amount, maxStamina);
            isStaminaEmpty = false;
        }
    }

    public void ActivateAdrenaline(float duration)
    {
        if (adrenalineCoroutine != null) StopCoroutine(adrenalineCoroutine);
        adrenalineCoroutine = StartCoroutine(AdrenalineRoutine(duration));
    }

    private IEnumerator AdrenalineRoutine(float duration)
    {
        isAdrenalineActive = true;
        isStaminaEmpty = false;
        yield return new WaitForSeconds(duration);
        isAdrenalineActive = false;
    }

    #endregion

    #region Muerte

    private void Die()
    {
        Debug.Log("¡El jugador ha muerto!");
        this.enabled = false;
#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = Vector3.zero;
#else
        rb.velocity = Vector3.zero;
#endif
    }

    #endregion
}