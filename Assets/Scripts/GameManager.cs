using UnityEngine;

/// <summary>
/// 游戏管理器（全局唯一）
/// 控制：回合、玩家/AI切换、游戏状态
/// </summary>
public class GameManager : MonoBehaviour
{
    // ================== 单例 ==================
    public static GameManager Instance;

    // ================== 回合数据 ==================
    public int currentTurn = 1;       // 当前回合数
    public bool isPlayerTurn = true;  // 是否玩家回合

    // ================== 游戏状态 ==================
    public enum GameState
    {
        Start,      // 游戏刚开始
        PlayerTurn, // 玩家回合
        AITurn,     // AI回合
        End         // 游戏结束
    }

    public GameState currentState;

    // ================== 初始化 ==================
    private void Awake()
    {
        // 单例初始化（保证全局唯一）
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 切场景不销毁
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
        isPlayerTurn = true;
        currentState = GameState.PlayerTurn;

        Debug.Log("游戏开始！");
        Debug.Log("当前回合: " + currentTurn);

        StartPlayerTurn();
    }

    // ================== 玩家回合 ==================

    void StartPlayerTurn()
    {
        currentState = GameState.PlayerTurn;
        Debug.Log("玩家回合开始");

        // 👉 后续在这里解锁玩家操作（移动/升级/召唤）
    }

    public void EndPlayerTurn()
    {
        Debug.Log("玩家回合结束");

        isPlayerTurn = false;
        StartAITurn();
    }

    // ================== AI回合 ==================

    void StartAITurn()
    {
        currentState = GameState.AITurn;
        Debug.Log("AI回合开始");

        // 👉 模拟AI行为（暂时用延迟代替）
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
        isPlayerTurn = true;

        Debug.Log("进入下一回合: " + currentTurn);

        StartPlayerTurn();
    }


    void Update()
    {
        // 按空格结束玩家回合（测试用）
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (currentState == GameState.PlayerTurn)
            {
                EndPlayerTurn();
            }
        }
    }
}