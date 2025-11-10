using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

public class EnemyDirector : MonoBehaviour
{
    #region Variables

    [Header("Referencias")]
    public Transform player;
    public EnemyController enemyController;

    [Header("Lógica del Director")]
    public float maxLeashDistance = 50f;
    public float pressureInterval = 45f;
    public LayerMask obstacleLayer;

    [Header("Lógica de Aparición")]
    public float minSpawnDistance = 15f;
    public float maxSpawnDistance = 40f;

    private NavMeshAgent enemyAgent;
    private GameObject[] allSpawnPoints;
    private float pressureTimer = 0f;

    #endregion

    #region Métodos de Unity

    void Start()
    {
        enemyAgent = enemyController.GetComponent<NavMeshAgent>();
        allSpawnPoints = GameObject.FindGameObjectsWithTag("EnemySpawnPoint");
        if (allSpawnPoints.Length == 0) { Debug.LogError("Director AI: No se encontró ningún 'EnemySpawnPoint'."); }
        if (player == null || enemyController == null) { Debug.LogError("Director AI: Falta la referencia del Player o del Enemigo."); this.enabled = false; }
    }

    void Update()
    {
        // --- LÓGICA MODIFICADA ---
        // Si el enemigo está investigando (persiguiendo), reinicia el timer.
        if (enemyController.currentState == EnemyController.EnemyState.INVESTIGATE)
        {
            pressureTimer = 0;
            return;
        }
        // --- FIN DE LA MODIFICACIÓN ---

        pressureTimer += Time.deltaTime;
        float distanceToEnemy = Vector3.Distance(player.position, enemyController.transform.position);

        bool isTooFar = (distanceToEnemy > maxLeashDistance);
        bool isTimeForPressure = (pressureTimer > pressureInterval);

        if (isTooFar || isTimeForPressure)
        {
            TeleportEnemyNearPlayer();
            pressureTimer = 0;
        }
    }
    #endregion

    #region Lógica de Teletransportación

    void TeleportEnemyNearPlayer()
    {
        List<Transform> goodSpawnPoints = new List<Transform>();
        foreach (var point in allSpawnPoints)
        {
            float distToPlayer = Vector3.Distance(point.transform.position, player.position);
            if (distToPlayer >= minSpawnDistance && distToPlayer <= maxSpawnDistance)
            {
                bool isObstructed = Physics.Linecast(point.transform.position, player.position, obstacleLayer);
                if (isObstructed)
                {
                    goodSpawnPoints.Add(point.transform);
                }
            }
        }

        if (goodSpawnPoints.Count > 0)
        {
            Transform chosenPoint = goodSpawnPoints[Random.Range(0, goodSpawnPoints.Count)];
            enemyAgent.Warp(chosenPoint.position);
            enemyController.ChangeState(EnemyController.EnemyState.PATROL);
        }
    }
    #endregion
}