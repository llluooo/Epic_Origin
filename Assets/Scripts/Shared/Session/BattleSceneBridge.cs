using System.Collections.Generic;
using UnityEngine.SceneManagement;

public enum BattleEncounterType
{
    None,
    ArmyCamp,
    EnemyStronghold,
    PlayerStronghold
}

public enum BattleOutcome
{
    PlayerVictory,
    EnemyVictory,
    Draw,
    PlayerSurrender,
    EnemySurrender
}

public static class BattleSceneBridge
{
    public const string MainSceneName = "MainScene";
    public const string BattleSceneName = "BattleScene";

    public static List<Card> PlayerDeck => GameSession.PendingBattle?.playerDeck;
    public static List<Card> EnemyDeck => GameSession.PendingBattle?.enemyDeck;
    public static BattleEncounterType EncounterType => GameSession.PendingBattle?.encounterType ?? BattleEncounterType.None;
    public static bool PlayerStartsAttacking => GameSession.PendingBattle?.playerStartsAttacking ?? true;

    public static bool HasBattleData => GameSession.HasPendingBattle &&
                                        PlayerDeck != null &&
                                        EnemyDeck != null;

    public static void LoadBattleScene(GameRunState runState, PendingBattleState battleState)
    {
        GameSession.StoreBattleLaunchState(runState, battleState);
        SceneManager.LoadScene(BattleSceneName);
    }

    public static void ResolveAndReturn(BattleOutcome outcome)
    {
        GameSession.SetPendingBattleResult(outcome);
        SceneManager.LoadScene(MainSceneName);
    }

    public static void Clear()
    {
        GameSession.ClearPendingBattleAfterResolution();
    }
}
