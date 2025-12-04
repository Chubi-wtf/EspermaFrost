using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class EnemyController : MonoBehaviour
{
    #region Configuración

    [Header("Referencias")]
    public Transform player;
    private PlayerMovement playerScript;
    private Animator animator;

    [Header("Movimiento")]
    public float patrolSpeed = 3.5f;
    public float chaseSpeed = 5.0f;
    public float wanderRadius = 20f;
    public float wanderInterval = 5f;

    [Header("Sensores (Oído)")]
    [Tooltip("Multiplicador de audición. 1 = Normal.")]
    public float hearingSensitivity = 1.0f;

    [Header("Búsqueda Frenética")]
    [Tooltip("Radio alrededor de tu última posición donde el enemigo correrá a buscar.")]
    public float searchRadius = 10f;
    [Tooltip("Tiempo que se queda esperando en el punto de búsqueda antes de rendirse.")]
    public float searchDuration = 4f;

    [Header("Ataque")]
    public float proximityAttackRange = 1.5f;
    public float damageAmount = 25f;
    public float attackCooldown = 2f;
    private float lastAttackTime = -99f;
    [Tooltip("Duración de la animación de ataque en segundos")]
    public float attackAnimationDuration = 1f;
    private bool isAttacking = false;

    [Header("Daño por Colisión")]
    [Tooltip("Daño que hace el enemigo al chocar con el jugador")]
    public float collisionDamage = 10f;
    [Tooltip("Tiempo de espera entre daños por colisión (para evitar spam)")]
    public float collisionDamageCooldown = 1f;
    [Tooltip("Número de golpes antes de detenerse")]
    public int hitsBeforeStop = 2;
    [Tooltip("Tiempo que se queda quieto después de golpear")]
    public float stunDuration = 2f;
    [Tooltip("Fuerza del empuje al jugador")]
    public float pushForce = 10f;
    private float lastCollisionDamageTime = -99f;
    private int consecutiveHits = 0;
    private bool isStunned = false;

    [Header("Audio General")]
    public AudioSource fuenteAudioPrincipal; // Para rugidos y ataques
    public AudioClip sonidoRugido;
    public AudioClip sonidoAtaque;
    [Tooltip("Sonido de rugido después de golpear")]
    public AudioClip sonidoRugidoGolpe;

    [Header("Audio de Movimiento (NUEVO)")]
    [Tooltip("Fuente de audio separada para los pasos (para no cortar rugidos). Si la dejas vacía, se crea sola.")]
    public AudioSource fuenteAudioPasos;
    public AudioClip pasosCaminar;
    public AudioClip pasosCorrer;
    [Range(0f, 1f)] public float volumenPasos = 0.8f;

    #endregion

    #region Estado Interno
    public enum EnemyState
    {
        PATROL,
        CHASE,
        SEARCH,
        STUNNED
    }

    public EnemyState currentState;

    private NavMeshAgent agent;
    private float stateTimer;
    private Vector3 lastKnownPosition;
    private Vector3 searchTargetPosition;

    // Parámetros del Animator
    private readonly int speedHash = Animator.StringToHash("Speed");
    private readonly int attackHash = Animator.StringToHash("Attack");

    private float currentAnimSpeed = 0f;

    #endregion

    #region Métodos de Unity

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        currentState = EnemyState.PATROL;
        agent.speed = patrolSpeed;

        if (player != null)
            playerScript = player.GetComponent<PlayerMovement>();
        else if (PlayerMovement.Instance != null)
        {
            playerScript = PlayerMovement.Instance;
            player = PlayerMovement.Instance.transform;
        }

        // --- Configuración Automática de Audio de Pasos ---
        if (fuenteAudioPasos == null)
        {
            // Creamos un AudioSource hijo si no existe, para tener control independiente
            GameObject audioObj = new GameObject("AudioPasos_Auto");
            audioObj.transform.SetParent(this.transform);
            audioObj.transform.localPosition = Vector3.zero;
            fuenteAudioPasos = audioObj.AddComponent<AudioSource>();

            // Configuración básica 3D
            fuenteAudioPasos.spatialBlend = 1.0f; // 3D total
            fuenteAudioPasos.loop = true;
            fuenteAudioPasos.playOnAwake = false;
            fuenteAudioPasos.volume = volumenPasos;
            fuenteAudioPasos.minDistance = 2f;
            fuenteAudioPasos.maxDistance = 20f;
        }
        else
        {
            fuenteAudioPasos.loop = true; // Asegurar que loopee
        }

        // Iniciar en Idle
        UpdateAnimationSpeed(0f);
    }

    void Update()
    {
        // Manejo del Audio de Pasos constante
        HandleMovementAudio();

        if (playerScript == null || player == null) return;

        // Si está atacando o aturdido, no procesar movimiento
        if (isAttacking || isStunned) return;

        bool canHearNow = CheckIfCanHearPlayer();

        switch (currentState)
        {
            case EnemyState.PATROL:
                HandlePatrol(canHearNow);
                break;
            case EnemyState.CHASE:
                HandleChase(canHearNow);
                break;
            case EnemyState.SEARCH:
                HandleSearch(canHearNow);
                break;
            case EnemyState.STUNNED:
                break;
        }

        CheckProximityAttack();
    }

    #endregion

    #region Sistema de Audio de Movimiento (NUEVO)

    void HandleMovementAudio()
    {
        // Si está aturdido o atacando, silenciar pasos inmediatamente
        if (isStunned || isAttacking)
        {
            if (fuenteAudioPasos.isPlaying) fuenteAudioPasos.Stop();
            return;
        }

        // Verificar si se está moviendo realmente
        // Usamos velocity.sqrMagnitude para rendimiento (es más rápido que magnitude)
        bool isMoving = agent.velocity.sqrMagnitude > 0.1f;

        if (isMoving)
        {
            // Determinar si corre o camina
            // Si la velocidad actual es mayor que la velocidad de patrulla + un pequeño margen, consideramos que corre
            bool isRunning = agent.velocity.magnitude > (patrolSpeed + 0.2f);

            AudioClip clipCorrecto = isRunning ? pasosCorrer : pasosCaminar;

            // Si no hay clip asignado, no hacemos nada
            if (clipCorrecto == null) return;

            // Lógica de cambio de clip
            if (!fuenteAudioPasos.isPlaying)
            {
                // Si estaba en silencio, empezar a reproducir
                fuenteAudioPasos.clip = clipCorrecto;
                fuenteAudioPasos.pitch = isRunning ? 1.1f : 0.9f; // Pequeña variación de tono
                fuenteAudioPasos.Play();
            }
            else if (fuenteAudioPasos.clip != clipCorrecto)
            {
                // Si estaba sonando pero era el clip incorrecto (cambió de caminar a correr o viceversa)
                fuenteAudioPasos.clip = clipCorrecto;
                fuenteAudioPasos.pitch = isRunning ? 1.1f : 0.9f;
                fuenteAudioPasos.Play(); // Reiniciar con el nuevo clip
            }
        }
        else
        {
            // Si está quieto, detener sonido
            if (fuenteAudioPasos.isPlaying)
            {
                fuenteAudioPasos.Stop();
            }
        }
    }

    #endregion

    #region Sistema de Colisiones

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isStunned)
        {
            DealCollisionDamage(collision);
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isStunned)
        {
            DealCollisionDamage(collision);
        }
    }

    #endregion

    #region Sistema de Animación

    void UpdateAnimationSpeed(float speed)
    {
        if (Mathf.Abs(currentAnimSpeed - speed) > 0.01f)
        {
            currentAnimSpeed = speed;
            animator.SetFloat(speedHash, speed);
        }
    }

    #endregion

    #region Lógica de Sentidos
    bool CheckIfCanHearPlayer()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        float noiseRadius = playerScript.noiseCollider.radius;
        return distance <= (noiseRadius * hearingSensitivity);
    }
    #endregion

    #region Comportamientos (Estados)

    // --- PATRULLA ---
    void HandlePatrol(bool canHearNow)
    {
        if (canHearNow) { StartChasing(); return; }

        agent.speed = patrolSpeed;
        stateTimer += Time.deltaTime;

        float normalizedSpeed = agent.velocity.magnitude / chaseSpeed;
        UpdateAnimationSpeed(Mathf.Clamp(normalizedSpeed, 0f, 0.5f));

        if (stateTimer >= wanderInterval || agent.remainingDistance < 0.5f)
        {
            Vector3 newPos = RandomNavMeshPosition(transform.position, wanderRadius);
            agent.SetDestination(newPos);
            stateTimer = 0;
        }
    }

    // --- PERSECUCIÓN ---
    void HandleChase(bool canHearNow)
    {
        if (canHearNow)
        {
            agent.speed = chaseSpeed;
            lastKnownPosition = player.position;
            agent.SetDestination(lastKnownPosition);
            UpdateAnimationSpeed(1f);
        }
        else
        {
            currentState = EnemyState.SEARCH;
            searchTargetPosition = RandomNavMeshPosition(lastKnownPosition, searchRadius);
            agent.SetDestination(searchTargetPosition);
            agent.speed = chaseSpeed;
            stateTimer = 0;
            UpdateAnimationSpeed(1f);
        }
    }

    // --- BÚSQUEDA ---
    void HandleSearch(bool canHearNow)
    {
        if (canHearNow) { StartChasing(); return; }

        if (agent.remainingDistance > 1.0f)
        {
            agent.speed = chaseSpeed;
            UpdateAnimationSpeed(1f);
        }
        else
        {
            if (!agent.pathPending)
            {
                agent.speed = 0;
                UpdateAnimationSpeed(0f);

                stateTimer += Time.deltaTime;

                if (stateTimer >= searchDuration)
                {
                    currentState = EnemyState.PATROL;
                    stateTimer = 0;
                }
            }
        }
    }

    void StartChasing()
    {
        if (currentState != EnemyState.CHASE)
        {
            currentState = EnemyState.CHASE;
            agent.ResetPath();
            UpdateAnimationSpeed(1f);

            if (fuenteAudioPrincipal != null && sonidoRugido != null)
                fuenteAudioPrincipal.PlayOneShot(sonidoRugido);
        }
    }
    #endregion

    #region Sistema de Ataque

    void CheckProximityAttack()
    {
        if (Vector3.Distance(transform.position, player.position) < proximityAttackRange &&
            Time.time > lastAttackTime + attackCooldown &&
            !isAttacking)
        {
            StartCoroutine(PerformAttack());
        }
    }

    System.Collections.IEnumerator PerformAttack()
    {
        isAttacking = true;
        agent.isStopped = true;

        // Detener sonido de pasos al atacar
        if (fuenteAudioPasos.isPlaying) fuenteAudioPasos.Stop();

        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        UpdateAnimationSpeed(0f);
        animator.SetTrigger(attackHash);

        if (fuenteAudioPrincipal != null && sonidoAtaque != null)
            fuenteAudioPrincipal.PlayOneShot(sonidoAtaque);

        yield return new WaitForSeconds(attackAnimationDuration * 0.5f);

        if (Vector3.Distance(transform.position, player.position) < proximityAttackRange)
        {
            playerScript.TakeDamage(damageAmount, transform.position);
        }

        yield return new WaitForSeconds(attackAnimationDuration * 0.5f);

        lastAttackTime = Time.time;
        isAttacking = false;
        agent.isStopped = false;
    }

    #endregion

    #region Sistema de Daño por Colisión

    void DealCollisionDamage(Collision collision)
    {
        if (Time.time > lastCollisionDamageTime + collisionDamageCooldown)
        {
            if (playerScript != null)
            {
                playerScript.TakeDamage(collisionDamage, transform.position);
                PushPlayer();

                lastCollisionDamageTime = Time.time;
                consecutiveHits++;

                Debug.Log("¡Enemigo hizo daño por colisión! (-" + collisionDamage + " HP)");

                if (consecutiveHits >= hitsBeforeStop)
                {
                    StartCoroutine(StunEnemy());
                }
            }
        }
    }

    void PushPlayer()
    {
        if (playerScript == null) return;
        Vector3 pushDirection = (player.position - transform.position).normalized;
        pushDirection.y = 0;
        playerScript.ApplyCustomKnockback(pushDirection, pushForce);
    }

    IEnumerator StunEnemy()
    {
        isStunned = true;
        currentState = EnemyState.STUNNED;
        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        // Silenciar pasos
        if (fuenteAudioPasos.isPlaying) fuenteAudioPasos.Stop();

        UpdateAnimationSpeed(0f);

        if (fuenteAudioPrincipal != null && sonidoRugidoGolpe != null)
        {
            fuenteAudioPrincipal.PlayOneShot(sonidoRugidoGolpe);
        }

        yield return new WaitForSeconds(stunDuration);

        consecutiveHits = 0;
        isStunned = false;
        agent.isStopped = false;
        currentState = EnemyState.PATROL;
    }

    #endregion

    #region Utilidades
    Vector3 RandomNavMeshPosition(Vector3 origin, float dist)
    {
        Vector3 randDir = Random.insideUnitSphere * dist;
        randDir += origin;
        NavMeshHit navHit;
        if (NavMesh.SamplePosition(randDir, out navHit, dist, NavMesh.AllAreas))
        {
            return navHit.position;
        }
        return origin;
    }

    private void OnDrawGizmos()
    {
        if (currentState == EnemyState.CHASE && player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, player.position);
        }
        if (currentState == EnemyState.SEARCH)
        {
            Gizmos.color = new Color(1, 0.5f, 0, 0.5f);
            Gizmos.DrawWireSphere(lastKnownPosition, searchRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(searchTargetPosition, 0.5f);
        }
        if (currentState == EnemyState.STUNNED)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, 2f);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, proximityAttackRange);
    }
    #endregion
}