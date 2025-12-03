using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    #region Configuración

    [Header("Referencias")]
    public Transform player;
    private PlayerMovement playerScript;

    [Header("Movimiento")]
    public float patrolSpeed = 3.5f;
    public float chaseSpeed = 5.0f; // Velocidad al perseguir Y buscar frenéticamente
    public float wanderRadius = 20f;
    public float wanderInterval = 5f;

    [Header("Sensores (Oído)")]
    [Tooltip("Multiplicador de audición. 1 = Normal.")]
    public float hearingSensitivity = 1.0f;

    [Header("Búsqueda Frenética (NUEVO)")]
    [Tooltip("Radio alrededor de tu última posición donde el enemigo correrá a buscar.")]
    public float searchRadius = 10f;
    [Tooltip("Tiempo que se queda esperando en el punto de búsqueda antes de rendirse.")]
    public float searchDuration = 4f;

    [Header("Ataque")]
    public float proximityAttackRange = 1.5f;
    public float damageAmount = 25f;
    public float attackCooldown = 2f;
    private float lastAttackTime = -99f;

    [Header("Audio (Opcional)")]
    public AudioSource fuenteAudioPrincipal;
    public AudioClip sonidoRugido;

    [Header("Animación")] // Renombrado a Animación para claridad
    private Animator animator;
    // Parámetro para el Trigger de ataque (asumiendo que tienes uno)
    [Tooltip("Nombre del Trigger para la animación de ataque.")]
    public string attackTriggerName = "Attack";
    // Usaremos un parámetro de velocidad para controlar Idle/Walk/Run
    // Asumiendo que IsCrouching e IsMoving se controlarán implícitamente por Speed > 0
    // Si necesitas IsCrouching, se controlaría con una lógica de estado adicional.
    #endregion

    #region Estado Interno
    public enum EnemyState
    {
        PATROL, // Patrullando tranquilo
        CHASE,  // Persiguiendo al jugador (Sabe dónde estás)
        SEARCH  // Corriendo a buscarte a una zona cercana (No sabe dónde estás)
    }

    public EnemyState currentState;

    private NavMeshAgent agent;
    private float stateTimer;
    private Vector3 lastKnownPosition;
    private Vector3 searchTargetPosition; // A donde correrá a buscarte
    #endregion

    #region Métodos de Unity

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        currentState = EnemyState.PATROL;
        agent.speed = patrolSpeed;

        // **INICIO DE CAMBIOS DE ANIMACIÓN**
        animator = GetComponent<Animator>();
        if (animator == null) Debug.LogError("Animator no encontrado en el objeto.");
        // **FIN DE CAMBIOS DE ANIMACIÓN**

        if (player != null) playerScript = player.GetComponent<PlayerMovement>();
        else if (PlayerMovement.Instance != null)
        {
            playerScript = PlayerMovement.Instance;
            player = PlayerMovement.Instance.transform;
        }
    }

    void Update()
    {
        if (playerScript == null || player == null) return;

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
        }

        CheckProximityAttack();
        // **ACTUALIZACIÓN DE ANIMACIÓN DE MOVIMIENTO (GLOBAL)**
        UpdateMovementAnimation();
        // **FIN DE CAMBIOS DE ANIMACIÓN**
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
            // Te escucha: Actualiza destino constante y corre
            agent.speed = chaseSpeed;
            lastKnownPosition = player.position;
            agent.SetDestination(lastKnownPosition);
        }
        else
        {
            // Te perdió. En vez de ir a donde estabas, calcula un punto random CERCA de donde estabas.
            currentState = EnemyState.SEARCH;

            // Calculamos un punto aleatorio alrededor de la última posición conocida
            searchTargetPosition = RandomNavMeshPosition(lastKnownPosition, searchRadius);

            agent.SetDestination(searchTargetPosition);
            agent.speed = chaseSpeed; // ¡SIGUE CORRIENDO!

            stateTimer = 0;
        }
    }

    // --- BÚSQUEDA (Corre a ver si estás por ahí) ---
    void HandleSearch(bool canHearNow)
    {
        // Si te vuelve a escuchar, cancela la búsqueda y persigue
        if (canHearNow) { StartChasing(); return; }

        // Mantiene la velocidad de correr mientras viaja al punto de búsqueda
        if (agent.remainingDistance > 1.0f)
        {
            agent.speed = chaseSpeed;
        }
        else
        {
            // Llegó al punto random de búsqueda. Ahora se calma y mira.
            if (!agent.pathPending)
            {
                agent.speed = 0; // Se detiene para escuchar/mirar

                stateTimer += Time.deltaTime;

                if (stateTimer >= searchDuration)
                {
                    // No te encontró, vuelve a patrullar
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
            if (fuenteAudioPrincipal != null && sonidoRugido != null)
                fuenteAudioPrincipal.PlayOneShot(sonidoRugido);
        }
    }
    #endregion

    // --- NUEVA LÓGICA DE ANIMACIÓN ---
    #region Animación

    void UpdateMovementAnimation()
    {
        if (animator == null) return;

        // La magnitud de la velocidad actual (Velocidad en el mundo real, no solo la meta del agente)
        float currentSpeed = agent.velocity.magnitude;

        // Si el agente está persiguiendo (CHASE) o buscando (SEARCH), el enemigo debería "correr"
        // Si no está corriendo, usa la velocidad real para Walk/Idle (PATROL)
        bool isRunning = currentState == EnemyState.CHASE || currentState == EnemyState.SEARCH;

        // Usamos la velocidad para transiciones Idle/Walk/Run
        animator.SetFloat("Speed", currentSpeed);

        // Seteamos el bool IsRunning para las transiciones específicas de correr (si las tienes)
        // Aunque Speed debería ser suficiente, incluimos IsRunning por si las transiciones lo usan.
        animator.SetBool("IsRunning", isRunning);

        // Seteamos IsMoving (Útil para saber si hay algún tipo de movimiento)
        animator.SetBool("IsMoving", currentSpeed > 0.01f);

        // NO seteamos IsCrouching aquí, ya que el enemigo no tiene esa lógica en este script.
    }

    void CheckProximityAttack()
    {
        if (Vector3.Distance(transform.position, player.position) < proximityAttackRange &&
            Time.time > lastAttackTime + attackCooldown)
        {
            // **ACTIVAR ANIMACIÓN DE ATAQUE**
            if (animator != null && !string.IsNullOrEmpty(attackTriggerName))
            {
                animator.SetTrigger(attackTriggerName);
            }
            // **FIN DE CAMBIOS DE ANIMACIÓN**

            // Lógica de daño
            playerScript.TakeDamage(damageAmount, transform.position);
            lastAttackTime = Time.time;
        }
    }
    #endregion
    // --- FIN DE NUEVA LÓGICA DE ANIMACIÓN ---


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
            // Dibuja el área donde está buscando
            Gizmos.color = new Color(1, 0.5f, 0, 0.5f); // Naranja
            Gizmos.DrawWireSphere(lastKnownPosition, searchRadius);
            // Dibuja el punto exacto al que está corriendo
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(searchTargetPosition, 0.5f);
        }
    }
    #endregion
}