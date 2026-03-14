using UnityEngine;

public static class CombatSessionState
{
    public static CombatDifficulty PendingDifficulty = CombatDifficulty.Easy;
    public static int PendingTargetPercent = 50;
    public static string PendingEnemyName = "Enemy Ship";

    public static int RollTargetPercent(CombatDifficulty difficulty)
    {
        switch (difficulty)
        {
            case CombatDifficulty.Easy:   return Random.Range(25, 51);
            case CombatDifficulty.Medium: return Random.Range(50, 71);
            case CombatDifficulty.Hard:   return Random.Range(70, 91);
            default: return 50;
        }
    }

    public static void PrepareEncounter(CombatDifficulty difficulty, string enemyName)
    {
        PendingDifficulty = difficulty;
        PendingTargetPercent = RollTargetPercent(difficulty);
        PendingEnemyName = string.IsNullOrWhiteSpace(enemyName) ? "Enemy Ship" : enemyName;
    }
}