using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DynamicIslandSpawner
/// -------------------------------------------------------
/// Attach to any persistent GameObject in your scene (e.g. GameManager).
///
/// As the player sails around, islands are spawned in a ring ahead of them
/// and despawned when they fall too far behind.
///
/// Setup:
///   1. Create 3 sprites named island1, island2, island3 as Prefabs
///      and assign them to the islandPrefabs array in the Inspector.
///   2. Tag your player GameObject as "Player".
///   3. Tune the radius/density variables below to taste.
/// </summary>
public class IslandSpawner : MonoBehaviour
{
    [Header("Island Prefabs")]
    [Tooltip("Drag island1, island2, island3 prefabs here.")]
    public GameObject[] islandPrefabs;

    [Header("Spawn Ring")]
    [Tooltip("Islands spawn when a chunk enters this radius around the player.")]
    public float spawnRadius = 60f;

    [Tooltip("Islands are destroyed when their chunk centre exceeds this radius.")]
    public float despawnRadius = 90f;

    [Tooltip("Islands will never spawn within this radius of the player. Prevents spawning on top of the ship at start.")]
    public float clearRadius = 15f;

    [Header("Chunk Grid")]
    [Tooltip("The world is divided into chunks of this size. One island can spawn per chunk.")]
    public float chunkSize = 20f;

    [Tooltip("Chance (0–1) that any given chunk contains an island.")]
    [Range(0f, 1f)]
    public float spawnChance = 0.4f;

    [Header("Island Placement")]
    [Tooltip("Minimum distance between any two islands.")]
    public float minIslandSpacing = 8f;

    [Tooltip("Islands are placed randomly within this fraction of a chunk (0 = centre, 1 = full jitter).")]
    [Range(0f, 1f)]
    public float placementJitter = 0.8f;

    [Header("Island Size")]
    [Tooltip("Minimum random scale applied to a spawned island.")]
    public float minIslandScale = 0.5f;

    [Tooltip("Maximum random scale applied to a spawned island.")]
    public float maxIslandScale = 2f;

    // -------------------------------------------------------
    //  Private state
    // -------------------------------------------------------

    private Transform playerTransform;

    // Tracks which chunks have been evaluated: true = has an island, false = empty
    private Dictionary<Vector2Int, bool> checkedChunks = new Dictionary<Vector2Int, bool>();
    // Maps chunk key → spawned island GameObject
    private Dictionary<Vector2Int, GameObject> spawnedIslands = new Dictionary<Vector2Int, GameObject>();

    // -------------------------------------------------------
    //  Unity lifecycle
    // -------------------------------------------------------

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
        else
            Debug.LogWarning("[DynamicIslandSpawner] No GameObject tagged 'Player' found.");

        if (islandPrefabs == null || islandPrefabs.Length == 0)
            Debug.LogError("[DynamicIslandSpawner] No island prefabs assigned!");
    }

    private void Update()
    {
        if (playerTransform == null || islandPrefabs.Length == 0) return;

        EvaluateChunksAroundPlayer();
        DespawnDistantIslands();
    }

    // -------------------------------------------------------
    //  Core logic
    // -------------------------------------------------------

    /// <summary>
    /// Iterates over every chunk within spawnRadius and decides
    /// whether to spawn an island there.
    /// </summary>
    private void EvaluateChunksAroundPlayer()
    {
        Vector2 playerPos = playerTransform.position;
        int chunksInRadius = Mathf.CeilToInt(spawnRadius / chunkSize);

        Vector2Int playerChunk = WorldToChunk(playerPos);

        for (int x = -chunksInRadius; x <= chunksInRadius; x++)
        {
            for (int y = -chunksInRadius; y <= chunksInRadius; y++)
            {
                Vector2Int chunkKey = new Vector2Int(playerChunk.x + x, playerChunk.y + y);

                // Only process each chunk once
                if (checkedChunks.ContainsKey(chunkKey)) continue;

                Vector2 chunkCentre = ChunkToWorld(chunkKey);

                // Only spawn chunks actually within the circle
                if (Vector2.Distance(playerPos, chunkCentre) > spawnRadius) continue;

                // Never spawn within the clear radius (prevents islands on top of player at start)
                if (Vector2.Distance(playerPos, chunkCentre) < clearRadius) continue;

                // Use the chunk key as a deterministic seed so the same
                // chunk always produces the same result across sessions.
                bool shouldSpawn = SeededChance(chunkKey);
                checkedChunks[chunkKey] = shouldSpawn;

                if (shouldSpawn)
                    TrySpawnIsland(chunkKey, chunkCentre);
            }
        }
    }

    /// <summary>Removes islands whose chunk centre has moved past despawnRadius.</summary>
    private void DespawnDistantIslands()
    {
        Vector2 playerPos = playerTransform.position;
        List<Vector2Int> toRemove = new List<Vector2Int>();

        foreach (var kvp in spawnedIslands)
        {
            Vector2 chunkCentre = ChunkToWorld(kvp.Key);
            if (Vector2.Distance(playerPos, chunkCentre) > despawnRadius)
            {
                if (kvp.Value != null)
                    Destroy(kvp.Value);

                toRemove.Add(kvp.Key);
                // Also un-check the chunk so it can respawn if the player returns
                checkedChunks.Remove(kvp.Key);
            }
        }

        foreach (var key in toRemove)
            spawnedIslands.Remove(key);
    }

    /// <summary>
    /// Spawns a random island prefab inside the given chunk,
    /// provided it won't overlap an existing island.
    /// </summary>
    private void TrySpawnIsland(Vector2Int chunkKey, Vector2 chunkCentre)
    {
        // Apply random jitter within the chunk
        float maxOffset = chunkSize * 0.5f * placementJitter;
        Vector2 jitter = new Vector2(
            Random.Range(-maxOffset, maxOffset),
            Random.Range(-maxOffset, maxOffset)
        );
        Vector2 spawnPos = chunkCentre + jitter;

        // Reject if too close to an existing island
        foreach (var kvp in spawnedIslands)
        {
            if (kvp.Value == null) continue;
            if (Vector2.Distance(spawnPos, kvp.Value.transform.position) < minIslandSpacing)
                return;
        }

        GameObject prefab = islandPrefabs[Random.Range(0, islandPrefabs.Length)];
        GameObject island = Instantiate(prefab, spawnPos, Quaternion.identity);

        // Random rotation so islands don't all look identical
        island.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

        // Random uniform scale so islands vary in size
        float scale = Random.Range(minIslandScale, maxIslandScale);
        island.transform.localScale = Vector3.one * scale;

        spawnedIslands[chunkKey] = island;
    }

    // -------------------------------------------------------
    //  Helpers
    // -------------------------------------------------------

    private Vector2Int WorldToChunk(Vector2 worldPos)
    {
        return new Vector2Int(
            Mathf.FloorToInt(worldPos.x / chunkSize),
            Mathf.FloorToInt(worldPos.y / chunkSize)
        );
    }

    private Vector2 ChunkToWorld(Vector2Int chunkKey)
    {
        return new Vector2(
            (chunkKey.x + 0.5f) * chunkSize,
            (chunkKey.y + 0.5f) * chunkSize
        );
    }

    /// <summary>
    /// Produces a deterministic true/false for a chunk key using
    /// a simple hash so the same chunk always gives the same result.
    /// </summary>
    private bool SeededChance(Vector2Int key)
    {
        int hash = key.x * 73856093 ^ key.y * 19349663;
        Random.State oldState = Random.state;
        Random.InitState(hash);
        bool result = Random.value < spawnChance;
        Random.state = oldState;
        return result;
    }
}