using UnityEngine;
using TMPro;

/// <summary>
/// UI管理器
/// 负责显示回合信息、资源、按钮监听
/// </summary>
public class UIManager : MonoBehaviour
{
    public TMP_Text turnText;
    public TMP_Text stateText;
    public TMP_Text resourceText;

    void Update()
    {
        UpdateUI();
    }

    void UpdateUI()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null) return;

        // 回合信息
        turnText.text = $"回合: {gm.currentTurn}/{gm.maxTurn}";

        // 状态信息
        if (gm.currentState == GameManager.GameState.End)
        {
            stateText.text = "游戏结束";
        }
        else if (gm.isPlayerTurn)
        {
            stateText.text = "你的回合";
            if (gm.hasPlayerActed)
                stateText.text += " [已行动]";
        }
        else
        {
            stateText.text = "AI回合";
        }

        // 资源信息
        if (resourceText != null && gm.player != null)
        {
            Player p = gm.player;
            resourceText.text = $"金币:{p.resources.gold}  建材:{p.resources.buildingMaterials}  据点Lv{p.strongholdLevel}";
        }
    }

    /// <summary>
    /// 结束回合按钮监听
    /// </summary>
    public void OnEndTurnButton()
    {
        GameManager.Instance.EndPlayerTurn();
    }
}
