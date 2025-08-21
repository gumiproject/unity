using UnityEngine;
using UnityEngine.InputSystem;

public class GhostViewController : MonoBehaviour
{
    // �ν����Ϳ��� ���� �� ���� �̹��� ������Ʈ�� �������ݴϴ�.
    public GameObject ghostImageObject;

    void Update()
    {
        // 'M' Ű�� ������ ������ �����մϴ�.
        if (Keyboard.current.mKey.wasPressedThisFrame)
        {
            if (ghostImageObject != null)
            {
                // ���� �̹����� ���� Ȱ��ȭ ���¸� ������ŵ�ϴ�.
                // (���������� �Ѱ�, ���������� ���ϴ�)
                ghostImageObject.SetActive(!ghostImageObject.activeSelf);
            }
        }
    }
}
