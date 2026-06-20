using UnityEngine;

/// <summary>
/// ����������������
/// </summary>
public class InputManager : MonoBehaviour
{
    public Hero hero;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
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