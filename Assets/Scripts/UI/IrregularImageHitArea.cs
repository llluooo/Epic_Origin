using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Makes a UI Image receive clicks only on non-transparent sprite pixels.
/// </summary>
[RequireComponent(typeof(Image))]
public class IrregularImageHitArea : MonoBehaviour
{
    [Range(0.01f, 1f)]
    public float alphaThreshold = 0.1f;

    [Tooltip("Keeps the mask nearly invisible while preserving raycast behavior.")]
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
