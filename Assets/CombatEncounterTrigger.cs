using UnityEngine;
using UnityEngine.SceneManagement;

public class CombatEncounterTrigger : MonoBehaviour
{
    [Header("Encounter")]
    [SerializeField] private CombatDifficulty difficulty = CombatDifficulty.Easy;
    [SerializeField] private string encounterLabel = "Enemy Ship";
    [SerializeField] private string combatSceneName = "CombatScene";
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool disableAfterTrigger = true;

    private bool triggered;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            StartEncounter();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            StartEncounter();
        }
    }

    private void StartEncounter()
    {
        if (triggered)
        {
            return;
        }

        triggered = true;

        CombatSessionState.BeginEncounter(
            difficulty,
            encounterLabel,
            SceneManager.GetActiveScene().name);

        if (disableAfterTrigger)
        {
            gameObject.SetActive(false);
        }

        SceneManager.LoadScene(combatSceneName);
    }
}
