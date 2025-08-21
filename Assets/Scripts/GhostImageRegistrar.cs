using UnityEngine;

public class GhostImageRegistrar : MonoBehaviour
{
    // 인스펙터 창에서 비활성화된 유령 이미지를 직접 연결합니다.
    public GameObject ghostImageToRegister;

    void Start()
    {
        // GameManager에 있는 GhostViewController를 찾아,
        // 연결된 유령 이미지를 등록해달라고 요청합니다.
        if (GhostViewController.instance != null && ghostImageToRegister != null)
        {
            GhostViewController.instance.RegisterGhostImage(ghostImageToRegister);
        }
    }
}
