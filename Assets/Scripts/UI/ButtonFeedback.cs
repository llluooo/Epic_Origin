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
        // 记录按钮最开始的缩放大小
        originalScale = transform.localScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // 按下时立刻缩小
        transform.localScale = originalScale * pressScale;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // 松开时恢复原始大小（可以用平滑恢复，也可以直接赋值）
        transform.localScale = originalScale;
    }
}