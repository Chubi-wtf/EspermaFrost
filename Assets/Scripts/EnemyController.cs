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
    public float chaseSpeed = 5.0f; // Opcional: si quieres que corra un poco más al perseguir
    public float wanderRadius = 20f;
    public float wanderInterval = 5f;

    [Header("Sensores")]
    [Tooltip("Multiplicador de audición. 1 = Normal. >1 = Oye más lejos.")]
    public float hearingSensitivity = 1.0f;

    [Header("Búsqueda (Tu requerimiento)")]
    [Tooltip("Tiempo que se queda buscando en la última posición conocida antes de volver a patrullar.")]
    public float searchDuration = 5f;

    [Header("Ataque")]
    public float proximityAttackRange = 1.5f;
    public float damageAmount = 25f;
    public float attackCooldown = 2f;
    private float lastAttackTime = -99f;
    #endregion

    #region Estado Interno
    public enum EnemyState
    {
        PATROL, // Patrullando al azar
        CHASE,  // Persiguiendo por ruido
        SEARCH  // Buscando en la última posición conocida
    }

    // Variable PÚBLICA para que el Director la lea
    public EnemyState currentState;

    private NavMeshAgent agent;
    private float stateTimer; // Timer multiuso (para patrulla y búsqueda)
    private Vector3 lastKnownPosition; // Donde te escuchó por última vez
    #endregion

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        currentState = EnemyState.PATROL;

        // Referencia segura al script del player
        if (player != null) playerScript = player.GetComponent<PlayerMovement>();
        else if (PlayerMovement.Instance != null) playerScript = PlayerMovement.Instance;
    }

    void Update()
    {
        if (playerScript == null) return;

        // 1. Detectar si escucha al jugador AHORA MISMO
        bool canHearNow = CheckIfCanHearPlayer();

        // 2. Máquina de Estados
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
    }

    #region Lógica de Sentidos
    bool CheckIfCanHearPlayer()
    {
        // Distancia real
        float distance = Vector3.Distance(transform.position, player.position);
        // Radio actual del jugador (depende si corre, camina o se agacha)
        float noiseRadius = playerScript.noiseCollider.radius;

        // Si estoy DENTRO de su esfera de ruido, lo escucho.
        return distance <= (noiseRadius * hearingSensitivity);
    }
    #endregion

    #region Comportamientos
    void HandlePatrol(bool canHearNow)
    {
        // TRANSICIÓN: Si escucha algo, empieza a perseguir
        if (canHearNow)
        {
            StartChasing();
            return;
        }

        agent.speed = patrolSpeed;
        stateTimer += Time.deltaTime;

        if (stateTimer >= wanderInterval || agent.remainingDistance < 0.5f)
        {
            Vector3 newPos = RandomNavMeshPosition(transform.position, wanderRadius);
            agent.SetDestination(newPos);
            stateTimer = 0;
        }
    }

    void HandleChase(bool canHearNow)
    {
        // Mientras escuche, actualiza la posición y el destino
        if (canHearNow)
        {
            agent.speed = chaseSpeed;
            lastKnownPosition = player.position; // Memoriza dónde estás
            agent.SetDestination(lastKnownPosition); // Ve hacia ti
        }
        else
        {
            // TRANSICIÓN: Si dejas de hacer ruido (te agachas o sales del rango)
            // El enemigo NO vuelve a patrullar, pasa a BUSCARTE.
            currentState = EnemyState.SEARCH;
            agent.SetDestination(lastKnownPosition); // Va al último lugar donde sonaste
            stateTimer = 0; // Reseteamos el timer para contar cuánto tiempo busca
        }
    }

    void HandleSearch(bool canHearNow)
    {
        // Si vuelve a escuchar ruido, interrumpe la búsqueda y persigue de nuevo
        if (canHearNow)
        {
            StartChasing();
            return;
        }

        agent.speed = patrolSpeed; // Baja la velocidad para buscar con calma

        // Verifica si ya llegó al punto donde te escuchó por última vez
        if (agent.remainingDistance < 1.0f && !agent.pathPending)
        {
            // Ya llegué al lugar del ruido. Ahora espero.
            stateTimer += Time.deltaTime;

            // Aquí podrías poner una animación de "Mirar alrededor"

            if (stateTimer >= searchDuration)
            {
                // Se acabó el tiempo de búsqueda, me rindo.
                currentState = EnemyState.PATROL;
                stateTimer = 0;
            }
        }
    }

    void StartChasing()
    {
        currentState = EnemyState.CHASE;
        agent.ResetPath();
    }
    #endregion

    #region Utilidades
    void CheckProximityAttack()
    {
        if (Vector3.Distance(transform.position, player.position) < proximityAttackRange &&
            Time.time > lastAttackTime + attackCooldown)
        {
            playerScript.TakeDamage(damageAmount, transform.position);
            lastAttackTime = Time.time;
        }
    }

    Vector3 RandomNavMeshPosition(Vector3 origin, float dist)
    {
        Vector3 randDir = Random.insideUnitSphere * dist;
        randDir += origin;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randDir, out navHit, dist, NavMesh.AllAreas);
        return navHit.position;
    }

    // Debug visual
    private void OnDrawGizmos()
    {
        if (currentState == EnemyState.CHASE) { Gizmos.color = Color.red; Gizmos.DrawLine(transform.position, player.position); }
        if (currentState == EnemyState.SEARCH) { Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(lastKnownPosition, 1f); }
    }
    #endregion
}