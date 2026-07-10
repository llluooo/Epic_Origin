using UnityEngine;

/// <summary>
/// 地图格子基类，所有地图格子脚本的父类。
/// </summary>
public abstract class Tile : MonoBehaviour
{
    public Vector2Int gridPosition; // 格子坐标
    public virtual bool IsWalkable => true;

    /// <summary>
    /// 该格子是否已被清除（一次性 POI 交互后标记）
    /// </summary>
    public bool Cleared { get; private set; }

    /// <summary>
    /// 英雄进入该格子时触发。
    /// </summary>
    public virtual void OnHeroEnter()
    {
        Debug.Log("进入普通格子: " + gridPosition);
    }

    /// <summary>
    /// 标记格子已被清除，隐藏 POI 视觉效果。
    /// </summary>
    public virtual void MarkCleared()
    {
        if (Cleared) return;
        Cleared = true;
        TileVisual vis = GetComponent<TileVisual>();
        if (vis != null) vis.HidePoi();
    }
}
