using UnityEngine;
using Unity.Cinemachine; // <--- 이 부분이 수정되었습니다.

public class CameraBoundarySetup : MonoBehaviour
{
    // 씬이 시작될 때 한번 호출됩니다.
    void Start()
    {
        // 씬에 있는 CinemachineCamera를 찾습니다. (최신 버전에선 이름이 바뀜)
        var vcam = FindObjectOfType<CinemachineCamera>();

        if (vcam != null)
        {
            // 그 카메라의 Confiner 2D 확장 기능을 가져옵니다.
            var confiner = vcam.GetComponent<CinemachineConfiner2D>();

            if (confiner != null)
            {
                // Confiner의 경계(Bounding Shape)를 이 스크립트가 붙어있는 오브젝트의
                // PolygonCollider2D로 설정합니다.
                confiner.BoundingShape2D = GetComponent<PolygonCollider2D>();
            }
        }
    }
}
