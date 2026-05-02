using UnityEngine;

/// <summary>
/// 游戏管理器（核心控制类）
/// 负责：回合控制、玩家行为限制、状态管理
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // ================== 回合数据 ==================
    public int currentTurn = 1;           // 当前回合数
    public bool isPlayerTurn = true;      // 是否玩家回合

    // ================== 玩家行为控制 ==================
    public bool hasPlayerActed = false;   // 本回合玩家是否已经执行过主操作

    // ================== 游戏状态 ==================
    public enum GameState
    {
        PlayerTurn,
        AITurn,
        End
    }

    public GameState currentState;

    private void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        StartGame();
    }

    /// <summary>
    /// 开始游戏
    /// </summary>
    void StartGame()
    {
        currentTurn = 1;
        StartPlayerTurn();
    }

    // ================== 玩家回合 ==================

    void StartPlayerTurn()
    {
        currentState = GameState.PlayerTurn;
        isPlayerTurn = true;

        // 关键：重置玩家操作状态
        hasPlayerActed = false;

        Debug.Log("玩家回合开始");
    }

    /// <summary>
    /// 玩家执行主操作（移动/升级/召唤）时调用
    /// </summary>
    public void OnPlayerAction()
    {
        hasPlayerActed = true;
    }

    /// <summary>
    /// 玩家结束回合
    /// </summary>
    public void EndPlayerTurn()
    {
        if (!hasPlayerActed)
        {
            Debug.Log("你还没有执行操作！");
            return;
        }

        Debug.Log("玩家回合结束");

        StartAITurn();
    }

    // ================== AI回合 ==================

    void StartAITurn()
    {
        currentState = GameState.AITurn;
        isPlayerTurn = false;

        Debug.Log("AI回合开始");

        // 模拟AI行为
        Invoke(nameof(EndAITurn), 1.5f);
    }

    void EndAITurn()
    {
        Debug.Log("AI回合结束");

        NextTurn();
    }

    // ================== 回合推进 ==================

    void NextTurn()
    {
        currentTurn++;
        StartPlayerTurn();
    }
}