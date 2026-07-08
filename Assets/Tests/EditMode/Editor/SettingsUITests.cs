using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUITests
{
    private GameObject canvasObject;

    [TearDown]
    public void TearDown()
    {
        AIDifficultySelector.SetDifficulty(AIDifficulty.Easy);

        if (canvasObject != null)
            UnityEngine.Object.DestroyImmediate(canvasObject);
    }

    [Test]
    public void RuntimeSettingsUI_opens_closes_and_updates_controls()
    {
        canvasObject = new GameObject("Canvas");
        canvasObject.AddComponent<Canvas>();

        SettingsUI settingsUI = SettingsUI.CreateRuntimeSettingsUI(canvasObject.transform);

        settingsUI.Open();
        Assert.IsTrue(settingsUI.panel.activeSelf);
        Assert.IsNotNull(settingsUI.volumeSlider);
        Assert.IsNotNull(settingsUI.volumeValueText);
        Assert.IsNotNull(settingsUI.difficultyButton);
        Assert.IsNotNull(settingsUI.difficultyLabel);
        Assert.IsNotNull(settingsUI.backButton);

        settingsUI.volumeSlider.value = 0.42f;
        Assert.AreEqual("42%", settingsUI.volumeValueText.text);

        AIDifficultySelector.SetDifficulty(AIDifficulty.Easy);
        settingsUI.difficultyButton.onClick.Invoke();
        Assert.AreEqual(AIDifficulty.Hard, AIDifficultySelector.CurrentDifficulty);
        Assert.AreEqual("AI难度: 困难", settingsUI.difficultyLabel.text);

        settingsUI.backButton.onClick.Invoke();
        Assert.IsFalse(settingsUI.panel.activeSelf);
    }
}
