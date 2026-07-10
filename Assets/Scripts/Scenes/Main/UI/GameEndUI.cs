using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// 游戏结束结算面板。胜利和失败显示不同的背景和标题。
/// 面板由用户在 Unity 编辑器中创建和配置。
/// </summary>
public class GameEndUI : MonoBehaviour
{
    [Header("面板根节点")]
    public GameObject panel;

    [Header("背景")]
    public Image backgroundImage;
    public Sprite victoryBackground;
    public Sprite defeatBackground;

    [Header("文本")]
    public TMP_Text titleText;
    public TMP_Text detailText;

    [Header("按钮")]
    public Button returnToMainMenuButton;

    public bool IsOpen { get; private set; }

    void Awake()
    {
        if (panel != null)
            panel.SetActive(false);

        if (returnToMainMenuButton != null)
            returnToMainMenuButton.onClick.AddListener(OnReturnToMainMenu);
    }

    public void ShowVictory(string detail)
    {
        if (panel != null)
            panel.SetActive(true);

        if (backgroundImage != null && victoryBackground != null)
            backgroundImage.sprite = victoryBackground;

        if (titleText != null)
            titleText.text = "胜利！";

        if (detailText != null)
            detailText.text = detail;

        IsOpen = true;
        Debug.Log("游戏结束：胜利");
    }

    public void ShowDefeat(string detail)
    {
        if (panel != null)
            panel.SetActive(true);

        if (backgroundImage != null && defeatBackground != null)
            backgroundImage.sprite = defeatBackground;

        if (titleText != null)
            titleText.text = "失败！";

        if (detailText != null)
            detailText.text = detail;

        IsOpen = true;
        Debug.Log("游戏结束：失败");
    }

    public void Close()
    {
        if (panel != null)
            panel.SetActive(false);
        IsOpen = false;
    }

    void OnReturnToMainMenu()
    {
        Debug.Log("结算完成，返回主菜单");
        GameSession.ClearAll();
        SceneManager.LoadScene("MainMenuScene");
    }
}
