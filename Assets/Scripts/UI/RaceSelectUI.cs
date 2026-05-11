using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 种族选择界面
/// </summary>
public class RaceSelectUI : MonoBehaviour
{
    [System.Serializable]
    public class RaceOption
    {
        public RaceType race;
        public Button selectButton;
        public Image highlightImage;
    }

    public RaceOption[] raceOptions;
    public Button confirmButton;

    private int selectedIndex = -1;

    void Start()
    {
        confirmButton.interactable = false;
        confirmButton.onClick.AddListener(OnConfirm);

        for (int i = 0; i < raceOptions.Length; i++)
        {
            int index = i;
            raceOptions[i].selectButton.onClick.AddListener(() => OnSelectRace(index));
            raceOptions[i].highlightImage.gameObject.SetActive(false);
        }
    }

    void OnSelectRace(int index)
    {
        if (selectedIndex == index)
        {
            // 取消选中
            raceOptions[index].highlightImage.gameObject.SetActive(false);
            selectedIndex = -1;
            confirmButton.interactable = false;
        }
        else
        {
            // 切换选中
            if (selectedIndex >= 0)
                raceOptions[selectedIndex].highlightImage.gameObject.SetActive(false);

            raceOptions[index].highlightImage.gameObject.SetActive(true);
            selectedIndex = index;
            confirmButton.interactable = true;
        }
    }

    void OnConfirm()
    {
        if (selectedIndex < 0 || selectedIndex >= raceOptions.Length) return;

        RaceType playerRace = raceOptions[selectedIndex].race;

        // 从剩余种族中随机选一个给敌人
        var remaining = new System.Collections.Generic.List<RaceType>();
        foreach (RaceType r in System.Enum.GetValues(typeof(RaceType)))
            if (r != playerRace) remaining.Add(r);
        RaceType enemyRace = remaining[Random.Range(0, remaining.Count)];

        GameSetupData.PlayerRace = playerRace;
        GameSetupData.EnemyRace = enemyRace;
        GameSetupData.IsNewGame = true;

        SceneManager.LoadScene("MainScene");
    }
}
