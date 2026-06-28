using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Tooltip("按下时缩放的比例，0.9 表示缩小到原来的 90%")]
    public float pressScale = 0.9f;

    [Tooltip("恢复到原始大小的时间（秒）")]
    public float restoreDuration = 0.1f;

    private Vector3 originalScale;

    void Start()
    {
        // 记录按钮初始缩放。
        originalScale = transform.localScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // 按下时立刻缩小。
        transform.localScale = originalScale * pressScale;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // 松开时恢复原始大小。
        transform.localScale = originalScale;
    }
}
