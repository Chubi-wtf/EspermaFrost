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

    [Header("Audio (Opcional)")]
    public AudioSource fuenteAudioPrincipal;
    public AudioClip sonidoRugido;
    public AudioClip sonidoAtaque;
    [Tooltip("Sonido de rugido después de golpear")]
    public AudioClip sonidoRugidoGolpe;

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

    // Parámetros del Animator (usando Float para Speed y Trigger para Ataque)
    private readonly int speedHash = Animator.StringToHash("Speed");
    private readonly int attackHash = Animator.StringToHash("Attack");

    // Control de animación anterior para evitar llamadas innecesarias
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

        // Iniciar en Idle (Speed = 0)
        UpdateAnimationSpeed(0f);
    }

    void Update()
    {
        if (playerScript == null || player == null) return;

        // Si está atacando o aturdido, no procesar otros estados
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
                // No hacer nada, esperar a que termine el stun
                break;
        }

        CheckProximityAttack();
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
        // Solo actualizar si cambió significativamente
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

        // Speed basado en velocidad real del agente
        // 0 = Idle, 0.5 = Walking, 1 = Run
        float normalizedSpeed = agent.velocity.magnitude / chaseSpeed;
        UpdateAnimationSpeed(Mathf.Clamp(normalizedSpeed, 0f, 0.5f));

        if (stateTimer >= wanderInterval || agent.remainingDistance < 0.5f)
        {
            Vector3 newPos = RandomNavMeshPosition(transform.position, wanderRadius);
            agent.SetDestination(newPos);
            stateTimer = 0;
        }
    }

    // --- PERSECUCIÓN (Sabe dónde estás) ---
    void HandleChase(bool canHearNow)
    {
        if (canHearNow)
        {
            agent.speed = chaseSpeed;
            lastKnownPosition = player.position;
            agent.SetDestination(lastKnownPosition);

            // Speed = 1 (Run)
            UpdateAnimationSpeed(1f);
        }
        else
        {
            currentState = EnemyState.SEARCH;
            searchTargetPosition = RandomNavMeshPosition(lastKnownPosition, searchRadius);
            agent.SetDestination(searchTargetPosition);
            agent.speed = chaseSpeed;
            stateTimer = 0;

            // Mantener Speed = 1 (Run)
            UpdateAnimationSpeed(1f);
        }
    }

    // --- BÚSQUEDA (Corre a ver si estás por ahí) ---
    void HandleSearch(bool canHearNow)
    {
        if (canHearNow) { StartChasing(); return; }

        if (agent.remainingDistance > 1.0f)
        {
            agent.speed = chaseSpeed;
            UpdateAnimationSpeed(1f); // Run
        }
        else
        {
            if (!agent.pathPending)
            {
                agent.speed = 0;
                UpdateAnimationSpeed(0f); // Idle

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
            UpdateAnimationSpeed(1f); // Run

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

        // Mirar hacia el jugador
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        // Speed a 0 y trigger de ataque
        UpdateAnimationSpeed(0f);
        animator.SetTrigger(attackHash);

        // Reproducir sonido de ataque
        if (fuenteAudioPrincipal != null && sonidoAtaque != null)
            fuenteAudioPrincipal.PlayOneShot(sonidoAtaque);

        // Esperar un momento antes de hacer el daño (para que coincida con la animación)
        yield return new WaitForSeconds(attackAnimationDuration * 0.5f);

        // Hacer daño si sigue cerca
        if (Vector3.Distance(transform.position, player.position) < proximityAttackRange)
        {
            playerScript.TakeDamage(damageAmount, transform.position);
        }

        // Esperar a que termine la animación
        yield return new WaitForSeconds(attackAnimationDuration * 0.5f);

        lastAttackTime = Time.time;
        isAttacking = false;
        agent.isStopped = false;
    }

    #endregion

    #region Sistema de Daño por Colisión

    void DealCollisionDamage(Collision collision)
    {
        // Solo hacer daño si ha pasado suficiente tiempo desde el último daño por colisión
        if (Time.time > lastCollisionDamageTime + collisionDamageCooldown)
        {
            if (playerScript != null)
            {
                // Aplicar daño
                playerScript.TakeDamage(collisionDamage, transform.position);

                // Empujar al jugador
                PushPlayer();

                lastCollisionDamageTime = Time.time;
                consecutiveHits++;

                Debug.Log("¡Enemigo hizo daño por colisión! (-" + collisionDamage + " HP) | Golpes: " + consecutiveHits);

                // Si alcanzó el límite de golpes, aturdirse
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

        // Calcular dirección de empuje (alejando del enemigo)
        Vector3 pushDirection = (player.position - transform.position).normalized;
        pushDirection.y = 0; // Mantener el empuje horizontal

        // Aplicar empuje al jugador usando su método de knockback mejorado
        playerScript.ApplyCustomKnockback(pushDirection, pushForce);
    }

    IEnumerator StunEnemy()
    {
        // Cambiar estado a aturdido
        isStunned = true;
        currentState = EnemyState.STUNNED;

        // Detener al agente
        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        // Poner animación en Idle
        UpdateAnimationSpeed(0f);

        // Reproducir rugido
        if (fuenteAudioPrincipal != null && sonidoRugidoGolpe != null)
        {
            fuenteAudioPrincipal.PlayOneShot(sonidoRugidoGolpe);
        }

        Debug.Log("¡Enemigo aturdido! Esperando " + stunDuration + " segundos...");

        // Esperar el tiempo de aturdimiento
        yield return new WaitForSeconds(stunDuration);

        // Resetear contador y estado
        consecutiveHits = 0;
        isStunned = false;
        agent.isStopped = false;

        // Volver a patrullar
        currentState = EnemyState.PATROL;

        Debug.Log("Enemigo recuperado del aturdimiento.");
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

        // Visualizar rango de ataque
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, proximityAttackRange);
    }
    #endregion
}