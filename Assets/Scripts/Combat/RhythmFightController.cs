using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RhythmFightController : MonoBehaviour
{
    [Header("Scene Refs")]
    public RhythmLane[] lanes;
    public Transform[] spawnPoints;
    public TMP_Text countdownText;
    public TMP_Text infoText;
    public TMP_Text progressText;
    public TMP_Text resultText;
    public TMP_Text feedbackText;
    public Slider progressSlider;
    public Button restartButton;
    public Button returnButton;

    [Header("Gameplay")]
    public int totalNotes = 24;
    public float beatInterval = 0.6f;
    public float noteSpeed = 3.2f;
    public float missLineY = -3.95f;
    public string explorationSceneName = "SampleScene";

    [Header("Visual Refs")]
    public Sprite runtimeSprite;
    public Transform notesParent;
    public Transform playerShip;
    public Transform enemyShip;
    public SpriteRenderer playerMuzzleFlash;
    public SpriteRenderer enemyDamageFlash;

    private float countdownTimer = 3f;
    private float spawnTimer;
    private bool countdownFinished;
    private bool combatEnded;

    private readonly List<RhythmNote> liveNotes = new List<RhythmNote>();

    private int spawnedNotes;
    private int judgedNotes;
    private int hitNotes;
    private int missNotes;
    private int wrongPresses;

    private Vector3 playerShipBasePos;
    private Vector3 enemyShipBasePos;
    private Coroutine feedbackRoutine;

    public bool InputEnabled => countdownFinished && !combatEnded;

    void Start()
    {
        if (playerShip != null) playerShipBasePos = playerShip.position;
        if (enemyShip != null) enemyShipBasePos = enemyShip.position;

        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(RestartCombat);
            restartButton.gameObject.SetActive(false);
        }

        if (returnButton != null)
        {
            returnButton.onClick.RemoveAllListeners();
            returnButton.onClick.AddListener(ReturnToExploration);
            returnButton.gameObject.SetActive(false);
        }

        if (resultText != null) resultText.text = "";
        if (feedbackText != null) feedbackText.text = "";

        if (playerMuzzleFlash != null) playerMuzzleFlash.color = new Color(1f, 0.85f, 0.3f, 0f);
        if (enemyDamageFlash != null) enemyDamageFlash.color = new Color(1f, 0.55f, 0.25f, 0f);

        RefreshInfo();
        RefreshProgress();
    }

    void Update()
    {
        if (combatEnded) return;

        RunCountdown();
        HandleInputs();

        if (!countdownFinished) return;

        RunSpawner();
        CleanupDeadNotes();
        CheckForCombatEnd();
    }

    void RunCountdown()
    {
        if (countdownFinished) return;

        countdownTimer -= Time.deltaTime;

        if (countdownText != null)
        {
            if (countdownTimer > 2f) countdownText.text = "3";
            else if (countdownTimer > 1f) countdownText.text = "2";
            else if (countdownTimer > 0f) countdownText.text = "1";
            else if (countdownTimer > -0.7f) countdownText.text = "BROADSIDE!";
            else countdownText.text = "";
        }

        if (countdownTimer <= -0.7f)
        {
            countdownFinished = true;
            spawnTimer = beatInterval * 0.25f;
        }
    }

void HandleInputs()
{
    if (UnityEngine.InputSystem.Keyboard.current != null)
    {
        if (UnityEngine.InputSystem.Keyboard.current.aKey.wasPressedThisFrame) Debug.Log("A pressed");
        if (UnityEngine.InputSystem.Keyboard.current.sKey.wasPressedThisFrame) Debug.Log("S pressed");
        if (UnityEngine.InputSystem.Keyboard.current.kKey.wasPressedThisFrame) Debug.Log("K pressed");
        if (UnityEngine.InputSystem.Keyboard.current.lKey.wasPressedThisFrame) Debug.Log("L pressed");
    }

    if (!InputEnabled || lanes == null) return;

    for (int i = 0; i < lanes.Length; i++)
    {
        RhythmLane lane = lanes[i];

        if (lane == null || lane.hitPoint == null)
        {
            Debug.LogWarning("Lane or hitPoint missing on lane index " + i);
            continue;
        }

        if (!lane.WasPressedThisFrame())
            continue;

        RhythmNote activeNote = lane.GetClosestActiveNote();

        if (activeNote == null)
        {
            wrongPresses++;
            ShowFeedback("MISS!", new Color(1f, 0.45f, 0.45f));
            ShakeShip(playerShip, playerShipBasePos, 0.06f, 0.08f);
            RefreshProgress();
            continue;
        }

        float distance = Mathf.Abs(activeNote.transform.position.y - lane.hitPoint.position.y);
        Debug.Log("Lane " + lane.key + " distance: " + distance);

        if (distance <= 0.45f)
        {
            activeNote.ResolveHit();
            ShowFeedback("DIRECT HIT!", new Color(1f, 0.93f, 0.55f));
        }
        else if (distance <= 1.1f)
        {
            activeNote.ResolveHit();
            ShowFeedback("HIT!", new Color(0.75f, 1f, 0.8f));
        }
        else
        {
            wrongPresses++;
            ShowFeedback("MISS!", new Color(1f, 0.45f, 0.45f));
            ShakeShip(playerShip, playerShipBasePos, 0.06f, 0.08f);
            RefreshProgress();
        }
    }
}
void RunSpawner()
{
    if (spawnedNotes >= totalNotes) return;

    spawnTimer -= Time.deltaTime;
    if (spawnTimer > 0f) return;

    SpawnSingleNote();
    spawnedNotes++;
    spawnTimer = beatInterval;
    RefreshProgress();
}
    void SpawnSingleNote()
    {
        if (lanes == null || lanes.Length == 0) return;

        int laneIndex = Random.Range(0, lanes.Length);
        RhythmLane lane = lanes[laneIndex];
        if (lane == null) return;

        Vector3 pos = new Vector3(-4.5f + laneIndex * 3f, 4.5f, 0f);
        if (spawnPoints != null && laneIndex < spawnPoints.Length && spawnPoints[laneIndex] != null)
            pos = spawnPoints[laneIndex].position;

        GameObject noteGO = new GameObject("Note");
        if (notesParent != null)
            noteGO.transform.SetParent(notesParent, true);

        noteGO.transform.position = pos;
        noteGO.transform.localScale = new Vector3(0.62f, 0.62f, 1f);

        SpriteRenderer sr = noteGO.AddComponent<SpriteRenderer>();
        sr.sprite = runtimeSprite;
        sr.color = GetLaneColor(laneIndex);
        sr.sortingOrder = 20;

        RhythmNote note = noteGO.AddComponent<RhythmNote>();
        note.lane = lane;
        note.controller = this;
        note.moveSpeed = noteSpeed;
        note.missLineY = missLineY;

        liveNotes.Add(note);
    }

    Color GetLaneColor(int laneIndex)
    {
        switch (laneIndex)
        {
            case 0: return new Color(0.83f, 0.95f, 1f);
            case 1: return new Color(1f, 0.84f, 0.52f);
            case 2: return new Color(0.62f, 1f, 0.83f);
            case 3: return new Color(1f, 0.66f, 0.66f);
            default: return Color.white;
        }
    }

    void CleanupDeadNotes()
    {
        for (int i = liveNotes.Count - 1; i >= 0; i--)
        {
            if (liveNotes[i] == null)
                liveNotes.RemoveAt(i);
        }
    }

    void CheckForCombatEnd()
    {
        if (spawnedNotes < totalNotes) return;
        if (liveNotes.Count > 0) return;
        EndCombat();
    }

    void EndCombat()
    {
        combatEnded = true;
        int percent = GetAccuracyPercent();
        bool won = percent >= CombatSessionState.PendingTargetPercent;

        if (resultText != null)
            resultText.text = won ? "Victory! Enemy crew captured." : "Defeat! Your broadside failed.";

        if (countdownText != null)
            countdownText.text = won ? "VICTORY!" : "DEFEAT!";

        if (feedbackText != null)
            feedbackText.text = won ? "Sea route secured." : "Retreat and regroup.";

        if (restartButton != null) restartButton.gameObject.SetActive(true);
        if (returnButton != null) returnButton.gameObject.SetActive(true);
    }

    public void RegisterHit(RhythmNote note)
    {
        hitNotes++;
        judgedNotes++;
        RefreshProgress();

        FlashSprite(playerMuzzleFlash, new Color(1f, 0.88f, 0.35f, 1f), 0.14f);
        FlashSprite(enemyDamageFlash, new Color(1f, 0.5f, 0.22f, 0.9f), 0.16f);

        ShakeShip(playerShip, playerShipBasePos, 0.1f, 0.12f);
        ShakeShip(enemyShip, enemyShipBasePos, 0.14f, 0.15f);
    }

    public void RegisterMiss(RhythmNote note)
    {
        missNotes++;
        judgedNotes++;
        RefreshProgress();
        ShowFeedback("MISS!", new Color(1f, 0.45f, 0.45f));
        ShakeShip(playerShip, playerShipBasePos, 0.05f, 0.08f);
    }

    int GetAccuracyPercent()
    {
        if (judgedNotes <= 0) return 0;
        return Mathf.RoundToInt((hitNotes / (float)judgedNotes) * 100f);
    }

    void RefreshInfo()
    {
        if (infoText == null) return;

        infoText.text =
            "Enemy: " + CombatSessionState.PendingEnemyName + "\n" +
            "Difficulty: " + CombatSessionState.PendingDifficulty + "\n" +
            "Target Accuracy: " + CombatSessionState.PendingTargetPercent + "%\n" +
            "Keys: A  S  K  L";
    }

    void RefreshProgress()
    {
        int accuracy = GetAccuracyPercent();

        if (progressText != null)
        {
            progressText.text =
                "Resolved " + judgedNotes + "/" + totalNotes +
                "   Hits " + hitNotes +
                "   Misses " + missNotes +
                "   Accuracy " + accuracy + "%";
        }

        if (progressSlider != null)
        {
            progressSlider.minValue = 0f;
            progressSlider.maxValue = 100f;
            progressSlider.value = accuracy;
        }
    }

    void ShowFeedback(string msg, Color color)
    {
        if (feedbackText == null) return;
        if (feedbackRoutine != null) StopCoroutine(feedbackRoutine);
        feedbackRoutine = StartCoroutine(FeedbackRoutine(msg, color));
    }

    IEnumerator FeedbackRoutine(string msg, Color color)
    {
        feedbackText.text = msg;
        feedbackText.color = color;
        yield return new WaitForSeconds(0.25f);
        if (!combatEnded) feedbackText.text = "";
    }

    void FlashSprite(SpriteRenderer sr, Color color, float duration)
    {
        if (sr == null) return;
        StartCoroutine(FlashRoutine(sr, color, duration));
    }

    IEnumerator FlashRoutine(SpriteRenderer sr, Color color, float duration)
    {
        sr.color = color;
        yield return new WaitForSeconds(duration);
        sr.color = new Color(color.r, color.g, color.b, 0f);
    }

    void ShakeShip(Transform ship, Vector3 basePos, float amount, float duration)
    {
        if (ship == null) return;
        StartCoroutine(ShakeRoutine(ship, basePos, amount, duration));
    }

    IEnumerator ShakeRoutine(Transform ship, Vector3 basePos, float amount, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            ship.position = basePos + (Vector3)Random.insideUnitCircle * amount;
            yield return null;
        }
        ship.position = basePos;
    }

    public void RestartCombat()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToExploration()
    {
        SceneManager.LoadScene(explorationSceneName);
    }
}