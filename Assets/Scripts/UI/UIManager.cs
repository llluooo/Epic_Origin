using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI管理器
/// 负责：显示回合信息、按钮交互
/// </summary>
public class UIManager : MonoBehaviour
{
    public Text turnText;        // 回合文本
    public Text stateText;       // 状态文本

    void Update()
    {
        UpdateUI();
    }

    /// <summary>
    /// 更新UI显示
    /// </summary>
    void UpdateUI()
    {
        // 显示回合数
        turnText.text = "回合: " + GameManager.Instance.currentTurn;

        // 显示当前状态
        if (GameManager.Instance.isPlayerTurn)
        {
            stateText.text = "玩家回合";

            if (GameManager.Instance.hasPlayerActed)
            {
                stateText.text += "（已行动）";
            }
        }
        else
        {
            stateText.text = "AI回合";
        }
    }

    /// <summary>
    /// 结束回合按钮点击事件
    /// </summary>
    public void OnEndTurnButton()
    {
        GameManager.Instance.EndPlayerTurn();
    }
}