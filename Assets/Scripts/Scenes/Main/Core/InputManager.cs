using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 地图输入管理器，负责把鼠标点击转换为地图格子移动请求。
/// </summary>
public class InputManager : MonoBehaviour
{
    public Hero hero;

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isBattleActive)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            // 点击在UI上时不触发地图交互，防止按钮点击穿透到格子
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            HandleClick();
        }
    }

    void HandleClick()
    {
        if (Camera.main == null) return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        Vector2Int gridPos = MapManager.Instance.WorldToGrid(mousePos);

        Tile tile = MapManager.Instance.GetTileAt(gridPos);

        if (tile != null)
        {
            hero.TryMove(gridPos);
        }
    }
}
