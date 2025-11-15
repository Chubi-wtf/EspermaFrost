using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
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
    public float slowDuration = 2f;      // Duración en segundos del efecto
    public float slowMultiplier = 0.5f;    // Multiplicador de velocidad (0.5 = 50% de velocidad)
    private float originalMovementSpeed;    // Para guardar la velocidad base original
    private float originalRunSpeed;         // Para guardar la velocidad de correr original
    public bool isSlowed = false;          // Evita aplicar el slow varias veces

    [Header("VARIABLES DE RUIDO")]
    public SphereCollider noiseCollider;
    public float baseNoiseRadius, walkNoiseRadius, runNoiseRadius;

    [Header("DEATH COLLIDER")]
    public CapsuleCollider DeathCollision;

    #region PRIVATES BOOLS
    [Header("ESTADO DEL JUGADOR")]
    public bool isPlayerCrouching = false;
    public bool isRunning = false;         // Cambiado a public
    public bool isStaminaEmpty = false;    // Cambiado a public
    private float timeSinceLastRun = 0f;

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
        Cursor.lockState = CursorLockMode.Locked;
        cam = GetComponentInChildren<Camera>().transform;
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        cc = GetComponent<CapsuleCollider>();
        currentHealth = maxHealth;
        currentStamina = maxStamina; // <-- Inicialización de Estamina

        originalMovementSpeed = movementSpeed;
        originalRunSpeed = runSpeed;


        if (noiseCollider != null)
        {
            noiseCollider.radius = baseNoiseRadius;
            noiseCollider.isTrigger = true;
        }
    }

    private void Update()
    {
        Movement();

        // LÓGICA DE STAMINA AÑADIDA
        HandleStamina();

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            Crouch();
        }

        UpdateNoiseRadius(currentSpeed);
    }

    // NUEVO MÉTODO PARA MANEJAR EL CONSUMO Y REGENERACIÓN DE STAMINA
    private void HandleStamina()
    {
        if (isRunning)
        {
            // CONSUMO: Si está corriendo, drena estamina
            currentStamina -= staminaDrainRate * Time.deltaTime;
            currentStamina = Mathf.Max(currentStamina, 0f);

            timeSinceLastRun = 0f;

            if (currentStamina <= 0f)
            {
                isStaminaEmpty = true;
                isRunning = false;
                timeSinceLastRun = -runCooldown; // <-- NUEVO: Inicia el cooldown
                Debug.Log("¡Estamina agotada! Cooldown activado.");
            }
        }
        else if (currentStamina < maxStamina)
        {
            // REGENERACIÓN
            timeSinceLastRun += Time.deltaTime;

            // NUEVO: Solo regenera si no está en cooldown
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


    #region RADIO DE ESCUCHA

    private void UpdateNoiseRadius(float currentSpeed)
    {
        if (noiseCollider == null) return;

        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
        {
            if (currentSpeed == runSpeed)
            {
                noiseCollider.radius = runNoiseRadius;
                Debug.Log("Generando ruido de correr");
            }
            else
            {
                noiseCollider.radius = walkNoiseRadius;
                Debug.Log("Generando ruido de caminar");
            }
        }
        else
        {
            noiseCollider.radius = baseNoiseRadius;
        }
    }
    #endregion

    #region DAÑO Y COLLIDERS CON ENEMIGO

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Añadir sonidos y efecto de cámara
        }
    }

    // ** MODIFICACIÓN DEL MÉTODO TAKE DAMAGE **
    // Ahora acepta la posición del atacante (Enemy)
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
            ApplyKnockback(attackerPosition); // <-- Llamada al Knockback
            ApplySlowEffect();
        }
    }

    private void ApplyKnockback(Vector3 attackerPosition)
    {
        Vector3 knockbackDirection = transform.position - attackerPosition;
        knockbackDirection.y = 0;
        knockbackDirection.Normalize();

        StopCoroutine(KnockbackRoutine(knockbackDirection));
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
        // Detiene cualquier efecto de lentitud anterior para reiniciar la duración
        if (isSlowed)
        {
            StopCoroutine("SlowDown");
        }
        // Inicia la corrutina
        StartCoroutine("SlowDown");
    }

    IEnumerator SlowDown()
    {
        isSlowed = true;

        // Aplicar la reducción de velocidad
        movementSpeed = originalMovementSpeed * slowMultiplier;
        runSpeed = originalRunSpeed * slowMultiplier;

        // Esperar la duración especificada
        yield return new WaitForSeconds(slowDuration);

        // Restaurar la velocidad original
        movementSpeed = originalMovementSpeed;
        runSpeed = originalRunSpeed;
        isSlowed = false;
    }

    private void Die()
    {
        Debug.Log("¡El jugador ha muerto!");

        this.enabled = false;
        rb.linearVelocity = Vector3.zero;
    }
    #endregion

    #region MOVIMIENTO
    private void Movement()
    {
        // Movimiento base (caminar)
        currentSpeed = movementSpeed;
        isRunning = false; // Reset de la bandera de correr

        // Comprobación para correr
        // AÑADIDO: Verifica si se presiona Shift Y si el jugador NO está en slow Y si la estamina NO está vacía
        if (Input.GetKey(KeyCode.LeftShift) && !isSlowed && !isStaminaEmpty)
        {
            // Verificamos que el jugador se esté moviendo realmente para no gastar estamina quieto
            if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
            {
                currentSpeed = runSpeed;
                isRunning = true; // El jugador está corriendo
            }
        }

        // Si el jugador intenta correr pero ya tiene la estamina vacía, forzamos a caminar.
        if (isStaminaEmpty)
        {
            currentSpeed = movementSpeed;
            isRunning = false;
        }

        keyboardX = Input.GetAxis("Horizontal");
        keyboardY = Input.GetAxis("Vertical");

        float mouseX = Input.GetAxis("Mouse X") * Time.deltaTime * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * Time.deltaTime * mouseSensitivity;

        horizontalRotation += mouseX;
        verticalRotation -= mouseY;

        verticalRotation = Mathf.Clamp(verticalRotation, -90, 90);

        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, horizontalRotation, transform.localEulerAngles.z);
        cam.localEulerAngles = new Vector3(verticalRotation, cam.localEulerAngles.y, cam.localEulerAngles.z);
    }

    private void Crouch()
    {
        isPlayerCrouching = !isPlayerCrouching;

        if (isPlayerCrouching)
        {
            cc.height = 1f;
            cc.center = new Vector3(0f, -0.5f, 0f);
        }
        else
        {
            cc.height = 2f;
            cc.center = new Vector3(0f, 0f, 0f);
        }
    }

    private void FixedUpdate()
    {
        Vector3 moveInput = new Vector3(keyboardX, 0, keyboardY);

        if (moveInput.magnitude > 1)
        {
            moveInput.Normalize();
        }

        Vector3 targetMove = transform.TransformDirection(moveInput) * currentSpeed;

        rb.linearVelocity = new Vector3(targetMove.x, rb.linearVelocity.y, targetMove.z);
    }
    #endregion

    #region HEALTH & STAMINA
    public void Heal(float amount)
    {
        if (currentHealth < maxHealth)
        {
            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
            Debug.Log($"[Botiquín] Curado {amount} de vida. Salud actual: {currentHealth}");
        }
    }

    // Método para agregar estamina (ej: con un consumible, opcional)
    public void AddStamina(float amount)
    {
        if (currentStamina < maxStamina)
        {
            currentStamina = Mathf.Min(currentStamina + amount, maxStamina);
            isStaminaEmpty = false; // Asegura que pueda correr si agrega estamina
            Debug.Log($"[Consumible] Recargado {amount} de estamina. Estamina actual: {currentStamina}");
        }
    }
    #endregion
}