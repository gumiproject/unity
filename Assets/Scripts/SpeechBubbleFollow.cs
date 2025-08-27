using UnityEngine;

public class SpeechBubbleFollow : MonoBehaviour
{
    public Transform target; // 따라갈 대상 (플레이어)
    public Vector3 offset;   // 플레이어 머리 위로 띄울 거리

    // LateUpdate는 target(플레이어)이 모든 움직임을 마친 후에 실행되어 떨림을 방지합니다.
    void LateUpdate()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }
}