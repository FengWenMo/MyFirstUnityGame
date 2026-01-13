using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CameraFollow : MonoBehaviour
{
    public Transform target; // 要跟随的目标（你的玩家角色）
    public float smoothSpeed = 0.125f; // 跟随平滑度，值越大跟随越紧
    public Vector3 offset; // 摄像机与目标的偏移量

//     void LateUpdate()
//     {
//         // 计算摄像机期望的位置：目标位置 + 偏移
//         Vector3 desiredPosition = target.position + offset;
//         // 使用插值让摄像机平滑地移动到期望位置
//         Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
//         // 更新摄像机的位置
//         transform.position = smoothedPosition;

//         // 确保摄像机的Z轴保持固定（2D游戏通常为-10）
//         transform.position = new Vector3(transform.position.x, transform.position.y, -10f);
//     }
// }
private Vector3 velocity = Vector3.zero; // 声明一个引用变量
public float smoothTime = 0.1f; // 达到目标大致所需时间，可调整

void LateUpdate()
{
    Vector3 targetPosition = target.position + offset;
    transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
}
}
