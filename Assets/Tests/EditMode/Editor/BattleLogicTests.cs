using System.Collections.Generic;
using NUnit.Framework;

public class BattleLogicTests
{
    [Test]
    public void BattleCalculator_applies_race_modifier_to_attack_damage()
    {
        BattleCard human = new BattleCard(HumanUnit.CreateCard(0), 2);
        BattleCard heaven = new BattleCard(HeavenUnit.CreateCard(0), 2);

        Assert.AreEqual(1.2f, BattleCalculator.GetRaceModifier(RaceType.Human, RaceType.Heaven));
        Assert.AreEqual(9, BattleCalculator.CalculateAttackDamage(human, heaven));
    }

    [Test]
    public void BattleManager_reports_player_victory_when_enemy_side_is_defeated()
    {
        BattleManager manager = new BattleManager();
        List<Card> playerDeck = new List<Card> { CreateCard(RaceType.Human, 4, 3) };
        List<Card> enemyDeck = new List<Card> { CreateCard(RaceType.Heaven, 0, 1) };

        manager.Initialize(playerDeck, enemyDeck);
        manager.ExecuteRound(0, 0);

        Assert.IsTrue(manager.IsBattleOver());
        Assert.AreEqual(BattleOutcome.PlayerVictory, manager.outcome);
    }

    [Test]
    public void BattleManager_reports_surrendering_side()
    {
        BattleManager manager = new BattleManager();
        manager.Initialize(
            new List<Card> { HumanUnit.CreateCard(0) },
            new List<Card> { GhostUnit.CreateCard(0) });

        manager.Surrender(playerSide: true);

        Assert.IsTrue(manager.IsBattleOver());
        Assert.AreEqual(BattleOutcome.PlayerSurrender, manager.outcome);
    }

    [Test]
    public void BattleManager_keeps_up_to_six_distinct_battle_cards()
    {
        BattleManager manager = new BattleManager();
        List<Card> playerDeck = new List<Card>
        {
            CreateCard(RaceType.Human, 0, 1),
            CreateCard(RaceType.Human, 1, 1),
            CreateCard(RaceType.Human, 2, 1),
            CreateCard(RaceType.Human, 3, 1),
            CreateCard(RaceType.Human, 4, 1),
            CreateCard(RaceType.Heaven, 0, 1),
            CreateCard(RaceType.Heaven, 1, 1),
        };

        manager.Initialize(playerDeck, new List<Card> { CreateCard(RaceType.Ghost, 0, 1) });

        Assert.AreEqual(6, manager.playerCards.Count);
    }

    [Test]
    public void Card_clone_copies_values_without_reusing_reference()
    {
        Card original = CreateCard(RaceType.Ghost, 2, 7);
        original.currentHP = 13;

        Card clone = original.Clone();

        Assert.AreNotSame(original, clone);
        Assert.AreEqual(original.unitIndex, clone.unitIndex);
        Assert.AreEqual(original.level, clone.level);
        Assert.AreEqual(original.quantity, clone.quantity);
        Assert.AreEqual(original.currentHP, clone.currentHP);
        Assert.AreEqual(original.race, clone.race);
        Assert.AreEqual(original.cardName, clone.cardName);
    }

    private static Card CreateCard(RaceType race, int unitIndex, int quantity)
    {
        Card card = race switch
        {
            RaceType.Human => HumanUnit.CreateCard(unitIndex),
            RaceType.Heaven => HeavenUnit.CreateCard(unitIndex),
            RaceType.Ghost => GhostUnit.CreateCard(unitIndex),
            _ => HumanUnit.CreateCard(unitIndex),
        };

        card.quantity = quantity;
        return card;
    }
}
