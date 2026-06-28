using UnityEngine;

/// <summary>
/// 玩家英雄控制器，负责格子移动和到达格子的交互。
/// </summary>
public class Hero : MonoBehaviour
{
    public Vector2Int currentGridPos;
    public float moveSpeed = 2.5f;
    public Vector3 visualOffset = new Vector3(0, -0.35f, 0);

    private bool isMoving = false;
    private Vector3 targetPos;
    private HeroWalkAnimator walkAnimator;

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
                tile.OnHeroEnter();
            }
        }
    }

    public void TryMove(Vector2Int targetGridPos)
    {
        if (GameManager.Instance != null && GameManager.Instance.isBattleActive)
        {
            Debug.Log("战斗正在进行，主地图移动已锁定。");
            return;
        }

        if (!GameManager.Instance.isPlayerTurn)
        {
            Debug.Log("当前不是玩家回合。");
            return;
        }

        if (isMoving) return;

        // 点击当前所在格子时只触发交互，不消耗本回合行动。
        if (targetGridPos == currentGridPos)
        {
            Tile tile = MapManager.Instance.GetTileAt(currentGridPos);
            if (tile != null)
            {
                tile.OnHeroEnter();
            }
            return;
        }

        if (!MapManager.Instance.CanReachWithinSteps(currentGridPos, targetGridPos, 3))
        {
            Debug.Log("目标格子被阻挡或超出移动范围。");
            return;
        }

        if (GameManager.Instance.hasPlayerActed)
        {
            Debug.Log("玩家本回合已经行动过。");
            return;
        }

        Vector2Int movementDelta = targetGridPos - currentGridPos;
        currentGridPos = targetGridPos;
        targetPos = MapManager.Instance.GridToWorld(targetGridPos) + visualOffset;
        isMoving = true;
        if (walkAnimator != null)
        {
            walkAnimator.StartWalking(new Vector2(movementDelta.x, movementDelta.y));
        }

        GameManager.Instance.OnPlayerAction();
    }
}
