using UnityEngine;

public class ResourceTile : POITile
{
    protected override void OnPOIEnter()
    {
        Player player = GetEnteringPlayer();
        if (player == null) return;

        bool isAI = player == GameManager.Instance.aiPlayer;
        bool giveGold = Random.value > 0.5f;
        int amount = Random.Range(40, 61);
        if (giveGold)
        {
            player.AddResources(new ResourceData(amount, 0));
            Debug.Log($"占领金矿！获得 {amount} 金币");
            MessageLogUI.Instance?.AddMessage($"{(isAI ? "AI " : "")}占领金矿！获得 {amount} 金币");

            if (!isAI && AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(SFX.GetGold);
        }
        else
        {
            player.AddResources(new ResourceData(0, amount));
            Debug.Log($"占领采石场！获得 {amount} 建材");
            MessageLogUI.Instance?.AddMessage($"{(isAI ? "AI " : "")}占领采石场！获得 {amount} 建材");

            if (!isAI && AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(SFX.GetWood);
        }

        player.capturedResourceCount++;
        Debug.Log($"资源点已占领，据点每回合产出 +20 金币 +20 建材（当前累计 +{player.capturedResourceCount * 20}）");
        MessageLogUI.Instance?.AddMessage($"资源点已占领，据点产出提升！");
    }

    private Player GetEnteringPlayer()
    {
        AIHero aiHero = Object.FindObjectOfType<AIHero>();
        if (aiHero != null && aiHero.currentGridPos == gridPosition)
        {
            return GameManager.Instance.aiPlayer;
        }
        return GameManager.Instance.player;
    }
}
