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

    [Header("动画")]
    public float gatherDuration = 0.6f;
    public float arenaMoveDuration = 0.35f;
    public float resetStayDuration = 1f;
    public float resetFadeDuration = 0.5f;
    public float resetMoveDuration = 0.35f;

    [Header("动画锚点")]
    public Transform playerPlaceAnchor;
    public Transform arenaLeftAnchor;
    public Transform arenaRightAnchor;

    private int selectedPlayerIndex = -1;
    private bool resultSubmitted;
    private bool isAnimating;

    void Start()
    {
        LoadCardBackSprites();
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
        if (battleManager == null || battleManager.IsBattleOver() || isAnimating)
        {
            return;
        }

        // 确认应战后按钮立即消失
        if (attackButton != null)
        {
            attackButton.gameObject.SetActive(false);
        }

        StartCoroutine(PlayEngageAnimation());
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

    /// <summary>
    /// 交战动画序列完整流程
    /// </summary>
    private IEnumerator PlayEngageAnimation()
    {
        isAnimating = true;

        // 预选攻守双方
        if (!TryBuildRoundSelection(true, out int attackerIndex, out int defenderIndex))
        {
            Debug.LogWarning("战斗出牌失败：没有可用的攻击或防守卡牌。");
            isAnimating = false;
            RefreshUI();
            yield break;
        }

        // 根据当前攻方确定玩家/敌方卡牌索引和视图
        // attackerIndex/defenderIndex 语义：
        //   玩家回合：attacker=玩家出牌, defender=敌方应战
        //   敌方回合：attacker=敌方出牌, defender=玩家应战
        int playerIndex, enemyIndex;
        CardView playerView, enemyView;

        if (battleManager.isPlayerAttacking)
        {
            playerIndex = attackerIndex;
            enemyIndex = defenderIndex;
            playerView = playerSlotViews[attackerIndex];
            enemyView = enemySlotViews[defenderIndex];
        }
        else
        {
            playerIndex = defenderIndex;
            enemyIndex = attackerIndex;
            playerView = playerSlotViews[defenderIndex];
            enemyView = enemySlotViews[attackerIndex];
        }

        float maxFlipDuration = 0.5f;

        // ② 所有敌方存活卡牌同时翻到背面
        List<CardView> aliveEnemyViews = new List<CardView>();
        foreach (CardView view in enemySlotViews)
        {
            if (view != null && view.gameObject.activeSelf)
            {
                view.FlipToBack();
                aliveEnemyViews.Add(view);
                if (view.flipDuration > maxFlipDuration) maxFlipDuration = view.flipDuration;
            }
        }
        yield return new WaitForSeconds(maxFlipDuration);

        // ③ 敌方卡牌聚合到区域中央
        if (aliveEnemyViews.Count > 1)
        {
            Vector3 centerPos = CalculateEnemyCenter(aliveEnemyViews);
            Vector3[] startPositions = new Vector3[aliveEnemyViews.Count];
            for (int i = 0; i < aliveEnemyViews.Count; i++)
            {
                startPositions[i] = aliveEnemyViews[i].transform.localPosition;
            }

            float elapsed = 0f;
            while (elapsed < gatherDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / gatherDuration);
                for (int i = 0; i < aliveEnemyViews.Count; i++)
                {
                    aliveEnemyViews[i].transform.localPosition = Vector3.Lerp(startPositions[i], centerPos, t);
                }
                yield return null;
            }

            foreach (CardView view in aliveEnemyViews)
            {
                view.transform.localPosition = centerPos;
            }
        }

        // ④ 隐藏非选中卡牌（敌我双方）
        HideNonSelectedCards(playerIndex, enemyIndex);

        // ⑤ 我方选中卡牌移至出牌区（屏幕底侧中央）
        if (playerPlaceAnchor != null && playerView != null)
        {
            yield return MoveViewToPosition(playerView, playerPlaceAnchor.position, arenaMoveDuration);
        }

        // ⑥ 我方选中卡牌翻到背面
        if (playerView != null)
        {
            playerView.FlipToBack();
            yield return new WaitForSeconds(playerView.flipDuration);
        }

        // ⑦ 双方移至对峙区（敌方左，我方右）
        if (arenaLeftAnchor != null && arenaRightAnchor != null &&
            enemyView != null && playerView != null)
        {
            Vector3 enemyStart = enemyView.transform.position;
            Vector3 enemyTarget = arenaLeftAnchor.position;
            Vector3 playerStart = playerView.transform.position;
            Vector3 playerTarget = arenaRightAnchor.position;

            float elapsed = 0f;
            while (elapsed < arenaMoveDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / arenaMoveDuration);
                enemyView.transform.position = Vector3.Lerp(enemyStart, enemyTarget, t);
                playerView.transform.position = Vector3.Lerp(playerStart, playerTarget, t);
                yield return null;
            }

            enemyView.transform.position = enemyTarget;
            playerView.transform.position = playerTarget;
        }

        // ⑧ 双方同时翻到正面
        if (playerView != null)
        {
            playerView.FlipToFront();
        }
        if (enemyView != null)
        {
            enemyView.FlipToFront();
        }
        yield return new WaitForSeconds(maxFlipDuration);

        // ⑨ 执行回合结算（使用数据层索引）
        battleManager.ExecuteRound(attackerIndex, defenderIndex);

        // ⑩-⑮ 重置动画序列
        yield return StartCoroutine(ResetSequence(playerView, enemyView,
            playerIndex, enemyIndex, maxFlipDuration));

        isAnimating = false;
        RefreshUI();
    }

    private void HideNonSelectedCards(int playerSelectedIndex, int enemySelectedIndex)
    {
        for (int i = 0; i < playerSlotViews.Length; i++)
        {
            if (i != playerSelectedIndex && playerSlotViews[i] != null)
            {
                playerSlotViews[i].gameObject.SetActive(false);
            }
        }

        for (int i = 0; i < enemySlotViews.Length; i++)
        {
            if (i != enemySelectedIndex && enemySlotViews[i] != null)
            {
                enemySlotViews[i].gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 重置动画：对峙停留 → 归位 → 存活/死亡分支 → 其余淡入 → 敌方翻面
    /// </summary>
    private IEnumerator ResetSequence(CardView playerView, CardView enemyView,
        int playerDataIndex, int enemyDataIndex, float flipDuration)
    {
        // ⑩ 对峙停留
        yield return new WaitForSeconds(resetStayDuration);

        // ⑪ 双方滑回区域中央
        Vector3 enemyCenter = CalculateEnemySlotCenter();
        Vector3 playerZone = playerPlaceAnchor != null ? playerPlaceAnchor.localPosition : Vector3.zero;

        if (playerView != null && enemyView != null)
        {
            Vector3 eStart = enemyView.transform.localPosition;
            Vector3 pStart = playerView.transform.localPosition;
            float elapsed = 0f;
            while (elapsed < resetMoveDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / resetMoveDuration);
                enemyView.transform.localPosition = Vector3.Lerp(eStart, enemyCenter, t);
                playerView.transform.localPosition = Vector3.Lerp(pStart, playerZone, t);
                yield return null;
            }
            enemyView.transform.localPosition = enemyCenter;
            playerView.transform.localPosition = playerZone;
        }

        // ⑫ 存活→归位原始槽位 / 死亡→淡出消失
        bool playerAlive = IsValidIndex(battleManager.playerCards, playerDataIndex);
        bool enemyAlive = IsValidIndex(battleManager.enemyCards, enemyDataIndex);
        float branchMax = Mathf.Max(resetMoveDuration, resetFadeDuration);

        Vector3 pSlideStart = Vector3.zero, pSlideTarget = Vector3.zero;
        Vector3 eSlideStart = Vector3.zero, eSlideTarget = Vector3.zero;
        CanvasGroup pCG = null, eCG = null;
        float pFadeStart = 1f, eFadeStart = 1f;

        if (playerView != null)
        {
            pCG = playerView.GetComponent<CanvasGroup>();
            if (playerAlive)
            {
                pSlideStart = playerView.transform.localPosition;
                pSlideTarget = playerView.OriginalLocalPosition;
            }
            else if (pCG != null) pFadeStart = pCG.alpha;
        }

        if (enemyView != null)
        {
            eCG = enemyView.GetComponent<CanvasGroup>();
            if (enemyAlive)
            {
                eSlideStart = enemyView.transform.localPosition;
                eSlideTarget = enemyView.OriginalLocalPosition;
            }
            else if (eCG != null) eFadeStart = eCG.alpha;
        }

        float branchElapsed = 0f;
        while (branchElapsed < branchMax)
        {
            branchElapsed += Time.deltaTime;

            if (playerView != null)
            {
                if (playerAlive)
                {
                    float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(branchElapsed / resetMoveDuration));
                    playerView.transform.localPosition = Vector3.Lerp(pSlideStart, pSlideTarget, t);
                }
                else if (pCG != null)
                {
                    float t = Mathf.Clamp01(branchElapsed / resetFadeDuration);
                    pCG.alpha = Mathf.Lerp(pFadeStart, 0f, t);
                }
            }

            if (enemyView != null)
            {
                if (enemyAlive)
                {
                    float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(branchElapsed / resetMoveDuration));
                    enemyView.transform.localPosition = Vector3.Lerp(eSlideStart, eSlideTarget, t);
                }
                else if (eCG != null)
                {
                    float t = Mathf.Clamp01(branchElapsed / resetFadeDuration);
                    eCG.alpha = Mathf.Lerp(eFadeStart, 0f, t);
                }
            }

            yield return null;
        }

        // 确保最终状态
        if (playerView != null)
        {
            if (playerAlive) playerView.transform.localPosition = pSlideTarget;
            else { playerView.SetAlpha(0f); playerView.gameObject.SetActive(false); }
        }
        if (enemyView != null)
        {
            if (enemyAlive) enemyView.transform.localPosition = eSlideTarget;
            else { enemyView.SetAlpha(0f); enemyView.gameObject.SetActive(false); }
        }

        // ⑬ 其余卡牌淡入（敌方保持背面，我方正面）
        yield return StartCoroutine(FadeInOtherCards(playerDataIndex, enemyDataIndex));

        // ⑭ 敌方卡牌翻回正面
        foreach (CardView view in enemySlotViews)
        {
            if (view != null && view.gameObject.activeSelf && !view.IsShowingFront())
            {
                view.FlipToFront();
            }
        }
        yield return new WaitForSeconds(flipDuration);

        // ⑮ 重置选择
        selectedPlayerIndex = -1;
    }

    private IEnumerator FadeInOtherCards(int playerAttackerIndex, int enemyDefenderIndex)
    {
        List<CardView> toFadeIn = new List<CardView>();

        for (int i = 0; i < playerSlotViews.Length; i++)
        {
            if (i != playerAttackerIndex && playerSlotViews[i] != null &&
                IsValidIndex(battleManager.playerCards, i))
            {
                CardView view = playerSlotViews[i];
                view.transform.localPosition = view.OriginalLocalPosition;
                view.SetAlpha(0f);
                view.gameObject.SetActive(true);
                toFadeIn.Add(view);
            }
        }

        for (int i = 0; i < enemySlotViews.Length; i++)
        {
            if (i != enemyDefenderIndex && enemySlotViews[i] != null &&
                IsValidIndex(battleManager.enemyCards, i))
            {
                CardView view = enemySlotViews[i];
                view.transform.localPosition = view.OriginalLocalPosition;
                view.SetAlpha(0f);
                view.gameObject.SetActive(true);
                toFadeIn.Add(view);
            }
        }

        if (toFadeIn.Count == 0) yield break;

        float elapsed = 0f;
        while (elapsed < resetFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / resetFadeDuration;
            foreach (CardView view in toFadeIn)
            {
                view.SetAlpha(t);
            }
            yield return null;
        }

        foreach (CardView view in toFadeIn)
        {
            view.SetAlpha(1f);
        }
    }

    private Vector3 CalculateEnemySlotCenter()
    {
        Vector3 sum = Vector3.zero;
        int count = 0;
        foreach (CardView view in enemySlotViews)
        {
            if (view != null)
            {
                sum += view.OriginalLocalPosition;
                count++;
            }
        }
        return count > 0 ? sum / count : Vector3.zero;
    }

    private IEnumerator MoveViewToPosition(CardView view, Vector3 targetPos, float duration)
    {
        Vector3 start = view.transform.position;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            view.transform.position = Vector3.Lerp(start, targetPos, t);
            yield return null;
        }
        view.transform.position = targetPos;
    }

    private Vector3 CalculateEnemyCenter(List<CardView> aliveViews)
    {
        Vector3 sum = Vector3.zero;
        foreach (CardView view in aliveViews)
        {
            sum += view.transform.localPosition;
        }
        return sum / aliveViews.Count;
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
            bool canShow = !isAnimating && battleManager != null && !battleManager.IsBattleOver() && hasValidSelection;
            attackButton.interactable = canShow;
            attackButton.gameObject.SetActive(canShow);
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

    private void LoadCardBackSprites()
    {
        Sprite cardBack = Resources.Load<Sprite>("Art/Battle/battle_card_back");
        if (cardBack == null)
        {
            Debug.LogWarning("找不到卡背贴图，请确保 Resources/Art/Battle/battle_card_back.png 存在");
            return;
        }

        ApplyCardBackToViews(playerSlotViews, cardBack);
        ApplyCardBackToViews(enemySlotViews, cardBack);
    }

    private void ApplyCardBackToViews(CardView[] views, Sprite cardBack)
    {
        if (views == null) return;
        foreach (CardView view in views)
        {
            if (view != null)
            {
                view.cardBackSprite = cardBack;
            }
        }
    }

    /// <summary>
    /// 所有敌方卡牌翻到背面
    /// </summary>
    public void FlipEnemyCardsToBack()
    {
        foreach (CardView view in enemySlotViews)
        {
            if (view != null && view.gameObject.activeSelf)
            {
                view.FlipToBack();
            }
        }
    }

    /// <summary>
    /// 所有敌方卡牌翻回正面
    /// </summary>
    public void FlipEnemyCardsToFront()
    {
        foreach (CardView view in enemySlotViews)
        {
            if (view != null && view.gameObject.activeSelf)
            {
                view.FlipToFront();
            }
        }
    }

    /// <summary>
    /// 所有敌方卡牌翻回正面（无动画）
    /// </summary>
    public void ResetEnemyCardsToFront()
    {
        foreach (CardView view in enemySlotViews)
        {
            if (view != null && !view.IsShowingFront())
            {
                view.SetFaceInstant(true);
            }
        }
    }

    public CardView GetEnemySlotView(int index)
    {
        if (enemySlotViews == null || index < 0 || index >= enemySlotViews.Length) return null;
        return enemySlotViews[index];
    }

    public CardView GetPlayerSlotView(int index)
    {
        if (playerSlotViews == null || index < 0 || index >= playerSlotViews.Length) return null;
        return playerSlotViews[index];
    }
}
