using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EnemySpawner
/// -------------------------------------------------------
/// Attach to any persistent GameObject in your scene (e.g. GameManager).
///
/// Enemies spawn in a ring around the player as they sail around,
/// and despawn when they drift too far away.
///
/// Setup:
///   1. Make your enemy ship a Prefab and assign it to enemyPrefabs.
///   2. Tag your player GameObject as "Player".
///   3. Tune the variables below to taste.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    [Tooltip("Drag one or more enemy ship prefabs here. A random one is picked each spawn.")]
    public GameObject[] enemyPrefabs;

    [Header("Spawn Ring")]
    [Tooltip("Enemies spawn at exactly this distance from the player. They appear at the edge, not inside it.")]
    public float spawnRadius = 40f;

    [Tooltip("Enemies are destroyed when they exceed this distance from the player.")]
    public float despawnRadius = 60f;

    [Tooltip("Enemies will never spawn within this radius. Prevents enemies appearing on top of the player.")]
    public float clearRadius = 15f;

    [Header("Population")]
    [Tooltip("Maximum number of enemies alive at the same time.")]
    public int maxEnemies = 6;

    [Tooltip("How many seconds between each spawn attempt.")]
    public float spawnInterval = 3f;

    [Tooltip("How many enemies to try to spawn in one batch.")]
    public int spawnBatchSize = 1;

    [Header("Spawn Scatter")]
    [Tooltip("Random offset applied to the spawn position so enemies don't all appear at the exact same point on the ring.")]
    public float spawnScatter = 5f;

    // -------------------------------------------------------
    //  Private state
    // -------------------------------------------------------

    private Transform playerTransform;
    private List<GameObject> activeEnemies = new List<GameObject>();
    private float spawnTimer = 0f;

    // -------------------------------------------------------
    //  Unity lifecycle
    // -------------------------------------------------------

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
        else
            Debug.LogWarning("[EnemySpawner] No GameObject tagged 'Player' found.");

        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
            Debug.LogError("[EnemySpawner] No enemy prefabs assigned!");
    }

    private void Update()
    {
        if (playerTransform == null || enemyPrefabs.Length == 0) return;

        CleanUpDeadEnemies();
        DespawnDistantEnemies();
        TrySpawnBatch();
    }

    // -------------------------------------------------------
    //  Core logic
    // -------------------------------------------------------

    /// <summary>Removes null entries caused by enemies being destroyed externally.</summary>
    private void CleanUpDeadEnemies()
    {
        activeEnemies.RemoveAll(e => e == null);
    }

    /// <summary>Destroys enemies that have wandered past despawnRadius.</summary>
    private void DespawnDistantEnemies()
    {
        Vector2 playerPos = playerTransform.position;
        List<GameObject> toRemove = new List<GameObject>();

        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy == null) continue;
            if (Vector2.Distance(playerPos, enemy.transform.position) > despawnRadius)
            {
                Destroy(enemy);
                toRemove.Add(enemy);
            }
        }

        foreach (GameObject enemy in toRemove)
            activeEnemies.Remove(enemy);
    }

    /// <summary>Counts down and attempts to spawn a batch of enemies.</summary>
    private void TrySpawnBatch()
    {
        spawnTimer -= Time.deltaTime;
        if (spawnTimer > 0f) return;
        spawnTimer = spawnInterval;

        // Don't spawn if already at cap
        if (activeEnemies.Count >= maxEnemies) return;

        int toSpawn = Mathf.Min(spawnBatchSize, maxEnemies - activeEnemies.Count);
        for (int i = 0; i < toSpawn; i++)
            SpawnEnemy();
    }

    /// <summary>Picks a random point on the spawn ring and instantiates an enemy there.</summary>
    private void SpawnEnemy()
    {
        Vector2 playerPos = playerTransform.position;

        // Pick a random angle and place on the ring at spawnRadius
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector2 spawnPos = playerPos + new Vector2(
            Mathf.Cos(angle) * spawnRadius,
            Mathf.Sin(angle) * spawnRadius
        );

        // Apply a small random scatter so enemies don't cluster at identical positions
        spawnPos += Random.insideUnitCircle * spawnScatter;

        // Safety: reject if scatter pushed it inside clearRadius
        if (Vector2.Distance(playerPos, spawnPos) < clearRadius) return;

        GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);

        activeEnemies.Add(enemy);
    }

    // -------------------------------------------------------
    //  Editor gizmos
    // -------------------------------------------------------

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Vector3 pos = playerTransform != null ? playerTransform.position : transform.position;

        // Spawn ring — green
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(pos, spawnRadius);

        // Despawn ring — red
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(pos, despawnRadius);

        // Clear radius — yellow
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(pos, clearRadius);
    }
#endif
}