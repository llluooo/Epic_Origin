using UnityEngine;

/// <summary>
/// 相机跟随英雄
/// </summary>
public class CameraFollow : MonoBehaviour
{
    public Transform target;       // 跟随目标（Hero）
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(0, 0, -10);  // 2D相机Z轴通常-10

    void Start()
    {
        if (target != null)
            transform.position = target.position + offset;
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPos = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);
    }
}
