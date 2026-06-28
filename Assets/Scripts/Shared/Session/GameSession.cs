using UnityEngine;

public class GameSession : MonoBehaviour
{
    private static GameSession instance;

    private GameRunState runState;
    private PendingBattleState pendingBattle;
    private PendingBattleResult pendingBattleResult;

    public static bool HasRunState => instance != null && instance.runState != null;
    public static bool HasPendingBattle => instance != null && instance.pendingBattle != null;
    public static bool HasPendingBattleResult => instance != null && instance.pendingBattleResult != null;

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
