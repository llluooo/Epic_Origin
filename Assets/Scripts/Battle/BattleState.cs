using System.Collections.Generic;

[System.Serializable]
public class BattleState
{
    public int currentRound;
    public bool isPlayerAttacking;
    public List<string> playerCardStates = new List<string>();
    public List<string> enemyCardStates = new List<string>();

    public override string ToString()
    {
        string p = string.Join(", ", playerCardStates);
        string e = string.Join(", ", enemyCardStates);
        return $"回合:{currentRound} 攻方:{(isPlayerAttacking?"玩家":"敌方")}\n玩家: {p}\n敌方: {e}";
    }
}
