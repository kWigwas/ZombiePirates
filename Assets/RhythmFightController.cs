using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RhythmFightController : MonoBehaviour
{
    [Header("Core Timing")]
    [SerializeField] private float bpm = 120f;
    [SerializeField] private float songDelay = 0.75f;
    [SerializeField] private float beatsShownAhead = 4f;
    [SerializeField] private float perfectWindow = 0.10f;
    [SerializeField] private float goodWindow = 0.22f;
    [SerializeField] private float missWindow = 0.30f;

    [Header("Audio / Text")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private Text countdownText;
    [SerializeField] private Text feedbackText;
    [SerializeField] private Text stateText;
    [SerializeField] private Text targetText;
    [SerializeField] private Text difficultyText;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text hitRateText;

    [Header("Progress Bars")]
    [SerializeField] private Slider playerProgressSlider;
    [SerializeField] private Slider targetProgressSlider;

    [Header("Buttons")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button returnButton;

    [Header("Lanes")]
    [SerializeField] private RhythmLane[] lanes = Array.Empty<RhythmLane>();
    [SerializeField] private KeyCode[] laneKeys = new KeyCode[] { KeyCode.A, KeyCode.S, KeyCode.K, KeyCode.L };

    [Header("Visual FX")]
    [SerializeField] private Animator playerShipAnimator;
    [SerializeField] private Animator enemyShipAnimator;

    [Header("Beatmap")]
    [SerializeField] private List<BeatNote> beatmap = new List<BeatNote>();
    [SerializeField] private bool autoGenerateIfBeatmapEmpty = true;

    private readonly List<RhythmNote> liveNotes = new List<RhythmNote>();
    private readonly Queue<BeatNote> pendingNotes = new Queue<BeatNote>();

    private CombatDifficulty difficulty;
    private int targetPercent;
    private int score;
    private int totalNotes;
    private int judgedNotes;
    private int successfulHits;
    private int perfectHits;
    private int goodHits;
    private int missedHits;
    private bool fightStarted;
    private bool fightEnded;
    private double dspSongStartTime;
    private float secondsPerBeat;

    [Serializable]
    public struct BeatNote
    {
        [Range(0, 3)] public int laneIndex;
        [Min(0f)] public float beat;
    }

    private void Awake()
    {
        secondsPerBeat = 60f / bpm;

        difficulty = CombatSessionState.HasEncounter ? CombatSessionState.CurrentDifficulty : CombatDifficulty.Medium;
        targetPercent = CombatSessionState.HasEncounter ? CombatSessionState.TargetPercent : 60;

        if ((beatmap == null || beatmap.Count == 0) && autoGenerateIfBeatmapEmpty)
        {
            GenerateBeatmapForDifficulty();
        }

        beatmap.Sort((a, b) => a.beat.CompareTo(b.beat));
        totalNotes = beatmap.Count;
        foreach (BeatNote note in beatmap)
        {
            pendingNotes.Enqueue(note);
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(RestartCombat);
            restartButton.gameObject.SetActive(false);
        }

        if (returnButton != null)
        {
            returnButton.onClick.RemoveAllListeners();
            returnButton.onClick.AddListener(ReturnToWorld);
            returnButton.gameObject.SetActive(false);
        }

        UpdateUI();
        SetFeedback(string.Empty);
        SetState("Run into an enemy ship to start the duel.");
    }

    private void Start()
    {
        StartCoroutine(IntroSequence());
    }

    private void Update()
    {
        if (!fightStarted || fightEnded)
        {
            return;
        }

        float songBeat = GetCurrentSongBeat();
        SpawnUpcomingNotes(songBeat);
        HandleInput(songBeat);
        CheckMissedNotes(songBeat);
        UpdateLiveNotePositions(songBeat);
        CheckEndConditions();
    }

    private IEnumerator IntroSequence()
    {
        yield return CountdownStep("3", 0.7f);
        yield return CountdownStep("2", 0.7f);
        yield return CountdownStep("1", 0.7f);
        yield return CountdownStep("Dance!", 0.6f);

        if (countdownText != null)
        {
            countdownText.text = string.Empty;
        }

        SetState($"Hit at least {targetPercent}% to win.");
        StartFight();
    }

    private IEnumerator CountdownStep(string textValue, float wait)
    {
        if (countdownText != null)
        {
            countdownText.text = textValue;
        }

        yield return new WaitForSeconds(wait);
    }

    private void StartFight()
    {
        fightStarted = true;
        dspSongStartTime = AudioSettings.dspTime + songDelay;

        if (musicSource != null)
        {
            musicSource.PlayScheduled(dspSongStartTime);
        }
    }

    private float GetCurrentSongBeat()
    {
        double elapsed = AudioSettings.dspTime - dspSongStartTime;
        return (float)(elapsed / secondsPerBeat);
    }

    private void SpawnUpcomingNotes(float songBeat)
    {
        while (pendingNotes.Count > 0)
        {
            BeatNote next = pendingNotes.Peek();
            if (next.beat - songBeat > beatsShownAhead)
            {
                break;
            }

            pendingNotes.Dequeue();
            SpawnNote(next);
        }
    }

    private void SpawnNote(BeatNote data)
    {
        if (data.laneIndex < 0 || data.laneIndex >= lanes.Length || lanes[data.laneIndex] == null)
        {
            return;
        }

        RhythmNote note = lanes[data.laneIndex].SpawnNote(data.beat, beatsShownAhead);
        if (note != null)
        {
            liveNotes.Add(note);
        }
    }

    private void HandleInput(float songBeat)
    {
        for (int i = 0; i < laneKeys.Length && i < lanes.Length; i++)
        {
            if (!Input.GetKeyDown(laneKeys[i]))
            {
                continue;
            }

            RhythmNote best = FindClosestNoteInLane(i);
            if (best == null)
            {
                RegisterMiss("Miss");
                continue;
            }

            float deltaBeats = Mathf.Abs(best.TargetBeat - songBeat);
            float deltaSeconds = deltaBeats * secondsPerBeat;

            if (deltaSeconds <= perfectWindow)
            {
                RegisterHit(best, 200, "Perfect");
            }
            else if (deltaSeconds <= goodWindow)
            {
                RegisterHit(best, 100, "Good");
            }
            else
            {
                RegisterMiss(deltaSeconds <= missWindow ? "Late" : "Miss");
            }
        }
    }

    private RhythmNote FindClosestNoteInLane(int laneIndex)
    {
        RhythmNote best = null;
        float bestDistance = float.MaxValue;

        foreach (RhythmNote note in liveNotes)
        {
            if (note == null || note.LaneIndex != laneIndex || note.WasJudged)
            {
                continue;
            }

            float distance = Mathf.Abs(note.DistanceToHitLine);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = note;
            }
        }

        return best;
    }

    private void CheckMissedNotes(float songBeat)
    {
        for (int i = liveNotes.Count - 1; i >= 0; i--)
        {
            RhythmNote note = liveNotes[i];
            if (note == null)
            {
                liveNotes.RemoveAt(i);
                continue;
            }

            if (note.WasJudged)
            {
                continue;
            }

            float secondsLate = (songBeat - note.TargetBeat) * secondsPerBeat;
            if (secondsLate > missWindow)
            {
                note.MarkJudged();
                note.PlayMissAndDestroy();
                liveNotes.RemoveAt(i);
                RegisterMiss("Miss");
            }
        }
    }

    private void UpdateLiveNotePositions(float songBeat)
    {
        for (int i = liveNotes.Count - 1; i >= 0; i--)
        {
            RhythmNote note = liveNotes[i];
            if (note == null)
            {
                liveNotes.RemoveAt(i);
                continue;
            }

            note.UpdatePosition(songBeat, beatsShownAhead);
        }
    }

    private void RegisterHit(RhythmNote note, int points, string label)
    {
        note.MarkJudged();
        note.PlayHitAndDestroy();
        liveNotes.Remove(note);

        judgedNotes++;
        successfulHits++;
        score += points;

        if (label == "Perfect")
        {
            perfectHits++;
        }
        else
        {
            goodHits++;
        }

        if (playerShipAnimator != null)
        {
            playerShipAnimator.SetTrigger("Attack");
        }

        UpdateUI();
        SetFeedback(label + "!");
    }

    private void RegisterMiss(string label)
    {
        judgedNotes++;
        missedHits++;
        score = Mathf.Max(0, score - 25);

        if (enemyShipAnimator != null)
        {
            enemyShipAnimator.SetTrigger("Attack");
        }

        UpdateUI();
        SetFeedback(label);
    }

    private void CheckEndConditions()
    {
        int remainingNotes = totalNotes - judgedNotes;
        float currentPercent = GetSuccessPercent();
        float maxPossiblePercent = totalNotes <= 0 ? 0f : ((float)(successfulHits + remainingNotes) / totalNotes) * 100f;

        if (maxPossiblePercent < targetPercent)
        {
            EndFight(false, $"You cannot reach {targetPercent}% anymore.");
            return;
        }

        if (pendingNotes.Count == 0 && liveNotes.Count == 0)
        {
            bool won = currentPercent >= targetPercent;
            EndFight(won, won
                ? $"Victory! You hit {currentPercent:F0}% and cleared the target."
                : $"Defeat. You hit {currentPercent:F0}% but needed {targetPercent}%."
            );
        }
    }

    private float GetSuccessPercent()
    {
        if (totalNotes <= 0)
        {
            return 0f;
        }

        return ((float)successfulHits / totalNotes) * 100f;
    }

    private void EndFight(bool won, string endMessage)
    {
        if (fightEnded)
        {
            return;
        }

        fightEnded = true;
        fightStarted = false;

        if (musicSource != null)
        {
            musicSource.Stop();
        }

        foreach (RhythmNote note in liveNotes)
        {
            if (note != null)
            {
                Destroy(note.gameObject);
            }
        }
        liveNotes.Clear();

        SetState(endMessage);
        SetFeedback(won ? "Win" : "Defeat");

        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(true);
        }

        if (returnButton != null && !string.IsNullOrEmpty(CombatSessionState.ReturnSceneName))
        {
            returnButton.gameObject.SetActive(true);
        }
    }

    private void RestartCombat()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void ReturnToWorld()
    {
        if (!string.IsNullOrEmpty(CombatSessionState.ReturnSceneName))
        {
            string sceneName = CombatSessionState.ReturnSceneName;
            CombatSessionState.ClearEncounter();
            SceneManager.LoadScene(sceneName);
        }
    }

    private void UpdateUI()
    {
        float percent = GetSuccessPercent();

        if (difficultyText != null)
        {
            difficultyText.text = $"Difficulty: {difficulty}";
        }

        if (targetText != null)
        {
            string encounterLabel = CombatSessionState.HasEncounter ? CombatSessionState.EncounterLabel : "Enemy Ship";
            targetText.text = $"{encounterLabel} - Target: {targetPercent}%";
        }

        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }

        if (hitRateText != null)
        {
            hitRateText.text = $"Hit Rate: {percent:F0}% ({successfulHits}/{totalNotes})";
        }

        if (playerProgressSlider != null)
        {
            playerProgressSlider.maxValue = 100f;
            playerProgressSlider.value = percent;
        }

        if (targetProgressSlider != null)
        {
            targetProgressSlider.maxValue = 100f;
            targetProgressSlider.value = targetPercent;
        }
    }

    private void SetFeedback(string value)
    {
        if (feedbackText != null)
        {
            feedbackText.text = value;
        }
    }

    private void SetState(string value)
    {
        if (stateText != null)
        {
            stateText.text = value;
        }
    }

    private void GenerateBeatmapForDifficulty()
    {
        beatmap = new List<BeatNote>();
        int laneCount = lanes != null && lanes.Length > 0 ? Mathf.Min(4, lanes.Length) : 4;
        int seed = Guid.NewGuid().GetHashCode();
        UnityEngine.Random.InitState(seed);

        int bars;
        float densityChance;
        float offBeatChance;

        switch (difficulty)
        {
            case CombatDifficulty.Easy:
                bars = 8;
                densityChance = 0.55f;
                offBeatChance = 0.10f;
                break;
            case CombatDifficulty.Medium:
                bars = 10;
                densityChance = 0.8f;
                offBeatChance = 0.25f;
                break;
            case CombatDifficulty.Hard:
                bars = 12;
                densityChance = 1f;
                offBeatChance = 0.45f;
                break;
            default:
                bars = 10;
                densityChance = 0.8f;
                offBeatChance = 0.25f;
                break;
        }

        float beat = 2f;
        for (int bar = 0; bar < bars; bar++)
        {
            for (int step = 0; step < 4; step++)
            {
                if (UnityEngine.Random.value <= densityChance)
                {
                    beatmap.Add(new BeatNote
                    {
                        laneIndex = UnityEngine.Random.Range(0, laneCount),
                        beat = beat + step
                    });
                }

                if (UnityEngine.Random.value <= offBeatChance)
                {
                    beatmap.Add(new BeatNote
                    {
                        laneIndex = UnityEngine.Random.Range(0, laneCount),
                        beat = beat + step + 0.5f
                    });
                }
            }

            beat += 4f;
        }
    }
}
