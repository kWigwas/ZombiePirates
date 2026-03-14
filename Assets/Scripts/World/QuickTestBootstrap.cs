using UnityEngine;

public class QuickTestBootstrap : MonoBehaviour
{
    private Sprite runtimeSprite;

    void Start()
    {
        SetupCamera();
        CreateRuntimeSprite();

        SpawnOcean();
        SpawnPlayer();
        SpawnEnemies();
    }

    void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        cam.orthographic = true;
        cam.orthographicSize = 6f;
        cam.transform.position = new Vector3(0f, 0f, -10f);
        cam.backgroundColor = new Color(0.25f, 0.45f, 0.75f);
    }

    void CreateRuntimeSprite()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();

        runtimeSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }

    void SpawnOcean()
    {
        GameObject ocean = new GameObject("Ocean");
        var sr = ocean.AddComponent<SpriteRenderer>();
        sr.sprite = runtimeSprite;
        sr.color = new Color(0.15f, 0.45f, 0.85f);
        ocean.transform.position = new Vector3(0f, 0f, 5f);
        ocean.transform.localScale = new Vector3(40f, 25f, 1f);
    }

    void SpawnPlayer()
    {
        GameObject player = new GameObject("PlayerShip");
        player.tag = "Player";
        player.transform.position = Vector3.zero;
        player.transform.localScale = new Vector3(1.5f, 1f, 1f);

        var sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = runtimeSprite;
        sr.color = new Color(0.95f, 0.95f, 0.95f);

        player.AddComponent<BoxCollider2D>();

        var rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        player.AddComponent<SimpleShipController2D>();
    }

    void SpawnEnemies()
    {
        SpawnEnemy("EnemyShip_Easy",   new Vector3(-6f,  1.5f, 0f), new Color(0.8f, 0.2f, 0.8f), CombatDifficulty.Easy);
        SpawnEnemy("EnemyShip_Medium", new Vector3( 8f, -1.8f, 0f), new Color(1f, 0.5f, 0.2f),   CombatDifficulty.Medium);
        SpawnEnemy("EnemyShip_Hard",   new Vector3( 5f,  2.8f, 0f), Color.red,                    CombatDifficulty.Hard);
    }

    void SpawnEnemy(string name, Vector3 pos, Color color, CombatDifficulty difficulty)
    {
        GameObject enemy = new GameObject(name);
        enemy.transform.position = pos;
        enemy.transform.localScale = new Vector3(1.5f, 1f, 1f);

        var sr = enemy.AddComponent<SpriteRenderer>();
        sr.sprite = runtimeSprite;
        sr.color = color;

        enemy.AddComponent<BoxCollider2D>();

        var rb = enemy.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.bodyType = RigidbodyType2D.Kinematic;

        var patrol = enemy.AddComponent<EnemyPatrolMovement>();
        patrol.leftPoint = pos + Vector3.left * 3f;
        patrol.rightPoint = pos + Vector3.right * 3f;
        patrol.combatSceneName = "CombatScene";

        var marker = enemy.AddComponent<EnemyDifficultyMarker>();
        marker.difficulty = difficulty;
        marker.enemyDisplayName = name;
    }
}