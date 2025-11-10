using UnityEngine;
using UnityEngine.AI;
using System;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    #region Variables

    [Header("Configuración de Patrullaje")]
    public float wanderRadius = 20f;
    public float wanderInterval = 5f;
    private float timer;

    [Header("Configuración de IA")]
    public Transform player;
    public float patrolSpeed = 3f;
    public float investigateSpeed = 6.5f; // <-- Nueva velocidad para investigar

    [Header("Configuración de Ataque")]
    public float proximityAttackRange = 1.5f;
    public float damageAmount = 25f;
    public float attackCooldown = 2f;
    private float lastAttackTime = -99f;

    [Header("Configuración de Audio")]
    public AudioSource fuenteAudioPrincipal;
    public AudioClip sonidoPatrullaLoop;
    public AudioClip sonidoRugido; // <-- Un solo sonido de rugido
    [Space(10)]
    public AudioSource fuenteAudioPasos;
    public AudioClip[] sonidosPasos;

    // Variables privadas
    private NavMeshAgent agent;
    private Animator anim;
    private Vector3 lastHeardPosition;
    private bool canHearPlayer = false;
    public static Action<Vector3> OnGlobalNoise;

    #endregion

    #region Estados de IA
    public enum EnemyState
    {
        PATROL,
        INVESTIGATE
    }
    public EnemyState currentState;
    #endregion

    #region Métodos de Unity

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>(); // <-- ¡Arreglado!
        timer = wanderInterval;
        currentState = EnemyState.PATROL;
        agent.speed = patrolSpeed;
        lastAttackTime = -attackCooldown;

        if (anim == null) { Debug.LogError("¡ERROR GRAVE! No se encontró el 'Animator'."); }
    }

    private void OnEnable() { OnGlobalNoise += HearGlobalNoise; }
    private void OnDisable() { OnGlobalNoise -= HearGlobalNoise; }

    void Update()
    {
        HandleStateMachine();
        CheckProximityAttack();

        // Esto siempre envía la velocidad al Blend Tree
        if (anim != null)
        {
            anim.SetFloat("Speed", agent.velocity.magnitude);
        }
    }
    #endregion

    #region Detección de Sonido

    void HearGlobalNoise(Vector3 noisePosition)
    {
        if (currentState == EnemyState.INVESTIGATE) return;
        lastHeardPosition = noisePosition;
        ChangeState(EnemyState.INVESTIGATE);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("PlayerNoise"))
        {
            canHearPlayer = true;
            lastHeardPosition = other.transform.position;

            if (currentState == EnemyState.PATROL)
            {
                ChangeState(EnemyState.INVESTIGATE);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PlayerNoise"))
        {
            canHearPlayer = false;
        }
    }
    #endregion

    #region Lógica de Estados

    void HandleStateMachine()
    {
        switch (currentState)
        {
            case EnemyState.PATROL:
                PatrolBehavior();
                break;
            case EnemyState.INVESTIGATE:
                InvestigateBehavior();
                break;
        }
    }

    public void ChangeState(EnemyState newState)
    {
        if (currentState == newState) return;

        currentState = newState;

        switch (currentState)
        {
            case EnemyState.PATROL:
                agent.speed = patrolSpeed;
                if (fuenteAudioPrincipal != null && sonidoPatrullaLoop != null)
                {
                    fuenteAudioPrincipal.clip = sonidoPatrullaLoop;
                    fuenteAudioPrincipal.loop = true;
                    fuenteAudioPrincipal.Play();
                }
                break;
            case EnemyState.INVESTIGATE:
                agent.speed = investigateSpeed; // <-- Velocidad rápida
                if (fuenteAudioPrincipal != null && sonidoRugido != null)
                {
                    fuenteAudioPrincipal.PlayOneShot(sonidoRugido); // <-- ¡Solo ruge!
                }
                break;
        }
    }

    void PatrolBehavior()
    {
        timer += Time.deltaTime;
        if (timer >= wanderInterval)
        {
            Vector3 newPos = GetRandomNavMeshPosition(transform.position, wanderRadius);
            agent.SetDestination(newPos);
            timer = 0;
        }
    }

    void InvestigateBehavior()
    {
        if (canHearPlayer)
        {
            agent.SetDestination(lastHeardPosition);
        }

        if (!canHearPlayer)
        {
            if (agent.remainingDistance < 1.0f && !agent.pathPending)
            {
                ChangeState(EnemyState.PATROL);
            }
        }
    }
    #endregion

    #region Métodos Auxiliares
    void CheckProximityAttack()
    {
        if (Vector3.Distance(transform.position, player.position) < proximityAttackRange &&
            Time.time > lastAttackTime + attackCooldown)
        {
            if (player.TryGetComponent<PlayerMovement>(out var playerMovement))
            {
                playerMovement.TakeDamage(damageAmount, transform.position);
                lastAttackTime = Time.time;
            }
        }
    }

    public static Vector3 GetRandomNavMeshPosition(Vector3 origin, float distance)
    {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * distance;
        randomDirection += origin;
        NavMeshHit navHit;
        if (NavMesh.SamplePosition(randomDirection, out navHit, distance, NavMesh.AllAreas))
        {
            return navHit.position;
        }
        return origin;
    }
    #endregion

    #region Eventos de Animacion
    // (Ya no necesitamos IniciarPersecucion)

    public void ReproducirPaso()
    {
        if (fuenteAudioPasos != null && sonidosPasos.Length > 0)
        {
            AudioClip clip = sonidosPasos[UnityEngine.Random.Range(0, sonidosPasos.Length)];
            fuenteAudioPasos.PlayOneShot(clip);
        }
    }
    #endregion
}