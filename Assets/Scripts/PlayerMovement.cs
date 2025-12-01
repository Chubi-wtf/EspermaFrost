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
    public float crouchHeight = 1f;       // Altura al estar agachado
    public float crouchSpeedMultiplier = 0.5f; // Velocidad al ir agachado
    public float crouchTransitionSpeed = 10f;  // Que tan rápido baja la cámara
    private float originalHeight;         // Altura original (se guarda sola)
    private Vector3 originalCenter;       // Centro original (se guarda solo)
    private Coroutine crouchCoroutine;    // Para manejar la animación suave

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
    #endregion

    Rigidbody rb;
    CapsuleCollider cc;

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }

        Cursor.lockState = CursorLockMode.Locked;
        cam = GetComponentInChildren<Camera>().transform;
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        // Configuración de Collider y Agachado
        cc = GetComponent<CapsuleCollider>();
        originalHeight = cc.height;
        originalCenter = cc.center;

        // Configuración de Stats
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        originalMovementSpeed = movementSpeed;
        originalRunSpeed = runSpeed;

        // Configuración de Ruido (Seguridad)
        if (noiseCollider != null)
        {
            // Asignamos valores por defecto si están en 0 para evitar errores
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

        // 1. SI ESTÁ AGACHADO -> RUIDO CERO (Prioridad Máxima)
        if (isPlayerCrouching)
        {
            noiseCollider.radius = 0f;
            return; // Salimos de la función aquí para que nada más lo modifique
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

        // Velocidad base
        float targetSpeed = movementSpeed;
        isRunning = false;

        // --- LÓGICA DE VELOCIDAD ---

        // 1. Si está agachado, se mueve lento
        if (isPlayerCrouching)
        {
            targetSpeed *= crouchSpeedMultiplier;
        }
        // 2. Si corre (Solo si NO está agachado, NO tiene slow y tiene stamina)
        else if (Input.GetKey(KeyCode.LeftShift) && !isSlowed && !isStaminaEmpty)
        {
            if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
            {
                targetSpeed = runSpeed;
                isRunning = true;
            }
        }

        // 3. Si se le acabó la estamina, forzamos caminar
        if (isStaminaEmpty && isRunning)
        {
            targetSpeed = movementSpeed;
            isRunning = false;
        }

        currentSpeed = targetSpeed;

        // Inputs y Cámara
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

    // --- NUEVO SISTEMA DE AGACHADO SUAVE ---
    private void ToggleCrouch()
    {
        isPlayerCrouching = !isPlayerCrouching;

        // Si ya hay una transición ocurriendo, la paramos para empezar la nueva
        if (crouchCoroutine != null) StopCoroutine(crouchCoroutine);
        crouchCoroutine = StartCoroutine(SmoothCrouch());
    }

    private IEnumerator SmoothCrouch()
    {
        float targetHeight = isPlayerCrouching ? crouchHeight : originalHeight;
        // Calculamos el centro para que los pies no se hundan en el suelo
        Vector3 targetCenter = isPlayerCrouching ? new Vector3(0, -0.5f, 0) : originalCenter;

        float currentHeight = cc.height;
        Vector3 currentCenter = cc.center;

        float timeElapsed = 0;

        while (timeElapsed < 1)
        {
            // Lerp para suavizar la transición
            cc.height = Mathf.Lerp(currentHeight, targetHeight, timeElapsed);
            cc.center = Vector3.Lerp(currentCenter, targetCenter, timeElapsed);

            timeElapsed += Time.deltaTime * crouchTransitionSpeed;
            yield return null;
        }

        // Aseguramos valores finales exactos
        cc.height = targetHeight;
        cc.center = targetCenter;
    }

    private void FixedUpdate()
    {
        Vector3 moveInput = new Vector3(keyboardX, 0, keyboardY);
        if (moveInput.magnitude > 1) moveInput.Normalize();

        Vector3 targetMove = transform.TransformDirection(moveInput) * currentSpeed;

        // Compatibilidad Unity 6 / Versiones viejas
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