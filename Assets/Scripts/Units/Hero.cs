using UnityEngine;

/// <summary>
/// Hero controller for the player unit.
/// </summary>
public class Hero : MonoBehaviour
{
    public Vector2Int currentGridPos;
    public float moveSpeed = 5f;

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
        if (!GameManager.Instance.isPlayerTurn)
        {
            Debug.Log("It is not the player's turn.");
            return;
        }

        if (isMoving) return;

        // Standing on the current tile opens/interacts without spending the turn.
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
            Debug.Log("Target is blocked or outside movement range.");
            return;
        }

        if (GameManager.Instance.hasPlayerActed)
        {
            Debug.Log("The player has already acted this turn.");
            return;
        }

        Vector2Int movementDelta = targetGridPos - currentGridPos;
        currentGridPos = targetGridPos;
        targetPos = MapManager.Instance.GridToWorld(targetGridPos);
        isMoving = true;
        if (walkAnimator != null)
        {
            walkAnimator.StartWalking(new Vector2(movementDelta.x, movementDelta.y));
        }

        GameManager.Instance.OnPlayerAction();
    }
}
