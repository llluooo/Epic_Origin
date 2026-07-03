using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    [Header("结算面板")]
    public GameObject battleResultPanel;
    public GameObject battleVictoryResultPanel;
    public GameObject battleDefeatResultPanel;
    public TMP_Text battleResultTitleText;
    public TMP_Text battleResultDetailText;
    public Button battleResultContinueButton;
    public Sprite battleResultContinueButtonSprite;

    [Header("胜利面板独立绑定")]
    public TMP_Text battleVictoryTitleText;
    public TMP_Text battleVictoryDetailText;
    public Button battleVictoryContinueButton;

    [Header("失败面板独立绑定")]
    public TMP_Text battleDefeatTitleText;
    public TMP_Text battleDefeatDetailText;
    public Button battleDefeatContinueButton;

    public bool showResultPanel = true;
    public float resultPanelAutoCloseDelay = 0.8f;

    [Header("动画")]
    public float gatherDuration = 0.6f;
    public float arenaMoveDuration = 0.35f;
    public float resetStayDuration = 1f;
    public float resetFadeDuration = 0.5f;
    public float resetMoveDuration = 0.35f;
    public float recenterDuration = 0.35f;

    [Header("动画锚点")]
    public Transform playerPlaceAnchor;
    public Transform arenaLeftAnchor;
    public Transform arenaRightAnchor;

    private int selectedPlayerIndex = -1;
    private bool resultSubmitted;
    private bool isAnimating;

    // 居中汇集布局缓存（基于场景初始位置计算，不变）
    private float playerSpacing;
    private float playerCenterX;
    private float enemySpacing;
    private float enemyCenterX;

    void Start()
    {
        LoadCardBackSprites();
        BindControls();
        CacheLayoutParams();
        AutoBindResultReferences();
        HideBattleResultPanel();
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

        // ⑤ 我方选中卡牌移至出牌区，同时标准化大小和旋转（以敌方世界缩放为准）
        if (playerPlaceAnchor != null && playerView != null)
        {
            // 用世界空间缩放确保视觉大小一致
            Vector3 enemyWorldScale = enemyView.transform.lossyScale;
            Vector3 playerParentWorldScale = playerView.transform.parent != null
                ? playerView.transform.parent.lossyScale
                : Vector3.one;
            Vector3 standardScale = new Vector3(
                enemyWorldScale.x / playerParentWorldScale.x,
                enemyWorldScale.y / playerParentWorldScale.y,
                enemyWorldScale.z / playerParentWorldScale.z);
            Quaternion standardRotation = enemyView.transform.localRotation;
            yield return playerView.StartCoroutine(
                playerView.MoveWithScaleAndRotation(playerPlaceAnchor.position, standardScale, standardRotation, arenaMoveDuration));
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
    /// 重置动画：对峙停留 → 归位 → 存活/死亡分支 → 其余淡入 → 居中汇集 → 敌方翻面
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
        Vector3 pScaleStart = Vector3.zero, pScaleTarget = Vector3.zero;
        Quaternion pRotStart = Quaternion.identity, pRotTarget = Quaternion.identity;
        CanvasGroup pCG = null, eCG = null;
        float pFadeStart = 1f, eFadeStart = 1f;

        if (playerView != null)
        {
            pCG = playerView.GetComponent<CanvasGroup>();
            if (playerAlive)
            {
                pSlideStart = playerView.transform.localPosition;
                pSlideTarget = playerView.OriginalLocalPosition;
                pScaleStart = playerView.transform.localScale;
                pScaleTarget = playerView.OriginalLocalScale;
                pRotStart = playerView.transform.localRotation;
                pRotTarget = playerView.OriginalLocalRotation;
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
                    playerView.transform.localScale = Vector3.Lerp(pScaleStart, pScaleTarget, t);
                    playerView.transform.localRotation = Quaternion.Slerp(pRotStart, pRotTarget, t);
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
            if (playerAlive)
            {
                playerView.transform.localPosition = pSlideTarget;
                playerView.transform.localScale = pScaleTarget;
                playerView.transform.localRotation = pRotTarget;
            }
            else { playerView.SetAlpha(0f); playerView.gameObject.SetActive(false); }
        }
        if (enemyView != null)
        {
            if (enemyAlive) enemyView.transform.localPosition = eSlideTarget;
            else { enemyView.SetAlpha(0f); enemyView.gameObject.SetActive(false); }
        }

        // ⑬ 其余卡牌淡入（敌方保持背面，我方正面）
        yield return StartCoroutine(FadeInOtherCards(playerDataIndex, enemyDataIndex));

        // 居中汇集：卡牌死亡后剩余卡牌向中间靠拢
        yield return StartCoroutine(RecenterCards(battleManager.playerCards, playerSlotViews, recenterDuration, playerSpacing, playerCenterX));
        yield return StartCoroutine(RecenterCards(battleManager.enemyCards, enemySlotViews, recenterDuration, enemySpacing, enemyCenterX));

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

    /// <summary>
    /// 缓存双方卡牌布局参数（基于场景初始位置，只计算一次）
    /// </summary>
    private void CacheLayoutParams()
    {
        if (playerSlotViews != null && playerSlotViews.Length > 1)
        {
            float minX = float.MaxValue, maxX = float.MinValue;
            foreach (CardView view in playerSlotViews)
            {
                if (view != null)
                {
                    float x = view.OriginalLocalPosition.x;
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                }
            }
            playerSpacing = (maxX - minX) / (playerSlotViews.Length - 1);
            playerCenterX = (maxX + minX) / 2f;
        }

        if (enemySlotViews != null && enemySlotViews.Length > 1)
        {
            float minX = float.MaxValue, maxX = float.MinValue;
            foreach (CardView view in enemySlotViews)
            {
                if (view != null)
                {
                    float x = view.OriginalLocalPosition.x;
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                }
            }
            enemySpacing = (maxX - minX) / (enemySlotViews.Length - 1);
            enemyCenterX = (maxX + minX) / 2f;
        }
    }

    /// <summary>
    /// 存活卡牌居中汇集：当有卡牌死亡后，剩余卡牌向中心靠拢消除空位
    /// </summary>
    private IEnumerator RecenterCards(List<BattleCard> cards, CardView[] views, float duration, float spacing, float centerX)
    {
        if (cards == null || views == null || views.Length == 0) yield break;

        // 收集存活卡牌
        List<CardView> aliveViews = new List<CardView>();
        for (int i = 0; i < views.Length && i < cards.Count; i++)
        {
            if (cards[i] != null && cards[i].IsAlive() && views[i] != null && views[i].gameObject.activeSelf)
            {
                aliveViews.Add(views[i]);
            }
        }

        int aliveCount = aliveViews.Count;
        if (aliveCount <= 1 || aliveCount >= views.Length) yield break;

        // 计算每张存活卡牌的目标位置
        Vector3[] startPositions = new Vector3[aliveCount];
        Vector3[] targetPositions = new Vector3[aliveCount];
        for (int i = 0; i < aliveCount; i++)
        {
            float targetX = centerX + (i - (aliveCount - 1) / 2f) * spacing;
            CardView view = aliveViews[i];
            startPositions[i] = view.transform.localPosition;
            targetPositions[i] = new Vector3(targetX, view.OriginalLocalPosition.y, 0);
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            for (int i = 0; i < aliveCount; i++)
            {
                aliveViews[i].transform.localPosition = Vector3.Lerp(startPositions[i], targetPositions[i], t);
            }
            yield return null;
        }

        // 动画完成后更新卡牌"家"位置，下一轮不会回到原位
        for (int i = 0; i < aliveCount; i++)
        {
            aliveViews[i].transform.localPosition = targetPositions[i];
            aliveViews[i].UpdateOriginalPosition(targetPositions[i]);
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

        if (battleManager != null && battleManager.IsBattleOver() && showResultPanel)
        {
            bool playerVictory = IsOutcomePlayerWin(battleManager.outcome);
            if (battleVictoryResultPanel != null || battleDefeatResultPanel != null)
            {
                if (battleVictoryResultPanel != null)
                {
                    battleVictoryResultPanel.SetActive(playerVictory);
                }
                if (battleDefeatResultPanel != null)
                {
                    battleDefeatResultPanel.SetActive(!playerVictory);
                }
            }
            else if (battleResultPanel != null)
            {
                battleResultPanel.SetActive(true);
            }
        }
    }

    private void TrySubmitBattleResult()
    {
        if (resultSubmitted || battleManager == null || !battleManager.IsBattleOver())
        {
            return;
        }

        resultSubmitted = true;
        if (showResultPanel)
        {
            ShowBattleResult();
        }
        else
        {
            if (returnToMapWhenBattleEnds)
            {
                StartCoroutine(ReturnToMapAfterDelay());
            }
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

    private void ShowBattleResult()
    {
        if (battleManager == null)
        {
            return;
        }

        bool playerVictory = IsOutcomePlayerWin(battleManager.outcome);
        HideBattleResultPanel();

        GameObject targetPanel = playerVictory ? battleVictoryResultPanel : battleDefeatResultPanel;
        if (targetPanel == null && battleResultPanel != null)
        {
            targetPanel = battleResultPanel;
        }

        if (targetPanel == null)
        {
            targetPanel = CreateFallbackResultPanel(playerVictory);
        }

        if (targetPanel != null)
        {
            ApplyResultTextToPanel(targetPanel, playerVictory);
            BindContinueButton(targetPanel, playerVictory);
            targetPanel.SetActive(true);
        }

        if (!showResultPanel)
        {
            StartCoroutine(AutoCloseResultPanel());
        }
    }

    private string GetBattleDetailText(BattleOutcome outcome)
    {
        if (battleManager == null)
        {
            return string.Empty;
        }

        int playerLost;
        int playerRemain;
        int enemyLost;
        int enemyRemain;
        BuildBattleCounts(battleManager.playerCards, out playerLost, out playerRemain);
        BuildBattleCounts(battleManager.enemyCards, out enemyLost, out enemyRemain);

        string enemyDead = GetDeadUnitNames(battleManager.enemyCards);
        string playerDead = GetDeadUnitNames(battleManager.playerCards);
        string playerStatus = GetRemainingUnitStatus(battleManager.playerCards);
        string rewardLine = GetBattleRewardLine(outcome);

        List<string> lines = new List<string>
        {
            $"敌方损失：{enemyLost} 部队，剩余：{enemyRemain}；我方损失：{playerLost} 部队，剩余：{playerRemain}。",
            $"敌方阵亡：{enemyDead}",
            $"我方阵亡：{playerDead}",
            $"我方剩余生命：{playerStatus}",
            $"奖励 / 惩罚：{rewardLine}"
        };

        return string.Join("\n", lines);
    }

    private void BuildBattleCounts(List<BattleCard> cards, out int lost, out int remain)
    {
        lost = 0;
        remain = 0;
        if (cards == null)
        {
            return;
        }

        foreach (BattleCard card in cards)
        {
            if (card == null)
            {
                continue;
            }

            lost += Math.Max(0, card.initialCount - card.currentCount);
            remain += card.currentCount;
        }
    }

    private string GetDeadUnitNames(List<BattleCard> cards)
    {
        if (cards == null)
        {
            return "无";
        }

        List<string> deadNames = new List<string>();
        foreach (BattleCard card in cards)
        {
            if (card == null || card.IsAlive())
            {
                continue;
            }

            string name = card.card.cardName;
            if (card.initialCount > 1)
            {
                name += $" x{card.initialCount}";
            }
            deadNames.Add(name);
        }

        return deadNames.Count > 0 ? string.Join("，", deadNames) : "无";
    }

    private string GetRemainingUnitStatus(List<BattleCard> cards)
    {
        if (cards == null)
        {
            return "无";
        }

        List<string> statusLines = new List<string>();
        foreach (BattleCard card in cards)
        {
            if (card == null || !card.IsAlive())
            {
                continue;
            }

            string name = card.card.cardName;
            string countText = card.currentCount > 1 ? $" x{card.currentCount}" : string.Empty;
            statusLines.Add($"{name}{countText} ({card.currentHP}/{card.initialHP})");
        }

        return statusLines.Count > 0 ? string.Join("，", statusLines) : "无";
    }

    private string GetBattleRewardLine(BattleOutcome outcome)
    {
        BattleEncounterType encounterType = BattleSceneBridge.EncounterType;
        switch (outcome)
        {
            case BattleOutcome.PlayerVictory:
                if (encounterType == BattleEncounterType.ArmyCamp)
                {
                    return "战胜兵营，获得随机战利品：2~3 张卡牌或 30~50 金币。";
                }
                if (encounterType == BattleEncounterType.EnemyStronghold)
                {
                    return "成功攻克敌方据点，获得胜利奖励。";
                }
                return "玩家获得胜利，返回地图继续冒险。";
            case BattleOutcome.EnemyVictory:
                return "敌方获胜，参战部队损失严重，返回地图重整军力。";
            case BattleOutcome.Draw:
                return "双方同归于尽，战斗结束，需补充兵力后再战。";
            case BattleOutcome.PlayerSurrender:
                return "玩家投降，损失 20% 金币和 20% 建材。";
            case BattleOutcome.EnemySurrender:
                return "敌方投降，获得战斗胜利。";
            default:
                return "战斗结束，返回地图继续游戏。";
        }
    }

    private IEnumerator AutoCloseResultPanel()
    {
        yield return new WaitForSeconds(Mathf.Max(0f, resultPanelAutoCloseDelay));
        HideBattleResultPanel();
        StartCoroutine(ReturnToMapAfterDelay());
    }

    private void HideBattleResultPanel()
    {
        if (battleResultPanel != null)
        {
            battleResultPanel.SetActive(false);
        }
        if (battleVictoryResultPanel != null)
        {
            battleVictoryResultPanel.SetActive(false);
        }
        if (battleDefeatResultPanel != null)
        {
            battleDefeatResultPanel.SetActive(false);
        }
    }

    private void OnBattleResultContinue()
    {
        BattleSceneBridge.ResolveAndReturn(battleManager.outcome);
    }

    private void AutoBindResultReferences()
    {
        if (battleVictoryResultPanel == null)
        {
            battleVictoryResultPanel = FindChildByName("BattleResult_Victory");
        }

        if (battleDefeatResultPanel == null)
        {
            battleDefeatResultPanel = FindChildByName("BattleResult_Defeat");
        }

        if (battleResultPanel == null)
        {
            battleResultPanel = FindChildByName("BattleResultPanel");
        }

        if (battleVictoryTitleText == null && battleVictoryResultPanel != null)
        {
            battleVictoryTitleText = FindFirstTextInPanel(battleVictoryResultPanel);
        }
        if (battleVictoryDetailText == null && battleVictoryResultPanel != null)
        {
            battleVictoryDetailText = FindSecondTextInPanel(battleVictoryResultPanel);
        }
        if (battleVictoryContinueButton == null && battleVictoryResultPanel != null)
        {
            battleVictoryContinueButton = FindFirstButtonInPanel(battleVictoryResultPanel);
        }

        if (battleDefeatTitleText == null && battleDefeatResultPanel != null)
        {
            battleDefeatTitleText = FindFirstTextInPanel(battleDefeatResultPanel);
        }
        if (battleDefeatDetailText == null && battleDefeatResultPanel != null)
        {
            battleDefeatDetailText = FindSecondTextInPanel(battleDefeatResultPanel);
        }
        if (battleDefeatContinueButton == null && battleDefeatResultPanel != null)
        {
            battleDefeatContinueButton = FindFirstButtonInPanel(battleDefeatResultPanel);
        }

        if (battleResultTitleText == null)
        {
            if (battleVictoryTitleText != null)
            {
                battleResultTitleText = battleVictoryTitleText;
            }
            else if (battleDefeatTitleText != null)
            {
                battleResultTitleText = battleDefeatTitleText;
            }
        }

        if (battleResultDetailText == null)
        {
            if (battleVictoryDetailText != null)
            {
                battleResultDetailText = battleVictoryDetailText;
            }
            else if (battleDefeatDetailText != null)
            {
                battleResultDetailText = battleDefeatDetailText;
            }
        }

        if (battleResultContinueButton == null)
        {
            if (battleVictoryContinueButton != null)
            {
                battleResultContinueButton = battleVictoryContinueButton;
            }
            else if (battleDefeatContinueButton != null)
            {
                battleResultContinueButton = battleDefeatContinueButton;
            }
        }
    }

    private GameObject CreateFallbackResultPanel(bool playerVictory)
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("Canvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        GameObject panelObject = new GameObject(playerVictory ? "BattleResult_Victory" : "BattleResult_Defeat");
        panelObject.transform.SetParent(canvas.transform, false);

        RectTransform rect = panelObject.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = panelObject.AddComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.7f);

        GameObject titleObject = new GameObject("TitleText");
        titleObject.transform.SetParent(panelObject.transform, false);
        RectTransform titleRect = titleObject.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.6f);
        titleRect.anchorMax = new Vector2(0.5f, 0.6f);
        titleRect.sizeDelta = new Vector2(400f, 80f);
        titleRect.anchoredPosition = Vector2.zero;

        TMP_Text titleText = titleObject.AddComponent<TextMeshProUGUI>();
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontSize = 42;
        titleText.color = Color.white;
        titleText.text = "战斗结果";

        GameObject buttonObject = new GameObject("ContinueButton");
        buttonObject.transform.SetParent(panelObject.transform, false);
        RectTransform buttonRect = buttonObject.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(1f, 1f);
        buttonRect.anchorMax = new Vector2(1f, 1f);
        buttonRect.pivot = new Vector2(1f, 1f);
        buttonRect.sizeDelta = new Vector2(220f, 70f);
        buttonRect.anchoredPosition = new Vector2(-20f, -20f);

        Image buttonImage = buttonObject.AddComponent<Image>();
        buttonImage.color = Color.white;
        if (battleResultContinueButtonSprite != null)
        {
            buttonImage.sprite = battleResultContinueButtonSprite;
            buttonImage.type = Image.Type.Sliced;
        }

        Button button = buttonObject.AddComponent<Button>();
        GameObject buttonTextObject = new GameObject("Text");
        buttonTextObject.transform.SetParent(buttonObject.transform, false);
        RectTransform buttonTextRect = buttonTextObject.AddComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.offsetMin = Vector2.zero;
        buttonTextRect.offsetMax = Vector2.zero;
        TMP_Text buttonText = buttonTextObject.AddComponent<TextMeshProUGUI>();
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.fontSize = 24;
        buttonText.color = Color.black;
        buttonText.text = "继续";

        if (playerVictory)
        {
            battleVictoryResultPanel = panelObject;
        }
        else
        {
            battleDefeatResultPanel = panelObject;
        }

        return panelObject;
    }

    private void ApplyResultTextToPanel(GameObject panel, bool playerVictory)
    {
        if (panel == null || battleManager == null)
        {
            return;
        }

        TMP_Text titleText = playerVictory ? battleVictoryTitleText : battleDefeatTitleText;
        TMP_Text detailText = playerVictory ? battleVictoryDetailText : battleDefeatDetailText;

        if (titleText == null)
        {
            titleText = battleResultTitleText;
        }
        if (detailText == null)
        {
            detailText = battleResultDetailText;
        }

        if (titleText == null)
        {
            titleText = FindFirstTextInPanel(panel);
        }
        if (detailText == null)
        {
            detailText = FindSecondTextInPanel(panel);
        }

        if (titleText != null)
        {
            titleText.text = battleManager.battleResult;
        }

        if (detailText != null)
        {
            detailText.text = GetBattleDetailText(battleManager.outcome);
        }
    }

    private void BindContinueButton(GameObject panel, bool playerVictory)
    {
        if (panel == null)
        {
            return;
        }

        Button button = playerVictory ? battleVictoryContinueButton : battleDefeatContinueButton;
        if (button == null)
        {
            button = battleResultContinueButton;
        }
        if (button == null)
        {
            button = FindFirstButtonInPanel(panel);
        }

        if (button != null)
        {
            if (playerVictory)
            {
                battleVictoryContinueButton = button;
            }
            else
            {
                battleDefeatContinueButton = button;
            }

            battleResultContinueButton = button;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnBattleResultContinue);
        }
    }

    private GameObject FindChildByName(string name)
    {
        Transform target = transform.Find(name);
        if (target != null)
        {
            return target.gameObject;
        }

        foreach (Transform child in transform)
        {
            if (child.name == name)
            {
                return child.gameObject;
            }
        }

        foreach (Transform sceneObject in Resources.FindObjectsOfTypeAll<Transform>())
        {
            if (sceneObject == null || sceneObject.gameObject == null)
            {
                continue;
            }

            if (!sceneObject.gameObject.scene.IsValid())
            {
                continue;
            }

            if (sceneObject.name == name)
            {
                return sceneObject.gameObject;
            }
        }

        return null;
    }

    private TMP_Text FindFirstTextInPanel(GameObject panel)
    {
        if (panel == null)
        {
            return null;
        }

        TMP_Text[] texts = panel.GetComponentsInChildren<TMP_Text>(true);
        return texts != null && texts.Length > 0 ? texts[0] : null;
    }

    private TMP_Text FindSecondTextInPanel(GameObject panel)
    {
        if (panel == null)
        {
            return null;
        }

        TMP_Text[] texts = panel.GetComponentsInChildren<TMP_Text>(true);
        return texts != null && texts.Length > 1 ? texts[1] : null;
    }

    private Button FindFirstButtonInPanel(GameObject panel)
    {
        if (panel == null)
        {
            return null;
        }

        Button[] buttons = panel.GetComponentsInChildren<Button>(true);
        return buttons != null && buttons.Length > 0 ? buttons[0] : null;
    }

    private bool IsOutcomePlayerWin(BattleOutcome outcome)
    {
        return outcome == BattleOutcome.PlayerVictory || outcome == BattleOutcome.EnemySurrender;
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
