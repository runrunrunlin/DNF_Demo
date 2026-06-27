using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;       // 玩家Transform
    public float smoothSpeed = 0.125f; // 平滑速度

    void LateUpdate()
    {
        if (player == null) return;

        // 目标位置只更新X轴，Y和Z保持摄像机当前值
        Vector3 targetPosition = new Vector3(player.position.x, transform.position.y, transform.position.z);

        // 平滑插值移动摄像机
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed);
    }
}
