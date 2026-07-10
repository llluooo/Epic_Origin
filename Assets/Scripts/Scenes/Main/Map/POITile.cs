using UnityEngine;

/// <summary>
/// 一次性 POI 地图格子中间层。继承 Tile，封装"进入一次后清除"的通用逻辑。
/// 子类只需实现 OnPOIEnter() 定义具体行为。
/// </summary>
public abstract class POITile : Tile
{
    /// <summary>
    /// 是否在 OnPOIEnter 执行后自动标记清除。默认 true。
    /// </summary>
    protected virtual bool AutoClear => true;

    public override void OnHeroEnter()
    {
        if (Cleared) return;
        OnPOIEnter();
        if (AutoClear)
            MarkCleared();
    }

    /// <summary>
    /// POI 具体交互逻辑，由最终子类实现。
    /// </summary>
    protected abstract void OnPOIEnter();
}
