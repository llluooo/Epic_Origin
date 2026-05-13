using UnityEngine;

/// <summary>
/// 地图格子基类（所有格子的父类）
/// </summary>
public abstract class Tile : MonoBehaviour
{
    public Vector2Int gridPosition; // 格子坐标

    /// <summary>
    /// 英雄进入该格子时触发
    /// </summary>
    public virtual void OnHeroEnter()
    {
        Debug.Log("进入普通格子: " + gridPosition);
    }
}