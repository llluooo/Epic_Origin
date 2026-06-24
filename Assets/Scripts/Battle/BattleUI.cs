using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 负责将 BattleManager 与 UI 连接。
/// </summary>
public class BattleUI : MonoBehaviour
{
    public BattleManager battleManager;

    [Header("Slots")]
    public Button[] playerSlotButtons;
    public TMP_Text[] playerSlotTexts;
    public TMP_Text[] enemySlotTexts;
    public CardView[] playerSlotViews;
    public CardView[] enemySlotViews;

    [Header("Phase Display")]
    public GameObject playerSlotGroup;
    public GameObject enemySlotGroup;
    public bool hideInactiveSideCards = false;

    [Header("Controls")]
    public TMP_Text turnText;
    public Button attackButton;
    public Toggle autoPlayToggle;
    public Button surrenderButton;
    public float returnToMapDelay = 0.8f;

    [Header("AI Surrender")]
    public int enemySurrenderMinRound = 4;
    [Range(0.01f, 1f)] public float enemyCriticalPowerRatio = 0.12f;
    [Range(0.01f, 1f)] public float enemyOpponentAdvantageRatio = 0.35f;
    [Range(0f, 1f)] public float enemyMaxSurrenderChance = 0.35f;

    private int selectedPlayerIndex = 0;
    private bool autoPlaying = false;
    private bool resultSubmitted = false;
    private Coroutine autoPlayCoroutine;

    void Start()
    {
        for (int i = 0; i < playerSlotButtons.Length; i++)
        {
            int idx = i;
            if (playerSlotButtons[i] != null)
            {
                playerSlotButtons[i].onClick.RemoveAllListeners();
                playerSlotButtons[i].onClick.AddListener(() => OnPlayerSlotClicked(idx));
            }
        }

        if (attackButton != null)
        {
            attackButton.onClick.RemoveAllListeners();
            attackButton.onClick.AddListener(OnAttackConfirm);
        }

        if (surrenderButton != null)
        {
            surrenderButton.onClick.RemoveAllListeners();
            surrenderButton.onClick.AddListener(OnSurrender);

            TMP_Text label = surrenderButton.GetComponentInChildren<TMP_Text>();
            if (label != null)
            {
                label.text = "投降";
            }
        }

        if (autoPlayToggle != null)
        {
            autoPlayToggle.onValueChanged.RemoveAllListeners();
            autoPlayToggle.onValueChanged.AddListener(OnAutoPlayToggle);
            autoPlayToggle.SetIsOnWithoutNotify(false);
            autoPlaying = false;
        }

        RefreshUI();
    }

    void Update()
    {
        if (turnText != null && battleManager != null)
        {
            turnText.text = battleManager.IsBattleOver()
                ? battleManager.battleResult
                : battleManager.GetBattleStatus();
        }

        if (attackButton != null && battleManager != null)
        {
            TMP_Text label = attackButton.GetComponentInChildren<TMP_Text>();
            if (label != null)
            {
                label.text = battleManager.isPlayerAttacking ? "确认出牌" : "确认应战";
            }
        }
    }

    public void RefreshUI()
    {
        if (battleManager == null)
        {
            return;
        }

        for (int i = 0; i < playerSlotTexts.Length; i++)
        {
            playerSlotTexts[i].text = i < battleManager.playerCards.Count && battleManager.playerCards[i] != null
                ? battleManager.playerCards[i].ToString()
                : "(空)";
        }

        for (int i = 0; i < playerSlotViews.Length; i++)
        {
            if (i < battleManager.playerCards.Count && battleManager.playerCards[i] != null)
            {
                playerSlotViews[i].SetCard(battleManager.playerCards[i]);
            }
            else if (playerSlotViews[i] != null)
            {
                playerSlotViews[i].ClearCard();
            }

            if (playerSlotViews[i] != null)
            {
                bool isSelected = i == selectedPlayerIndex &&
                                  i < battleManager.playerCards.Count &&
                                  battleManager.playerCards[i] != null &&
                                  battleManager.playerCards[i].IsAlive();
                playerSlotViews[i].SetSelected(isSelected);
            }
        }

        for (int i = 0; i < enemySlotTexts.Length; i++)
        {
            enemySlotTexts[i].text = i < battleManager.enemyCards.Count && battleManager.enemyCards[i] != null
                ? battleManager.enemyCards[i].ToString()
                : "(空)";
        }

        for (int i = 0; i < enemySlotViews.Length; i++)
        {
            if (i < battleManager.enemyCards.Count && battleManager.enemyCards[i] != null)
            {
                enemySlotViews[i].SetCard(battleManager.enemyCards[i]);
            }
            else if (enemySlotViews[i] != null)
            {
                enemySlotViews[i].ClearCard();
            }
        }

        UpdatePhaseDisplay();
        TrySubmitBattleResult();
    }

    private void UpdatePhaseDisplay()
    {
        if (battleManager == null || !hideInactiveSideCards)
        {
            return;
        }

        if (playerSlotGroup != null)
        {
            playerSlotGroup.SetActive(battleManager.isPlayerAttacking);
        }

        if (enemySlotGroup != null)
        {
            enemySlotGroup.SetActive(!battleManager.isPlayerAttacking);
        }
    }

    public void OnPlayerSlotClicked(int index)
    {
        selectedPlayerIndex = index;
        Debug.Log($"选择玩家卡槽 {index}");
        RefreshUI();
    }

    public void OnAttackConfirm()
    {
        if (battleManager == null || battleManager.IsBattleOver())
        {
            return;
        }

        if (battleManager.isPlayerAttacking)
        {
            int attackerIndex = selectedPlayerIndex;
            if (attackerIndex < 0 || attackerIndex >= battleManager.playerCards.Count || !battleManager.playerCards[attackerIndex].IsAlive())
            {
                attackerIndex = BattleAI.ChooseAttackerIndex(battleManager.playerCards);
            }

            int defenderIndex = BattleAI.ChooseDefenderIndex(battleManager.enemyCards, battleManager.playerCards[attackerIndex]);
            battleManager.ExecuteRound(attackerIndex, defenderIndex);
        }
        else
        {
            int attackerIndex = BattleAI.ChooseAttackerIndex(battleManager.enemyCards);
            int defenderIndex = selectedPlayerIndex;
            if (defenderIndex < 0 || defenderIndex >= battleManager.playerCards.Count || !battleManager.playerCards[defenderIndex].IsAlive())
            {
                defenderIndex = BattleAI.ChooseDefenderIndex(battleManager.playerCards, battleManager.enemyCards[attackerIndex]);
            }

            battleManager.ExecuteRound(attackerIndex, defenderIndex);
        }

        TryEnemySurrender();
        RefreshUI();
        TrySubmitBattleResult();
    }

    public void OnAutoPlayToggle(bool on)
    {
        autoPlaying = on;
        if (autoPlaying)
        {
            if (autoPlayCoroutine == null)
            {
                autoPlayCoroutine = StartCoroutine(AutoPlayRoutine());
            }
        }
        else if (autoPlayCoroutine != null)
        {
            StopCoroutine(autoPlayCoroutine);
            autoPlayCoroutine = null;
        }
    }

    private IEnumerator AutoPlayRoutine()
    {
        while (battleManager != null && !battleManager.IsBattleOver())
        {
            if (battleManager.isPlayerAttacking)
            {
                int attackerIndex = BattleAI.ChooseAttackerIndex(battleManager.playerCards);
                int defenderIndex = BattleAI.ChooseDefenderIndex(battleManager.enemyCards, battleManager.playerCards[attackerIndex]);
                battleManager.ExecuteRound(attackerIndex, defenderIndex);
            }
            else
            {
                int attackerIndex = BattleAI.ChooseAttackerIndex(battleManager.enemyCards);
                int defenderIndex = BattleAI.ChooseDefenderIndex(battleManager.playerCards, battleManager.enemyCards[attackerIndex]);
                battleManager.ExecuteRound(attackerIndex, defenderIndex);
            }

            TryEnemySurrender();
            RefreshUI();
            TrySubmitBattleResult();
            yield return new WaitForSeconds(0.4f);
        }

        autoPlayCoroutine = null;
    }

    public void OnSurrender()
    {
        if (battleManager == null)
        {
            return;
        }

        battleManager.Surrender(playerSide: true);
        RefreshUI();
        TrySubmitBattleResult();
    }

    private void TryEnemySurrender()
    {
        if (battleManager == null || battleManager.IsBattleOver())
        {
            return;
        }

        BattleAI.SurrenderDecision decision = BattleAI.EvaluateSurrender(
            battleManager.enemyCards,
            battleManager.playerCards,
            battleManager.currentRound,
            enemySurrenderMinRound,
            enemyCriticalPowerRatio,
            enemyOpponentAdvantageRatio,
            enemyMaxSurrenderChance);

        if (!decision.canSurrender)
        {
            return;
        }

        float roll = Random.value;
        Debug.Log($"敌方投降评估：{decision.currentPower}/{decision.initialPower} vs 玩家{decision.opponentPower}，概率 {decision.chance:0.00}，掷点 {roll:0.00}");

        if (roll <= decision.chance)
        {
            battleManager.Surrender(playerSide: false);
        }
    }

    private void TrySubmitBattleResult()
    {
        if (resultSubmitted || battleManager == null || !battleManager.IsBattleOver())
        {
            return;
        }

        resultSubmitted = true;
        if (attackButton != null)
        {
            attackButton.interactable = false;
        }

        if (surrenderButton != null)
        {
            surrenderButton.interactable = false;
        }

        if (autoPlayToggle != null)
        {
            autoPlayToggle.SetIsOnWithoutNotify(false);
            autoPlayToggle.interactable = false;
        }

        StartCoroutine(ReturnToMapAfterDelay());
    }

    private IEnumerator ReturnToMapAfterDelay()
    {
        yield return new WaitForSeconds(Mathf.Max(0f, returnToMapDelay));
        BattleSceneBridge.ResolveAndReturn(battleManager.outcome);
    }
}
