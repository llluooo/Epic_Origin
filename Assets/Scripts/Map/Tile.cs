using UnityEngine;

/// <summary>
/// 地图格子基类，所有地图格子脚本的父类。
/// </summary>
public abstract class Tile : MonoBehaviour
{
    public Vector2Int gridPosition; // 格子坐标
    public virtual bool IsWalkable => true;

    /// <summary>
    /// 英雄进入该格子时触发。
    /// </summary>
    public virtual void OnHeroEnter()
    {
        Debug.Log("进入普通格子: " + gridPosition);
    }
}
