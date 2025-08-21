using UnityEngine;
using UnityEngine.InputSystem;

public class GhostViewController : MonoBehaviour
{
    public static GhostViewController instance;
    private GameObject currentGhostImageObject;

    private void Awake()
    {
        // 올바른 싱글톤 패턴으로 수정
        if (instance != null && instance != this)
        {
            // 이 스크립트가 붙어있는 오브젝트(GameManager)를 파괴
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        // 이 오브젝트가 파괴되지 않도록 설정
        DontDestroyOnLoad(this.gameObject);
    }

    // 유령 이미지 오브젝트가 스스로를 등록할 때 호출할 함수
    public void RegisterGhostImage(GameObject ghostImage)
    {
        currentGhostImageObject = ghostImage;
        // 등록된 이미지가 있다면, 일단 비활성화
        if (currentGhostImageObject != null)
        {
            currentGhostImageObject.SetActive(false);
        }
    }

    void Update()
    {
        // 등록된 유령 이미지가 없으면 아무것도 하지 않음
        if (currentGhostImageObject == null) return;

        bool isMKeyPressed = Keyboard.current.mKey.isPressed;

        // 유령 이미지의 활성화 상태를 M키가 눌린 상태와 똑같이 만듦
        if (currentGhostImageObject.activeSelf != isMKeyPressed)
        {
            currentGhostImageObject.SetActive(isMKeyPressed);
        }
    }
}