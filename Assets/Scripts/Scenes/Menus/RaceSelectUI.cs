using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 种族选择界面。
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
        public RaceHotspotFeedback hotspotFeedback;
        public Sprite heroSprite;
        public Sprite heroFrameSprite;
        public Sprite textFrameSprite;
        public string displayName;

        [TextArea(4, 10)]
        public string storyDescription;
    }

    [Header("面板")]
    public GameObject mapPanel;
    public GameObject detailPanel;

    [Header("种族选项")]
    public RaceOption[] raceOptions;

    [Header("详情视图")]
    public Image detailHeroImage;
    public Image heroFrameImage;
    public Image textFrameImage;
    public TMP_Text detailTitleText;
    public TMP_Text detailBodyText;

    [Header("按钮")]
    public Button backButton;
    public Button confirmButton;

    [Header("地图反馈")]
    public float mapClickTransitionDelay = 0.22f;

    private const string MainMenuSceneName = "MainMenuScene";

    private int selectedIndex = -1;
    private bool isTransitioning;
    private Coroutine showRaceDetailCoroutine;

    void Start()
    {
        AudioManager.Instance?.PlayBGM(BGM.MainMenu);

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
                ResolveRaceOptionReferences(i);

                int index = i;
                if (raceOptions[i].hotspotButton != null)
                    raceOptions[i].hotspotButton.onClick.AddListener(() => OnHotspotClicked(index));

                SetHighlightVisible(i, false);
            }
        }

        ShowMapPanel();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && CanReturnToMainMenu())
            ReturnToMainMenu();
    }

    public void ShowMapPanel()
    {
        selectedIndex = -1;
        isTransitioning = false;
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

        isTransitioning = false;
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

        ApplyDetailFrame(heroFrameImage, option.heroFrameSprite);
        ApplyDetailFrame(textFrameImage, option.textFrameSprite);

        if (mapPanel != null)
            mapPanel.SetActive(false);

        if (detailPanel != null)
            detailPanel.SetActive(true);

        if (confirmButton != null)
            confirmButton.interactable = true;
    }

    void OnHotspotClicked(int index)
    {
        if (!IsValidOptionIndex(index) || isTransitioning)
            return;

        float delay = mapClickTransitionDelay;
        RaceHotspotFeedback feedback = raceOptions[index].hotspotFeedback;

        if (feedback == null && raceOptions[index].hotspotButton != null)
            feedback = raceOptions[index].hotspotButton.GetComponent<RaceHotspotFeedback>();

        if (feedback != null)
            delay = Mathf.Max(delay, feedback.PlayClickFeedback());

        if (delay <= 0f)
        {
            ShowRaceDetail(index);
            return;
        }

        showRaceDetailCoroutine = StartCoroutine(ShowRaceDetailAfterDelay(index, delay));
    }

    IEnumerator ShowRaceDetailAfterDelay(int index, float delay)
    {
        isTransitioning = true;
        yield return new WaitForSecondsRealtime(delay);
        showRaceDetailCoroutine = null;
        ShowRaceDetail(index);
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

    bool CanReturnToMainMenu()
    {
        return mapPanel != null
            && detailPanel != null
            && mapPanel.activeSelf
            && !detailPanel.activeSelf;
    }

    void ReturnToMainMenu()
    {
        if (showRaceDetailCoroutine != null)
        {
            StopCoroutine(showRaceDetailCoroutine);
            showRaceDetailCoroutine = null;
        }

        isTransitioning = false;
        SceneManager.LoadScene(MainMenuSceneName);
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
        ResolveRaceOptionReferences(index);

        if (!IsValidOptionIndex(index) || raceOptions[index].highlightImage == null)
            return;

        raceOptions[index].highlightImage.gameObject.SetActive(visible);
    }

    void ResolveRaceOptionReferences(int index)
    {
        if (!IsValidOptionIndex(index))
            return;

        RaceOption option = raceOptions[index];

        if (option.hotspotFeedback == null && option.hotspotButton != null)
            option.hotspotFeedback = option.hotspotButton.GetComponent<RaceHotspotFeedback>();

        if (option.highlightImage == null && option.hotspotFeedback != null)
            option.highlightImage = option.hotspotFeedback.HighlightGraphic as Image;
    }

    void ApplyDetailFrame(Image frameImage, Sprite frameSprite)
    {
        if (frameImage == null)
            return;

        frameImage.sprite = frameSprite;
        frameImage.enabled = frameSprite != null;
        frameImage.raycastTarget = false;
    }
}
