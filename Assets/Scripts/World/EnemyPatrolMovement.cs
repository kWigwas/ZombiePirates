using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyPatrolMovement : MonoBehaviour
{
    public Vector3 leftPoint;
    public Vector3 rightPoint;
    public float speed = 2f;
    public string combatSceneName = "CombatScene";

    private bool movingRight = true;
    private bool encounterStarted = false;

    void Update()
    {
        if (encounterStarted) return;

        if (movingRight)
        {
            transform.position += Vector3.right * speed * Time.deltaTime;
            if (transform.position.x >= rightPoint.x)
                movingRight = false;
        }
        else
        {
            transform.position += Vector3.left * speed * Time.deltaTime;
            if (transform.position.x <= leftPoint.x)
                movingRight = true;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (encounterStarted) return;
        if (!collision.gameObject.CompareTag("Player")) return;

        encounterStarted = true;

        EnemyDifficultyMarker marker = GetComponent<EnemyDifficultyMarker>();
        CombatDifficulty difficulty = marker != null ? marker.difficulty : CombatDifficulty.Easy;
        string enemyName = marker != null ? marker.enemyDisplayName : gameObject.name;

        CombatSessionState.PrepareEncounter(difficulty, enemyName);
        SceneManager.LoadScene(combatSceneName);
    }
}