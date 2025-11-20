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
    public bool canMove = true; // Control para bloquear movimiento (Script 2)
    public float mouseSensitivity = 100f;
    public float movementSpeed = 5f;
    public float runSpeed = 10f;

    [Header("VARIABLES EFECTO DE REDUCCIÓN DE VELOCIDAD")]
    public float slowDuration = 2f;
    public float slowMultiplier = 0.5f;
    private float originalMovementSpeed;
    private float originalRunSpeed;
    public bool isSlowed = false;

    [Header("VARIABLES DE RUIDO")]
    public SphereCollider noiseCollider;
    public float baseNoiseRadius = 5f;
    public float walkNoiseRadius = 15f;
    public float runNoiseRadius = 30f;
    public float crouchNoiseRadius = 2f;

    [Header("DEATH COLLIDER")]
    public CapsuleCollider DeathCollision;

    [Header("Configuración de Agacharse (Suave)")]
    public float crouchHeight = 1f;
    public float crouchSpeedMultiplier = 0.5f;
    public float crouchTransitionSpeed = 5f;
    private float standingHeight;
    private Vector3 standingCenter;
    private bool isTransitioningCrouch = false;

    #region PRIVATES BOOLS & STATE
    [Header("ESTADO DEL JUGADOR")]
    public bool isPlayerCrouching = false;
    public bool isRunning = false;
    public bool isStaminaEmpty = false;

    private float timeSinceLastRun = 0f;
    private bool isAdrenalineActive = false; // (Script 2)
    private Coroutine adrenalineCoroutine;   // (Script 2)

    private Transform cam;
    private float horizontalRotation, verticalRotation;
    private float keyboardX, keyboardY;
    private float currentSpeed;
    #endregion

    Rigidbody rb;
    CapsuleCollider cc;

    private void Awake()
    {
        // Singleton Pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        cc = GetComponent<CapsuleCollider>();

        // --- BÚSQUEDA SEGURA DE CÁMARA ---
        Camera foundCam = GetComponentInChildren<Camera>();
        if (foundCam != null) cam = foundCam.transform;
        else if (Camera.main != null) cam = Camera.main.transform;
        else Debug.LogError("¡NO SE ENCONTRÓ NINGUNA CÁMARA!");

        // Inicialización de valores
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        originalMovementSpeed = movementSpeed;
        originalRunSpeed = runSpeed;

        standingHeight = cc.height;
        standingCenter = cc.center;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // --- CORRECCIÓN DEL RUIDO (Script 1 - Muy útil) ---
        SetupNoiseCollider();
    }

    private void Update()
    {
        Movement();
        HandleStamina();

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            Crouch();
        }

        UpdateNoiseRadius(currentSpeed);
    }

    private void FixedUpdate()
    {
        if (rb == null) return;

        Vector3 moveInput = new Vector3(keyboardX, 0, keyboardY);

        if (moveInput.magnitude > 1)
        {
            moveInput.Normalize();
        }

        Vector3 targetMove = transform.TransformDirection(moveInput) * currentSpeed;

        // NOTA: rb.linearVelocity es para Unity 6. 
        // Si usas versiones anteriores (2022 o menos), cambia por: rb.velocity
        rb.linearVelocity = new Vector3(targetMove.x, rb.linearVelocity.y, targetMove.z);
    }

    #region MOVIMIENTO Y CÁMARA
    private void Movement()
    {
        if (!canMove) return; // Bloqueo de movimiento (Script 2)

        // 1. Definir velocidad base
        float targetSpeed = movementSpeed;
        isRunning = false;

        // 2. Modificadores de velocidad
        if (isPlayerCrouching)
        {
            targetSpeed *= crouchSpeedMultiplier;
        }
        // Correr: Shift + No Slow + Estamina OK (o Adrenalina)
        else if (Input.GetKey(KeyCode.LeftShift) && !isSlowed && (!isStaminaEmpty || isAdrenalineActive))
        {
            if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
            {
                targetSpeed = runSpeed;
                isRunning = true;
            }
        }

        // Si se acabó la estamina y no hay adrenalina, forzar caminar
        if (isStaminaEmpty && !isAdrenalineActive)
        {
            targetSpeed = movementSpeed;
            isRunning = false;
        }

        currentSpeed = targetSpeed;
        keyboardX = Input.GetAxis("Horizontal");
        keyboardY = Input.GetAxis("Vertical");

        // Rotación de Cámara
        float mouseX = Input.GetAxis("Mouse X") * Time.deltaTime * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * Time.deltaTime * mouseSensitivity;

        horizontalRotation += mouseX;
        verticalRotation -= mouseY;

        // Clamp -90 a 90 es estándar para FPS (Script 1). Script 2 tenía 60.
        verticalRotation = Mathf.Clamp(verticalRotation, -90, 90);

        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, horizontalRotation, transform.localEulerAngles.z);

        if (cam != null)
        {
            cam.localEulerAngles = new Vector3(verticalRotation, cam.localEulerAngles.y, cam.localEulerAngles.z);
        }
    }

    // Usamos la versión suave (Script 1) porque se siente mejor que el "snap" instantáneo
    private void Crouch()
    {
        if (isTransitioningCrouch) return;

        isPlayerCrouching = !isPlayerCrouching;
        isTransitioningCrouch = true;

        StartCoroutine(TransitionCrouch());
    }

    private IEnumerator TransitionCrouch()
    {
        float targetHeight = isPlayerCrouching ? crouchHeight : standingHeight;
        Vector3 targetCenter = isPlayerCrouching ? new Vector3(0f, -0.5f, 0f) : standingCenter;

        float currentHeight = cc.height;
        Vector3 currentCenter = cc.center;

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * crouchTransitionSpeed;
            cc.height = Mathf.Lerp(currentHeight, targetHeight, t);
            cc.center = Vector3.Lerp(currentCenter, targetCenter, t);
            yield return null;
        }

        // Asegurar valores finales
        cc.height = targetHeight;
        cc.center = targetCenter;

        isTransitioningCrouch = false;
    }
    #endregion

    #region STAMINA & ADRENALINA
    private void HandleStamina()
    {
        // Lógica de Adrenalina (Script 2)
        if (isAdrenalineActive)
        {
            currentStamina = maxStamina;
            isStaminaEmpty = false;
            return;
        }

        if (isRunning)
        {
            // CONSUMO
            currentStamina -= staminaDrainRate * Time.deltaTime;
            currentStamina = Mathf.Max(currentStamina, 0f);
            timeSinceLastRun = 0f;

            if (currentStamina <= 0f)
            {
                isStaminaEmpty = true;
                isRunning = false;
                timeSinceLastRun = -runCooldown; // Inicia el cooldown
                // Debug.Log("¡Estamina agotada!");
            }
        }
        else if (currentStamina < maxStamina)
        {
            // REGENERACIÓN
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

    public void ActivateAdrenaline(float duration)
    {
        if (adrenalineCoroutine != null)
        {
            StopCoroutine(adrenalineCoroutine);
        }
        adrenalineCoroutine = StartCoroutine(AdrenalineRoutine(duration));
    }

    private IEnumerator AdrenalineRoutine(float duration)
    {
        isAdrenalineActive = true;
        isStaminaEmpty = false; // Reactiva inmediatamente si estaba cansado
        Debug.Log($"[Adrenalina] Activada por {duration} segundos.");

        yield return new WaitForSeconds(duration);

        isAdrenalineActive = false;
        Debug.Log("[Adrenalina] Efecto terminado.");
    }
    #endregion

    #region SALUD & DAÑO
    // Modificado para devolver float (Script 2) pero mantener logs detallados (Script 1)
    public float Heal(float amount)
    {
        float healthBeforeHeal = currentHealth;
        if (currentHealth < maxHealth)
        {
            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
            float healed = currentHealth - healthBeforeHeal;
            Debug.Log($"[Botiquín] Curado {healed}. Salud: {currentHealth}");
            return healed;
        }
        return 0f;
    }

    public void AddStamina(float amount)
    {
        if (currentStamina < maxStamina)
        {
            currentStamina = Mathf.Min(currentStamina + amount, maxStamina);
            isStaminaEmpty = false;
            Debug.Log($"[Consumible] Recargado {amount} estamina.");
        }
    }

    public void TakeDamage(float damageAmount, Vector3 attackerPosition)
    {
        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(currentHealth, 0f);

        Debug.Log("Vida actual: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
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

    private void Die()
    {
        Debug.Log("¡El jugador ha muerto!");
        this.enabled = false;
        rb.linearVelocity = Vector3.zero;
        // Aquí puedes disparar eventos de UI de Game Over
    }
    #endregion

    #region SISTEMA DE RUIDO
    private void SetupNoiseCollider()
    {
        if (noiseCollider == null)
        {
            SphereCollider[] colliders = GetComponents<SphereCollider>();
            foreach (var col in colliders)
            {
                if (col.isTrigger)
                {
                    noiseCollider = col;
                    break;
                }
            }
            if (noiseCollider == null) Debug.LogWarning("Falta NoiseCollider en el Player.");
        }

        if (noiseCollider != null)
        {
            noiseCollider.isTrigger = true;
            noiseCollider.radius = baseNoiseRadius;
        }
    }

    private void UpdateNoiseRadius(float currentSpeed)
    {
        if (noiseCollider == null) return;

        // Si no hay input de movimiento
        if (Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0)
        {
            noiseCollider.radius = baseNoiseRadius;
            return;
        }

        // Si se mueve
        if (isPlayerCrouching)
        {
            noiseCollider.radius = crouchNoiseRadius;
        }
        else if (currentSpeed == runSpeed)
        {
            noiseCollider.radius = runNoiseRadius;
        }
        else
        {
            noiseCollider.radius = walkNoiseRadius;
        }
    }
    #endregion

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Lógica de impacto físico con enemigo (opcional)
        }
    }
}