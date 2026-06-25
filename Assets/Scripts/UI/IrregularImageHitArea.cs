using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 让 UI 图片只在非透明像素区域响应点击。
/// </summary>
[RequireComponent(typeof(Image))]
public class IrregularImageHitArea : MonoBehaviour
{
    [Range(0.01f, 1f)]
    public float alphaThreshold = 0.1f;

    [Tooltip("保持遮罩接近不可见，同时保留射线检测行为。")]
    public bool keepMaskInvisible = true;

    private Image image;

    void Awake()
    {
        Apply();
    }

    void OnValidate()
    {
        Apply();
    }

    public void Apply()
    {
        if (image == null)
            image = GetComponent<Image>();

        if (image == null)
            return;

        image.raycastTarget = true;
        image.alphaHitTestMinimumThreshold = alphaThreshold;

        if (keepMaskInvisible)
        {
            Color color = image.color;
            color.a = 0.01f;
            image.color = color;
        }
    }
}
