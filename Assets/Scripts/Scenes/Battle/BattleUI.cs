using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 战斗界面只负责刷新手动摆放的卡牌图片和处理玩家选择。
/// </summary>
public class BattleUI : MonoBehaviour
{
    public BattleManager battleManager;

    [Header("手动摆放的卡牌")]
    public CardView[] playerSlotViews;
    public CardView[] enemySlotViews;
    public Button[] playerSlotButtons;

    [Header("回合控制")]
    public Button attackButton;
    public bool returnToMapWhenBattleEnds = true;
    public float returnToMapDelay = 0.8f;

    private int selectedPlayerIndex = -1;
    private bool resultSubmitted;

    void Start()
    {
        BindControls();
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (battleManager == null)
        {
            return;
        }

        ClampSelectedIndex();
        RefreshCardViews(battleManager.playerCards, playerSlotViews, true);
        RefreshCardViews(battleManager.enemyCards, enemySlotViews, false);
        UpdateControlState();
        TrySubmitBattleResult();
    }

    public void OnPlayerSlotClicked(int index)
    {
        if (battleManager == null || battleManager.IsBattleOver())
        {
            return;
        }

        if (!IsValidIndex(battleManager.playerCards, index))
        {
            return;
        }

        selectedPlayerIndex = index;
        RefreshUI();
    }

    public void OnAttackConfirm()
    {
        if (battleManager == null || battleManager.IsBattleOver())
        {
            return;
        }

        ExecuteSingleRound(usePlayerSelection: true);
        RefreshUI();
    }

    private void BindControls()
    {
        if (playerSlotButtons != null)
        {
            for (int i = 0; i < playerSlotButtons.Length; i++)
            {
                int index = i;
                Button button = playerSlotButtons[i];
                if (button == null)
                {
                    continue;
                }

                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => OnPlayerSlotClicked(index));
            }
        }

        if (attackButton != null)
        {
            attackButton.onClick.RemoveAllListeners();
            attackButton.onClick.AddListener(OnAttackConfirm);
        }
    }

    private void RefreshCardViews(List<BattleCard> cards, CardView[] views, bool playerSide)
    {
        if (cards == null || views == null)
        {
            return;
        }

        for (int i = 0; i < views.Length; i++)
        {
            CardView view = views[i];
            if (view == null)
            {
                continue;
            }

            bool hasCard = i < cards.Count && cards[i] != null && cards[i].IsAlive();
            view.gameObject.SetActive(hasCard);

            if (hasCard)
            {
                view.SetCard(cards[i]);
                view.SetSelected(playerSide && i == selectedPlayerIndex);
            }
            else
            {
                view.ClearCard();
            }
        }
    }

    private void ExecuteSingleRound(bool usePlayerSelection)
    {
        if (!TryBuildRoundSelection(usePlayerSelection, out int attackerIndex, out int defenderIndex))
        {
            Debug.LogWarning("战斗出牌失败：没有可用的攻击或防守卡牌。");
            return;
        }

        battleManager.ExecuteRound(attackerIndex, defenderIndex);
    }

    private bool TryBuildRoundSelection(bool usePlayerSelection, out int attackerIndex, out int defenderIndex)
    {
        attackerIndex = 0;
        defenderIndex = 0;

        if (battleManager.isPlayerAttacking)
        {
            attackerIndex = usePlayerSelection
                ? GetValidSelectedPlayerIndex()
                : BattleAI.ChooseAttackerIndex(battleManager.playerCards);

            if (!IsValidIndex(battleManager.playerCards, attackerIndex))
            {
                return false;
            }

            defenderIndex = BattleAI.ChooseDefenderIndex(battleManager.enemyCards, battleManager.playerCards[attackerIndex]);
            return IsValidIndex(battleManager.enemyCards, defenderIndex);
        }

        attackerIndex = BattleAI.ChooseAttackerIndex(battleManager.enemyCards);
        if (!IsValidIndex(battleManager.enemyCards, attackerIndex))
        {
            return false;
        }

        defenderIndex = usePlayerSelection
            ? GetValidSelectedPlayerIndex()
            : BattleAI.ChooseDefenderIndex(battleManager.playerCards, battleManager.enemyCards[attackerIndex]);

        return IsValidIndex(battleManager.playerCards, defenderIndex);
    }

    private int GetValidSelectedPlayerIndex()
    {
        if (IsValidIndex(battleManager.playerCards, selectedPlayerIndex))
        {
            return selectedPlayerIndex;
        }

        return BattleAI.ChooseAttackerIndex(battleManager.playerCards);
    }

    private bool IsValidIndex(List<BattleCard> cards, int index)
    {
        return cards != null &&
               index >= 0 &&
               index < cards.Count &&
               cards[index] != null &&
               cards[index].IsAlive();
    }

    private void ClampSelectedIndex()
    {
        if (battleManager == null || selectedPlayerIndex < 0 || IsValidIndex(battleManager.playerCards, selectedPlayerIndex))
        {
            return;
        }

        selectedPlayerIndex = BattleAI.ChooseAttackerIndex(battleManager.playerCards);
    }

    private void UpdateControlState()
    {
        if (attackButton != null)
        {
            bool hasValidSelection = IsValidIndex(battleManager.playerCards, selectedPlayerIndex);
            attackButton.interactable = battleManager != null && !battleManager.IsBattleOver() && hasValidSelection;
            attackButton.gameObject.SetActive(hasValidSelection);
        }
    }

    private void TrySubmitBattleResult()
    {
        if (resultSubmitted || battleManager == null || !battleManager.IsBattleOver())
        {
            return;
        }

        resultSubmitted = true;
        UpdateControlState();

        if (returnToMapWhenBattleEnds)
        {
            StartCoroutine(ReturnToMapAfterDelay());
        }
    }

    private IEnumerator ReturnToMapAfterDelay()
    {
        yield return new WaitForSeconds(Mathf.Max(0f, returnToMapDelay));
        BattleSceneBridge.ResolveAndReturn(battleManager.outcome);
    }
}
