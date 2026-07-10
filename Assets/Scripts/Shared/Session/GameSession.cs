using UnityEngine;

public class GameSession : MonoBehaviour
{
    private static GameSession instance;

    private GameRunState runState;
    private PendingBattleState pendingBattle;
    private PendingBattleResult pendingBattleResult;

    // 据点兵力场景返回数据
    private Deck garrisonHeroDeck;
    private Deck garrisonGarrisonDeck;
    private RaceType garrisonPlayerRace;
    private bool garrisonShouldReopenStronghold;

    public static bool HasRunState => instance != null && instance.runState != null;
    public static bool HasPendingBattle => instance != null && instance.pendingBattle != null;
    public static bool HasPendingBattleResult => instance != null && instance.pendingBattleResult != null;
    public static bool HasGarrisonReturnState => instance != null && instance.garrisonHeroDeck != null;
    public static bool HasGarrisonShouldReopenStronghold
        => instance != null && instance.garrisonShouldReopenStronghold;

    public static bool HasGarrisonPlayerRace => instance != null;

    public static GameRunState RunState => instance != null ? instance.runState : null;
    public static PendingBattleState PendingBattle => instance != null ? instance.pendingBattle : null;
    public static PendingBattleResult PendingBattleResult => instance != null ? instance.pendingBattleResult : null;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static void StoreBattleLaunchState(GameRunState nextRunState, PendingBattleState nextPendingBattle)
    {
        GameSession session = EnsureExists();
        session.runState = nextRunState?.Clone();
        session.pendingBattle = nextPendingBattle?.Clone();
        session.pendingBattleResult = null;
    }

    public static void SetPendingBattleResult(BattleOutcome outcome)
    {
        GameSession session = EnsureExists();
        PendingBattleState battle = session.pendingBattle;
        session.pendingBattleResult = new PendingBattleResult
        {
            encounterType = battle != null ? battle.encounterType : BattleEncounterType.None,
            sourceTilePos = battle != null ? battle.sourceTilePos : Vector2Int.zero,
            outcome = outcome
        };
    }

    public static void UpdateRunState(GameRunState nextRunState)
    {
        GameSession session = EnsureExists();
        session.runState = nextRunState?.Clone();
    }

    public static void ClearPendingBattleAfterResolution()
    {
        if (instance == null)
        {
            return;
        }

        instance.pendingBattle = null;
        instance.pendingBattleResult = null;
    }

    public static void ClearAll()
    {
        if (instance == null)
        {
            return;
        }

        instance.runState = null;
        instance.pendingBattle = null;
        instance.pendingBattleResult = null;
        instance.garrisonHeroDeck = null;
        instance.garrisonGarrisonDeck = null;
        instance.garrisonPlayerRace = RaceType.Human;
        instance.garrisonShouldReopenStronghold = false;
    }

    public static void StoreGarrisonState(GameRunState nextRunState)
    {
        GameSession session = EnsureExists();
        session.runState = nextRunState?.Clone();

        // 同时存储兵力数据到专用字段，供 GarrisonUI 读取
        if (nextRunState?.player != null)
        {
            session.garrisonHeroDeck = GameRunState.CloneDeck(nextRunState.player.deck);
            session.garrisonGarrisonDeck = GameRunState.CloneDeck(nextRunState.player.garrisonDeck);
            session.garrisonPlayerRace = nextRunState.player.race;
        }
        else
        {
            session.garrisonHeroDeck = null;
            session.garrisonGarrisonDeck = null;
        }
    }

    public static void StoreGarrisonReturnData(Deck heroDeck, Deck garrisonDeck)
    {
        GameSession session = EnsureExists();
        session.garrisonHeroDeck = GameRunState.CloneDeck(heroDeck);
        session.garrisonGarrisonDeck = GameRunState.CloneDeck(garrisonDeck);
    }

    public static Deck GetGarrisonHeroDeck()
    {
        if (instance == null || instance.garrisonHeroDeck == null) return null;
        return GameRunState.CloneDeck(instance.garrisonHeroDeck);
    }

    public static Deck GetGarrisonGarrisonDeck()
    {
        if (instance == null || instance.garrisonGarrisonDeck == null) return null;
        return GameRunState.CloneDeck(instance.garrisonGarrisonDeck);
    }

    public static RaceType GetGarrisonPlayerRace()
    {
        if (instance == null) return RaceType.Human;
        return instance.garrisonPlayerRace;
    }

    public static void ClearGarrisonReturnData()
    {
        if (instance == null) return;
        instance.garrisonHeroDeck = null;
        instance.garrisonGarrisonDeck = null;
        instance.garrisonPlayerRace = RaceType.Human;
    }

    public static void SetGarrisonShouldReopenStronghold()
    {
        GameSession session = EnsureExists();
        session.garrisonShouldReopenStronghold = true;
    }

    public static void ClearGarrisonShouldReopenStronghold()
    {
        if (instance == null) return;
        instance.garrisonShouldReopenStronghold = false;
    }

    public static void ResetForTests()
    {
        if (instance == null)
        {
            return;
        }

        GameSession oldInstance = instance;
        instance = null;
        if (Application.isPlaying)
        {
            Destroy(oldInstance.gameObject);
        }
        else
        {
            DestroyImmediate(oldInstance.gameObject);
        }
    }

    private static GameSession EnsureExists()
    {
        if (instance != null)
        {
            return instance;
        }

        GameObject sessionObject = new GameObject("GameSession");
        instance = sessionObject.AddComponent<GameSession>();
        DontDestroyOnLoad(sessionObject);
        return instance;
    }
}
