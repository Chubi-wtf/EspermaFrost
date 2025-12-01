using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

public class EnemyDirector : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;
    public EnemyController enemyController;

    [Header("Configuración")]
    public float maxLeashDistance = 50f; // Si está patrullando más lejos que esto, lo traemos
    public float pressureInterval = 45f;
    public LayerMask obstacleLayer;
    public float minSpawnDistance = 15f;
    public float maxSpawnDistance = 40f;

    private NavMeshAgent enemyAgent;
    private GameObject[] allSpawnPoints;
    private float pressureTimer = 0f;

    void Start()
    {
        if (enemyController == null) { this.enabled = false; return; }
        enemyAgent = enemyController.GetComponent<NavMeshAgent>();
        allSpawnPoints = GameObject.FindGameObjectsWithTag("EnemySpawnPoint");
    }

    void Update()
    {
        if (enemyController == null) return;

        // --- CORRECCIÓN CLAVE ---
        // Si el enemigo NO está patrullando (o sea, está Persiguiendo o Buscando),
        // el Director no debe molestar.
        if (enemyController.currentState != EnemyController.EnemyState.PATROL)
        {
            pressureTimer = 0; // Reseteamos la presión
            return;
        }

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

    void TeleportEnemyNearPlayer()
    {
        if (allSpawnPoints.Length == 0) return;

        List<Transform> goodSpawnPoints = new List<Transform>();
        foreach (var point in allSpawnPoints)
        {
            float distToPlayer = Vector3.Distance(point.transform.position, player.position);

            if (distToPlayer >= minSpawnDistance && distToPlayer <= maxSpawnDistance)
            {
                if (Physics.Linecast(point.transform.position, player.position, obstacleLayer))
                {
                    goodSpawnPoints.Add(point.transform);
                }
            }
        }

        if (goodSpawnPoints.Count > 0)
        {
            Transform chosenPoint = goodSpawnPoints[Random.Range(0, goodSpawnPoints.Count)];
            enemyAgent.Warp(chosenPoint.position);

            // Forzamos estado de patrulla al teletransportar
            enemyController.currentState = EnemyController.EnemyState.PATROL;
        }
    }
}