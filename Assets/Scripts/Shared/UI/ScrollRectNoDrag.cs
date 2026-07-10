using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    /// <summary>
    /// 禁用鼠标拖拽的 ScrollRect，仅响应滚轮滚动。
    /// </summary>
    public class ScrollRectNoDrag : ScrollRect
    {
        public override void OnBeginDrag(PointerEventData eventData) { }
        public override void OnDrag(PointerEventData eventData) { }
    }
}
