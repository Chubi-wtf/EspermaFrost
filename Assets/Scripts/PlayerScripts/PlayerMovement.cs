using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement Instance;

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

    [Header("CONFIGURACIÓN DE KNOCKBACK")]
    public float knockbackForce = 15f;
    public float knockbackDuration = 0.15f;

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

    [Header("CONFIGURACIÓN DE AGACHARSE (NUEVO)")]
    public float crouchHeight = 1f;        // Altura al estar agachado
    public float crouchSpeedMultiplier = 0.5f; // Velocidad al ir agachado
    public float crouchTransitionSpeed = 10f;  // Que tan rápido baja la cámara
    private float originalHeight;         // Altura original (se guarda sola)
    private Vector3 originalCenter;       // Centro original (se guarda solo)
    private Coroutine crouchCoroutine;    // Para manejar la animación suave

    // [MODIFICADO] Variable para recordar la posición de la cámara
    private Vector3 originalCamPos;

    [Header("VARIABLES DE RUIDO")]
    public SphereCollider noiseCollider;
    public float baseNoiseRadius, walkNoiseRadius, runNoiseRadius;

    [Header("DEATH COLLIDER")]
    public CapsuleCollider DeathCollision;

    #region PRIVATES BOOLS
    [Header("ESTADO DEL JUGADOR")]
    private Coroutine adrenalineCoroutine;
    public bool isPlayerCrouching = false;
    public bool isRunning = false;
    public bool isStaminaEmpty = false;
    private float timeSinceLastRun = 0f;
    private bool isAdrenalineActive = false;
    public bool canMove = true;

    private Transform cam;
    private float horizontalRotation, verticalRotation;

    private float keyboardX;
    private float keyboardY;
    private float currentSpeed;

    Rigidbody rb;
    CapsuleCollider cc;

    public Animator animator;
    #endregion

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

        // [MODIFICADO] Guardamos la posición original de la cámara (Ojos)
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

    void UpdateAnimations()
    {
        if (animator == null) return;

        // Calculamos la velocidad real según la versión de Unity
        float velocityMagnitude = 0f;
#if UNITY_6000_0_OR_NEWER
        velocityMagnitude = rb.linearVelocity.magnitude;
#else
        velocityMagnitude = rb.velocity.magnitude;
#endif

        // 1. Pasar la velocidad
        animator.SetFloat("Speed", velocityMagnitude);

        // 2. Pasar si está agachado
        animator.SetBool("IsCrouching", isPlayerCrouching);

        // 3. Pasar si está corriendo y moviéndose
        bool isMovingParams = (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0);
        animator.SetBool("IsRunning", isRunning && isMovingParams);

        // 4. Pasar si se está moviendo
        animator.SetBool("IsMoving", isMovingParams);
    }

    // --- SISTEMA DE STAMINA ---
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

    #region RADIO DE ESCUCHA (MODIFICADO)
    private void UpdateNoiseRadius(float currentSpeed)
    {
        if (noiseCollider == null) return;

        // 1. SI ESTÁ AGACHADO -> RUIDO CERO
        if (isPlayerCrouching)
        {
            noiseCollider.radius = 0f;
            return;
        }

        // 2. Si no se mueve -> Ruido Base
        if (Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0)
        {
            noiseCollider.radius = baseNoiseRadius;
            return;
        }

        // 3. Movimiento Normal
        if (currentSpeed == runSpeed)
            noiseCollider.radius = runNoiseRadius;
        else
            noiseCollider.radius = walkNoiseRadius;
    }
    #endregion

    #region MOVIMIENTO Y AGACHADO
    private void Movement()
    {
        if (!canMove) return;

        float targetSpeed = movementSpeed;
        isRunning = false;

        // --- LÓGICA DE VELOCIDAD ---
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

    // --- NUEVO SISTEMA DE AGACHADO SUAVE (CORREGIDO) ---
    private void ToggleCrouch()
    {
        isPlayerCrouching = !isPlayerCrouching;

        if (crouchCoroutine != null) StopCoroutine(crouchCoroutine);
        crouchCoroutine = StartCoroutine(SmoothCrouch());
    }

    private IEnumerator SmoothCrouch()
    {
        // 1. Definir objetivos del Cuerpo (Collider)
        float targetHeight = isPlayerCrouching ? crouchHeight : originalHeight;

        // CÁLCULO MATEMÁTICO: Ajustamos el centro para que los pies NO se muevan
        float heightDifference = originalHeight - targetHeight;
        Vector3 targetCenter = originalCenter - new Vector3(0, heightDifference / 2, 0);

        // 2. Definir objetivos de la Cámara (Ojos)
        // La cámara baja exactamente lo mismo que se encoge el personaje
        Vector3 targetCamPos = isPlayerCrouching ?
            originalCamPos - new Vector3(0, heightDifference, 0) :
            originalCamPos;

        // 3. Valores actuales para iniciar la transición
        float currentHeight = cc.height;
        Vector3 currentCenter = cc.center;
        Vector3 currentCamPos = cam.localPosition;

        float timeElapsed = 0;

        while (timeElapsed < 1)
        {
            // Lerp de Collider (Cuerpo)
            cc.height = Mathf.Lerp(currentHeight, targetHeight, timeElapsed);
            cc.center = Vector3.Lerp(currentCenter, targetCenter, timeElapsed);

            // Lerp de Cámara (Visión) - [ESTO BAJA LA CÁMARA]
            cam.localPosition = Vector3.Lerp(currentCamPos, targetCamPos, timeElapsed);

            timeElapsed += Time.deltaTime * crouchTransitionSpeed;
            yield return null;
        }

        // Aseguramos valores finales exactos
        cc.height = targetHeight;
        cc.center = targetCenter;
        cam.localPosition = targetCamPos;
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
    #endregion

    #region DAÑO, SALUD Y MÉTODOS PÚBLICOS
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Lógica de choque
        }
    }

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