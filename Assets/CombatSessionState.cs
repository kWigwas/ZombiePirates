using UnityEngine;

public static class CombatSessionState
{
    public static CombatDifficulty CurrentDifficulty { get; private set; } = CombatDifficulty.Medium;
    public static int TargetPercent { get; private set; } = 60;
    public static string EncounterLabel { get; private set; } = "Enemy Ship";
    public static string ReturnSceneName { get; private set; } = string.Empty;
    public static bool HasEncounter { get; private set; }

    public static void BeginEncounter(CombatDifficulty difficulty, string encounterLabel, string returnSceneName)
    {
        CurrentDifficulty = difficulty;
        EncounterLabel = string.IsNullOrWhiteSpace(encounterLabel) ? "Enemy Ship" : encounterLabel;
        ReturnSceneName = returnSceneName;
        TargetPercent = GenerateTargetPercent(difficulty);
        HasEncounter = true;
    }

    public static void ClearEncounter()
    {
        HasEncounter = false;
    }

    private static int GenerateTargetPercent(CombatDifficulty difficulty)
    {
        switch (difficulty)
        {
            case CombatDifficulty.Easy:
                return Random.Range(25, 51);
            case CombatDifficulty.Medium:
                return Random.Range(50, 71);
            case CombatDifficulty.Hard:
                return Random.Range(70, 91);
            default:
                return 60;
        }
    }
}
