using UnityEngine;

/// <summary>
/// 相机跟随英雄
/// </summary>
public class CameraFollow : MonoBehaviour
{
    public Transform target;       // 跟随目标（英雄）
    public float smoothSpeed = 5f;
    public float targetOrthographicSize = 2f;
    public Vector3 offset = new Vector3(0, 0, -10);  // 二维相机 Z 轴通常为 -10
    public bool clampToMapBounds = true;
    public bool autoUseMapBounds = true;
    public Vector2 minBounds;
    public Vector2 maxBounds;

    private Camera followCamera;
    private bool hasBounds;

    void Start()
    {
        followCamera = GetComponent<Camera>();
        if (followCamera != null)
        {
            followCamera.orthographicSize = targetOrthographicSize;
        }
        TryBindMapBounds();

        if (target != null)
            transform.position = ClampPositionToBounds(target.position + offset);
    }

    void LateUpdate()
    {
        if (target == null) return;

        TryBindMapBounds();
        Vector3 desiredPos = target.position + offset;
        desiredPos = ClampPositionToBounds(desiredPos);
        transform.position = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);
    }

    public void SetBounds(Rect bounds)
    {
        minBounds = bounds.min;
        maxBounds = bounds.max;
        hasBounds = true;
    }

    public Vector3 ClampPositionToBounds(Vector3 desiredPos)
    {
        if (!clampToMapBounds || !hasBounds)
        {
            return desiredPos;
        }

        Camera cameraComponent = followCamera != null ? followCamera : GetComponent<Camera>();
        if (cameraComponent == null || !cameraComponent.orthographic)
        {
            return desiredPos;
        }

        float halfHeight = cameraComponent.orthographicSize;
        float halfWidth = halfHeight * cameraComponent.aspect;
        float clampedX = ClampAxis(desiredPos.x, minBounds.x + halfWidth, maxBounds.x - halfWidth);
        float clampedY = ClampAxis(desiredPos.y, minBounds.y + halfHeight, maxBounds.y - halfHeight);

        return new Vector3(clampedX, clampedY, desiredPos.z);
    }

    private void TryBindMapBounds()
    {
        if (!autoUseMapBounds || hasBounds || MapManager.Instance == null || !MapManager.Instance.HasMap)
        {
            return;
        }

        SetBounds(MapManager.Instance.GetMapWorldRect());
    }

    private static float ClampAxis(float value, float min, float max)
    {
        if (min > max)
        {
            return (min + max) * 0.5f;
        }

        return Mathf.Clamp(value, min, max);
    }
}
