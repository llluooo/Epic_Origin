using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 主菜单界面
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    public Button newGameButton;
    public Button loadGameButton;
    public Button settingsButton;
    public Button exitButton;

    void Start()
    {
        newGameButton.onClick.AddListener(OnNewGame);
        loadGameButton.onClick.AddListener(OnLoadGame);
        settingsButton.onClick.AddListener(OnSettings);
        exitButton.onClick.AddListener(OnExit);
    }

    void OnNewGame()
    {
        SceneManager.LoadScene("RaceSelectScene");
    }

    void OnLoadGame()
    {
        // 预留：载入存档
    }

    void OnSettings()
    {
        // 预留：设置
    }

    void OnExit()
    {
        Application.Quit();
    }
}
