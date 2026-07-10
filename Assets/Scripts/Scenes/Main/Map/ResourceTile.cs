using UnityEngine;
public class ResourceTile : Tile
{
    public override void OnHeroEnter()
    {
        bool giveGold = Random.value > 0.5f;
        int amount = Random.Range(40, 61);
        if (giveGold)
        {
            GameManager.Instance.player.AddResources(new ResourceData(amount, 0));
            Debug.Log($"占领金矿！获得 {amount} 金币");
            MessageLogUI.Instance?.AddMessage($"占领金矿！获得 {amount} 金币");

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(SFX.GetGold);
        }
        else
        {
            GameManager.Instance.player.AddResources(new ResourceData(0, amount));
            Debug.Log($"占领采石场！获得 {amount} 建材");
            MessageLogUI.Instance?.AddMessage($"占领采石场！获得 {amount} 建材");

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(SFX.GetWood);
        }
    }
}