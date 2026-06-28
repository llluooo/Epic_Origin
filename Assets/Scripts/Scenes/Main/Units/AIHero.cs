using UnityEngine;
using Action = System.Action;

/// <summary>
/// AI 英雄控制器，由 AIController 驱动移动，在地图上提供 AI 的视觉表现。
/// </summary>
public class AIHero : MonoBehaviour
{
    public Vector2Int currentGridPos;
    public float moveSpeed = 1.5f;
    public Vector3 visualOffset = new Vector3(0, -0.35f, 0);

    private bool isMoving = false;
    private Vector3 targetPos;
    private HeroWalkAnimator walkAnimator;

    /// <summary>
    /// 移动完成后的回调，通知 GameManager AI 英雄已到达目标。
    /// </summary>
    public event Action OnMoveComplete;

    /// <summary>
    /// AI 英雄是否正在移动中。
    /// </summary>
    public bool IsMoving => isMoving;

    private void Awake()
    {
        walkAnimator = GetComponent<HeroWalkAnimator>();
    }

    public void SetRaceAppearance(RaceType race)
    {
        if (walkAnimator == null)
        {
            walkAnimator = GetComponent<HeroWalkAnimator>();
        }

        if (walkAnimator != null)
        {
            walkAnimator.SetRace(race);
        }
    }

    void Update()
    {
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPos) < 0.01f)
            {
                transform.position = targetPos;
                isMoving = false;

                if (walkAnimator != null)
                {
                    walkAnimator.StopWalking();
                }

                Tile tile = MapManager.Instance.GetTileAt(currentGridPos);
                if (tile != null)
                {
                    OnAIEnterTile(tile);
                }

                OnMoveComplete?.Invoke();
            }
        }
    }

    /// <summary>
    /// AI 英雄移动到指定格子坐标。由 AIController 调用。
    /// </summary>
    public void MoveTo(Vector2Int targetGridPos)
    {
        if (isMoving) return;

        if (targetGridPos == currentGridPos) return;

        Vector2Int movementDelta = targetGridPos - currentGridPos;
        currentGridPos = targetGridPos;
        targetPos = MapManager.Instance.GridToWorld(targetGridPos) + visualOffset;
        isMoving = true;

        if (walkAnimator != null)
        {
            walkAnimator.StartWalking(new Vector2(movementDelta.x, movementDelta.y));
        }

        Debug.Log($"[AIHero] 移动到格子 {targetGridPos}");
    }

    /// <summary>
    /// 直接设置位置（不播放移动动画），用于初始化或存档恢复。
    /// </summary>
    public void SetPosition(Vector2Int gridPos, Vector3 worldPos)
    {
        currentGridPos = gridPos;
        transform.position = worldPos + visualOffset;
        isMoving = false;

        if (walkAnimator != null)
        {
            walkAnimator.StopWalking();
        }
    }

    /// <summary>
    /// AI 英雄进入格子时的交互逻辑。
    /// </summary>
    private void OnAIEnterTile(Tile tile)
    {
        if (tile is ResourceTile resourceTile)
        {
            int gold = Random.Range(3, 9);
            int mat = Random.Range(2, 6);
            if (Random.value < 0.5f)
            {
                GameManager.Instance.aiPlayer.resources.gold += gold;
                Debug.Log($"[AIHero] 在资源点获得 {gold} 金币");
            }
            else
            {
                GameManager.Instance.aiPlayer.resources.buildingMaterials += mat;
                Debug.Log($"[AIHero] 在资源点获得 {mat} 建材");
            }
        }
        else if (tile is ArmyCampTile armyCamp)
        {
            int aiPower = GameManager.Instance.aiPlayer.deck.GetTotalCombatPower();
            int enemyPower = Random.Range(10, 51);
            if (aiPower >= enemyPower)
            {
                int goldReward = Random.Range(15, 36);
                GameManager.Instance.aiPlayer.resources.gold += goldReward;
                Debug.Log($"[AIHero] 战胜军营，获得 {goldReward} 金币");
            }
            else
            {
                int goldLoss = Random.Range(5, 16);
                GameManager.Instance.aiPlayer.resources.gold -= goldLoss;
                Debug.Log($"[AIHero] 败给军营，损失 {goldLoss} 金币");
            }
        }
        else if (tile is EventTile eventTile)
        {
            float roll = Random.value;
            if (roll < 0.4f)
            {
                GameManager.Instance.aiPlayer.resources.gold += Random.Range(3, 9);
                Debug.Log("[AIHero] 事件：获得资源");
            }
            else if (roll < 0.7f)
            {
                GameManager.Instance.aiPlayer.resources.gold -= Random.Range(2, 6);
                Debug.Log("[AIHero] 事件：损失金币");
            }
        }
    }
}
