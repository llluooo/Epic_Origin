using UnityEngine;

public class TileVisual : MonoBehaviour
{
    public SpriteRenderer groundRenderer;
    public SpriteRenderer poiRenderer;
    public SpriteRenderer overlayRenderer;

    private void Reset()
    {
        groundRenderer = GetComponent<SpriteRenderer>();
    }

    public void ConfigureForGeneratedTile()
    {
        AutoBindRenderers();
        DisableUnmanagedRenderers();
    }

    public void SetGround(Sprite sprite, float targetWorldSize)
    {
        if (groundRenderer == null)
        {
            groundRenderer = GetComponent<SpriteRenderer>();
        }

        SetRendererSprite(groundRenderer, sprite, sprite != null, targetWorldSize, 0);
    }

    public void SetPoi(Sprite sprite, float targetWorldSize)
    {
        SetRendererSprite(poiRenderer, sprite, sprite != null, targetWorldSize, 10);
    }

    public void SetOverlay(Sprite sprite, bool visible, float targetWorldSize)
    {
        SetRendererSprite(overlayRenderer, sprite, visible && sprite != null, targetWorldSize, 20);
    }

    private static void SetRendererSprite(SpriteRenderer renderer, Sprite sprite, bool visible, float targetWorldSize, int sortingOrder)
    {
        if (renderer == null) return;

        if (!renderer.gameObject.activeSelf)
        {
            renderer.gameObject.SetActive(true);
        }

        renderer.sprite = sprite;
        renderer.color = Color.white;
        renderer.enabled = visible;
        renderer.sortingOrder = sortingOrder;
        renderer.drawMode = SpriteDrawMode.Simple;

        if (sprite != null && targetWorldSize > 0f)
        {
            FitRendererToWorldSize(renderer, targetWorldSize);
        }
    }

    private static void FitRendererToWorldSize(SpriteRenderer renderer, float targetWorldSize)
    {
        Vector2 spriteSize = renderer.sprite.bounds.size;
        float largestSide = Mathf.Max(spriteSize.x, spriteSize.y);
        if (largestSide <= 0f) return;

        float scale = targetWorldSize / largestSide;
        renderer.transform.localScale = new Vector3(scale, scale, renderer.transform.localScale.z);
    }

    private void AutoBindRenderers()
    {
        SpriteRenderer childGround = FindChildRenderer("Ground");
        SpriteRenderer childPoi = FindChildRenderer("POI");
        SpriteRenderer childOverlay = FindChildRenderer("Overlay");

        if (childGround != null) groundRenderer = childGround;
        else if (groundRenderer == null) groundRenderer = GetComponent<SpriteRenderer>();

        if (childPoi != null) poiRenderer = childPoi;
        if (childOverlay != null) overlayRenderer = childOverlay;
    }

    private SpriteRenderer FindChildRenderer(string childName)
    {
        Transform child = transform.Find(childName);
        return child != null ? child.GetComponent<SpriteRenderer>() : null;
    }

    private void DisableUnmanagedRenderers()
    {
        foreach (SpriteRenderer renderer in GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (renderer != groundRenderer && renderer != poiRenderer && renderer != overlayRenderer)
            {
                renderer.enabled = false;
            }
        }
    }
}
