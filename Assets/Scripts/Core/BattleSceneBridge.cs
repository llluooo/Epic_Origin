using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum BattleEncounterType
{
    None,
    ArmyCamp,
    EnemyStronghold
}

public enum BattleOutcome
{
    PlayerVictory,
    EnemyVictory,
    Draw,
    PlayerSurrender,
    EnemySurrender
}

/// <summary>
/// 从地图场景向 BattleScene 传递战斗数据，并在战斗结束后回到主地图。
/// </summary>
public static class BattleSceneBridge
{
    public const string BattleSceneName = "BattleScene";

    public static List<Card> PlayerDeck;
    public static List<Card> EnemyDeck;
    public static BattleEncounterType EncounterType = BattleEncounterType.None;
    public static bool PlayerStartsAttacking = true;

    public static bool HasBattleData => PlayerDeck != null && EnemyDeck != null;

    public static void LoadBattleScene(
        List<Card> playerDeck,
        List<Card> enemyDeck,
        BattleEncounterType encounterType,
        bool playerStartsAttacking = true)
    {
        PlayerDeck = CloneCardList(playerDeck);
        EnemyDeck = CloneCardList(enemyDeck);
        EncounterType = encounterType;
        PlayerStartsAttacking = playerStartsAttacking;

        SceneManager.LoadScene(BattleSceneName, LoadSceneMode.Additive);
    }

    public static void ResolveAndReturn(BattleOutcome outcome)
    {
        BattleEncounterType encounterType = EncounterType;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResolveBattleResult(encounterType, outcome);
        }
        else
        {
            Debug.LogWarning("BattleSceneBridge: GameManager 不存在，无法结算战斗结果。");
        }

        Clear();

        if (SceneManager.GetSceneByName(BattleSceneName).isLoaded)
        {
            SceneManager.UnloadSceneAsync(BattleSceneName);
        }
    }

    public static void Clear()
    {
        PlayerDeck = null;
        EnemyDeck = null;
        EncounterType = BattleEncounterType.None;
        PlayerStartsAttacking = true;
    }

    private static List<Card> CloneCardList(List<Card> source)
    {
        if (source == null)
        {
            return null;
        }

        var list = new List<Card>(source.Count);
        foreach (Card card in source)
        {
            list.Add(card?.Clone());
        }

        return list;
    }
}
