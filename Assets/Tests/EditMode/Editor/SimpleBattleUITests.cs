using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class SimpleBattleUITests
{
    [Test]
    public void RefreshUI保留手动摆放的卡牌位置()
    {
        BattleUI battleUI = new GameObject("BattleUI").AddComponent<BattleUI>();
        CardView playerView = CreateImageOnlyCardView("PlayerCard");
        CardView enemyView = CreateImageOnlyCardView("EnemyCard");

        RectTransform playerRect = playerView.transform as RectTransform;
        RectTransform enemyRect = enemyView.transform as RectTransform;
        playerRect.anchoredPosition = new Vector2(123f, -45f);
        enemyRect.anchoredPosition = new Vector2(-80f, 210f);

        battleUI.playerSlotViews = new[] { playerView };
        battleUI.enemySlotViews = new[] { enemyView };
        battleUI.playerSlotButtons = new[] { playerView.gameObject.AddComponent<Button>() };

        battleUI.battleManager = new BattleManager();
        battleUI.battleManager.Initialize(
            new List<Card> { HumanUnit.CreateCard(0) },
            new List<Card> { GhostUnit.CreateCard(0) });

        battleUI.RefreshUI();

        Assert.AreEqual(new Vector2(123f, -45f), playerRect.anchoredPosition);
        Assert.AreEqual(new Vector2(-80f, 210f), enemyRect.anchoredPosition);
    }

    private static CardView CreateImageOnlyCardView(string name)
    {
        GameObject cardObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CardView));
        CardView view = cardObject.GetComponent<CardView>();
        view.cardImage = cardObject.GetComponent<Image>();
        return view;
    }
}
