using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 开发者测试工具：AI 难度选择菜单
/// 在开发模式下显示按键提示，允许在游戏运行时切换 AI 难度
/// </summary>
public class AIDifficultySelector : MonoBehaviour
{
    public static AIDifficulty CurrentDifficulty { get; private set; } = AIDifficulty.Easy;

    private bool showMenu = false;
    private GUIStyle titleStyle;
    private GUIStyle buttonStyle;
    private GUIStyle infoStyle;
    private bool stylesInitialized = false;

    private void Awake()
    {
        // 确保单例存活
        if (FindObjectsOfType<AIDifficultySelector>().Length > 1)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        Debug.Log("AI 难度选择器已初始化。按 [Shift+F1] 打开难度菜单（开发者工具）");
    }

    private void Update()
    {
        // F1 打开/关闭菜单
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.F1))
        {
            showMenu = !showMenu;
        }
    }

    private void OnGUI()
    {
        if (!stylesInitialized)
        {
            InitializeStyles();
            stylesInitialized = true;
        }

        if (!showMenu) return;

        GUILayout.BeginArea(new Rect(10, 10, 400, 300));

        GUILayout.Label("=== AI 难度选择（开发者工具）===", titleStyle);
        GUILayout.Space(20);

        GUILayout.Label($"当前难度: {CurrentDifficulty}", infoStyle);
        GUILayout.Space(10);

        if (GUILayout.Button("简单 (Easy) - 随机移动，被动采集", buttonStyle))
        {
            SetDifficulty(AIDifficulty.Easy);
        }

        if (GUILayout.Button("困难 (Hard) - 主动寻路，策略行动", buttonStyle))
        {
            SetDifficulty(AIDifficulty.Hard);
        }

        GUILayout.Space(20);
        GUILayout.Label("Shift+F1 打开/关闭此菜单", infoStyle);
        GUILayout.Label("选择难度后立即生效（当前回合后生效）", infoStyle);

        if (GUILayout.Button("关闭", buttonStyle))
        {
            showMenu = false;
        }

        GUILayout.EndArea();
    }

    private void SetDifficulty(AIDifficulty difficulty)
    {
        CurrentDifficulty = difficulty;
        Debug.Log($"AI 难度已切换为: {difficulty}");
    }

    private void InitializeStyles()
    {
        titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 16,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };

        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 14,
            padding = new RectOffset(10, 10, 10, 10),
            fixedHeight = 50
        };

        infoStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 12,
            normal = { textColor = Color.yellow }
        };
    }
}
