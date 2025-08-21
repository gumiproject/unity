using UnityEngine;
using UnityEngine.InputSystem;

public class GhostViewController : MonoBehaviour
{
    // 인스펙터에서 껐다 켤 유령 이미지 오브젝트를 연결해줍니다.
    public GameObject ghostImageObject;

    void Update()
    {
        // 'M' 키가 눌리는 순간을 감지합니다.
        if (Keyboard.current.mKey.wasPressedThisFrame)
        {
            if (ghostImageObject != null)
            {
                // 유령 이미지의 현재 활성화 상태를 반전시킵니다.
                // (꺼져있으면 켜고, 켜져있으면 끕니다)
                ghostImageObject.SetActive(!ghostImageObject.activeSelf);
            }
        }
    }
}
