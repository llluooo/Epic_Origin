using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Race selection screen.
/// </summary>
public class RaceSelectUI : MonoBehaviour
{
    [System.Serializable]
    public class RaceOption
    {
        public RaceType race;

        [FormerlySerializedAs("selectButton")]
        public Button hotspotButton;

        public Image highlightImage;
        public Sprite heroSprite;
        public string displayName;

        [TextArea(4, 10)]
        public string storyDescription;
    }

    [Header("Panels")]
    public GameObject mapPanel;
    public GameObject detailPanel;

    [Header("Race Options")]
    public RaceOption[] raceOptions;

    [Header("Detail View")]
    public Image detailHeroImage;
    public TMP_Text detailTitleText;
    public TMP_Text detailBodyText;

    [Header("Buttons")]
    public Button backButton;
    public Button confirmButton;

    private int selectedIndex = -1;

    void Start()
    {
        if (confirmButton != null)
        {
            confirmButton.interactable = false;
            confirmButton.onClick.AddListener(OnConfirm);
        }

        if (backButton != null)
            backButton.onClick.AddListener(ShowMapPanel);

        if (raceOptions != null)
        {
            for (int i = 0; i < raceOptions.Length; i++)
            {
                int index = i;
                if (raceOptions[i].hotspotButton != null)
                    raceOptions[i].hotspotButton.onClick.AddListener(() => ShowRaceDetail(index));

                SetHighlightVisible(i, false);
            }
        }

        ShowMapPanel();
    }

    public void ShowMapPanel()
    {
        selectedIndex = -1;
        SetAllHighlightsHidden();

        if (mapPanel != null)
            mapPanel.SetActive(true);

        if (detailPanel != null)
            detailPanel.SetActive(false);

        if (confirmButton != null)
            confirmButton.interactable = false;
    }

    public void ShowRaceDetail(int index)
    {
        if (!IsValidOptionIndex(index))
            return;

        selectedIndex = index;
        SetAllHighlightsHidden();
        SetHighlightVisible(index, true);

        RaceOption option = raceOptions[index];

        if (detailTitleText != null)
            detailTitleText.text = string.IsNullOrWhiteSpace(option.displayName) ? option.race.ToString() : option.displayName;

        if (detailBodyText != null)
            detailBodyText.text = option.storyDescription ?? string.Empty;

        if (detailHeroImage != null)
        {
            detailHeroImage.sprite = option.heroSprite;
            detailHeroImage.enabled = option.heroSprite != null;
            detailHeroImage.preserveAspect = true;
        }

        if (mapPanel != null)
            mapPanel.SetActive(false);

        if (detailPanel != null)
            detailPanel.SetActive(true);

        if (confirmButton != null)
            confirmButton.interactable = true;
    }

    void OnConfirm()
    {
        if (!IsValidOptionIndex(selectedIndex))
            return;

        RaceType playerRace = raceOptions[selectedIndex].race;

        var remaining = new System.Collections.Generic.List<RaceType>();
        foreach (RaceType race in System.Enum.GetValues(typeof(RaceType)))
        {
            if (race != playerRace)
                remaining.Add(race);
        }

        RaceType enemyRace = remaining[Random.Range(0, remaining.Count)];

        GameSetupData.PlayerRace = playerRace;
        GameSetupData.EnemyRace = enemyRace;
        GameSetupData.IsNewGame = true;

        SceneManager.LoadScene("MainScene");
    }

    bool IsValidOptionIndex(int index)
    {
        return raceOptions != null && index >= 0 && index < raceOptions.Length;
    }

    void SetAllHighlightsHidden()
    {
        if (raceOptions == null)
            return;

        for (int i = 0; i < raceOptions.Length; i++)
            SetHighlightVisible(i, false);
    }

    void SetHighlightVisible(int index, bool visible)
    {
        if (!IsValidOptionIndex(index) || raceOptions[index].highlightImage == null)
            return;

        raceOptions[index].highlightImage.gameObject.SetActive(visible);
    }
}
