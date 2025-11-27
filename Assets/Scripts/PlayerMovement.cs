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
    public GameObject darkness;

    [Header("CONFIGURACIÓN DE KNOCKBACK")]
    public float knockbackForce = 15f;
    public float knockbackDuration = 0.15f;

    [Header("VARIABLES DE MOVIMIENTO")]
    public float mouseSensitivity;
    public float movementSpeed = 5f;
    public float runSpeed = 10f;

    [Header("VARIABLES EFECTO DE REDUCCIÓN DE VELOCIDAD")]
    public float slowDuration = 2f;      // Duración en segundos del efecto
    public float slowMultiplier = 0.5f;    // Multiplicador de velocidad
    private float originalMovementSpeed;    // Para guardar la velocidad base original
    private float originalRunSpeed;         // Para guardar la velocidad de correr original
    public bool isSlowed = false;          // Evita aplicar el slow varias veces

    [Header("VARIABLES DE RUIDO")]
    public SphereCollider noiseCollider;
    public float baseNoiseRadius, walkNoiseRadius, runNoiseRadius;

    [Header("Variables Sonido")]
    [SerializeField] public AudioClip damageSoundClip;
    [SerializeField] public AudioClip walkingSoundClip;
    [SerializeField] public AudioClip runningSoundClip;

    //Variables para frencuencia de sonidos de caminar:
    public float stepInterval = 9f;
    private float stepTimer = 0f;


    [SerializeField] public AudioSource playerAudioSource;




    [Header("DEATH COLLIDER")]
    public CapsuleCollider DeathCollision;

    #region PRIVATES BOOLS
    [Header("ESTADO DEL JUGADOR")]
    private Coroutine adrenalineCoroutine;
    public bool isPlayerCrouching = false;
    public bool isRunning = false;
    public bool isStaminaEmpty = false;
    public bool isWalking = false;
    private float timeSinceLastRun = 0f;
    private bool isAdrenalineActive = false;
    public bool canMove = true; // Control para bloquear movimiento y cámara

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
        // Asignación de la instancia Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Evita duplicados
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
        cam = GetComponentInChildren<Camera>().transform;
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        cc = GetComponent<CapsuleCollider>();
        currentHealth = maxHealth;
        currentStamina = maxStamina;

        originalMovementSpeed = movementSpeed;
        originalRunSpeed = runSpeed;

        if (noiseCollider != null)
        {
            noiseCollider.radius = baseNoiseRadius;
            noiseCollider.isTrigger = true;
        }

        playerAudioSource = GetComponent<AudioSource>();
        darkness.SetActive(false);
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

        #region isWalkingBool

        if (rb.linearVelocity != Vector3.zero && !isRunning)
        {
            if (!isWalking) // solo entra la primera vez
            {
                Debug.Log("Player started walking");
                isWalking = true;

                playerAudioSource.clip = walkingSoundClip;
                playerAudioSource.loop = true; // que se repita mientras camina
                playerAudioSource.Play();
            }
        }
        else
        {
            if (isWalking) // solo entra la primera vez que deja de caminar
            {
                Debug.Log("Player stopped walking");
                isWalking = false;

                playerAudioSource.Stop();
            }
        }

        #endregion
    }

    // --- SISTEMA DE STAMINA ---
    private void HandleStamina()
    {
        // Si la adrenalina está activa, la estamina no baja y se mantiene al máximo
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
                timeSinceLastRun = -runCooldown; // Inicia el cooldown negativo
                darkness.SetActive(true);
                Debug.Log("¡Estamina agotada! Cooldown activado.");
            }
        }
        else if (currentStamina < maxStamina)
        {
            // REGENERACIÓN
            timeSinceLastRun += Time.deltaTime;

            // Solo regenera si ha pasado el tiempo de delay
            if (timeSinceLastRun >= Mathf.Max(regenDelay, 0f))
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
                currentStamina = Mathf.Min(currentStamina, maxStamina);

                if (currentStamina > 0f && timeSinceLastRun >= runCooldown)
                {
                    isStaminaEmpty = false;
                    darkness.SetActive(false);
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
                noiseCollider.radius = runNoiseRadius;
            else
                noiseCollider.radius = walkNoiseRadius;
        }
        else
        {
            noiseCollider.radius = baseNoiseRadius;
        }
    }
    #endregion

    #region DAÑO Y COLLIDERS
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Lógica futura de choque con enemigo
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
    }
    #endregion

    #region MOVIMIENTO
    private void Movement()
    {
        if (!canMove) return;

        currentSpeed = movementSpeed;
        isRunning = false;

        // Correr: Shift + No Slow + Estamina disponible
        if (Input.GetKey(KeyCode.LeftShift) && !isSlowed && !isStaminaEmpty)
        {
            if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
            {
                currentSpeed = runSpeed;
                isRunning = true;
                isWalking = false;


                if (!playerAudioSource.isPlaying || playerAudioSource.clip != runningSoundClip)
                {
                    playerAudioSource.clip = runningSoundClip;
                    playerAudioSource.loop = true;   // que se repita mientras corre
                    playerAudioSource.Play();
                }
            }
        }
        else
        {
            // Cuando deja de correr, detener el sonido
            if (isRunning)
            {
                isRunning = false;
                playerAudioSource.Stop();
            }
        }

        // Si estamina vacía, forzar caminar
        if (isStaminaEmpty)
        {
            currentSpeed = movementSpeed;
            isRunning = false;
        }

        keyboardX = Input.GetAxis("Horizontal");
        keyboardY = Input.GetAxis("Vertical");

        // Cámara
        float mouseX = Input.GetAxis("Mouse X") * Time.deltaTime * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * Time.deltaTime * mouseSensitivity;

        horizontalRotation += mouseX;
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -60, 60);

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
        if (moveInput.magnitude > 1) moveInput.Normalize();

        Vector3 targetMove = transform.TransformDirection(moveInput) * currentSpeed;
        rb.linearVelocity = new Vector3(targetMove.x, rb.linearVelocity.y, targetMove.z);
    }
    #endregion

    #region HEALTH & STAMINA (PUBLIC METHODS)
    public float Heal(float amount)
    {
        float healthBeforeHeal = currentHealth;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);

        float actualHealedAmount = currentHealth - healthBeforeHeal;

        if (actualHealedAmount > 0)
        {
            Debug.Log($"[Botiquín] Curado {actualHealedAmount} de vida. Salud actual: {currentHealth}");
        }
        return actualHealedAmount;
    }

    public void AddStamina(float amount)
    {
        if (currentStamina < maxStamina)
        {
            currentStamina = Mathf.Min(currentStamina + amount, maxStamina);
            isStaminaEmpty = false;
            Debug.Log($"[Consumible] Recargado {amount} de estamina.");
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
        Debug.Log($"Adrenalina activada. Estamina infinita por {duration} segundos.");

        yield return new WaitForSeconds(duration);

        isAdrenalineActive = false;
        Debug.Log("Adrenalina agotada.");
    }
    #endregion
}