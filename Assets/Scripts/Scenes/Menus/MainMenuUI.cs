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

    [Header("设置面板")]
    public SettingsUI settingsUI;

    void Start()
    {
        AudioManager.Instance?.PlayBGM(BGM.MainMenu);

        newGameButton.onClick.AddListener(OnNewGame);
        loadGameButton.onClick.AddListener(OnLoadGame);
        settingsButton.onClick.AddListener(OnSettings);
        exitButton.onClick.AddListener(OnExit);

        loadGameButton.interactable = SaveSystem.HasAnySave();
    }

    void OnNewGame()
    {
        SceneManager.LoadScene("RaceSelectScene");
    }

    void OnLoadGame()
    {
        SceneManager.LoadScene("SaveSelectScene");
    }

    void OnSettings()
    {
        if (settingsUI != null)
            settingsUI.Open();
    }

    void OnExit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }


}
