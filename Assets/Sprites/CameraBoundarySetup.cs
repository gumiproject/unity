using UnityEngine;
using Cinemachine; // 시네머신을 사용하기 위해 꼭 필요합니다!

public class CameraBoundarySetup : MonoBehaviour
{
    // 씬이 시작될 때 한번 호출됩니다.
    void Start()
    {
        // 씬에 있는 CinemachineVirtualCamera를 찾습니다.
        CinemachineVirtualCamera vcam = FindObjectOfType<CinemachineVirtualCamera>();

        if (vcam != null)
        {
            // 그 카메라의 Confiner 2D 확장 기능을 가져옵니다.
            CinemachineConfiner2D confiner = vcam.GetCinemachineComponent<CinemachineConfiner2D>();

            if (confiner != null)
            {
                // Confiner의 경계(Bounding Shape)를 이 스크립트가 붙어있는 오브젝트의
                // PolygonCollider2D로 설정합니다.
                confiner.m_BoundingShape2D = GetComponent<PolygonCollider2D>();
            }
        }
    }
}

