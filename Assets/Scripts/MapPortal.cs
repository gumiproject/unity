using UnityEngine;
using UnityEngine.SceneManagement; // 씬 관리를 위해 꼭 필요합니다!

public class MapPortal : MonoBehaviour
{
    // 인스펙터 창에서 이동할 씬의 이름을 직접 지정할 수 있습니다.
    public string sceneToLoad;

    // 플레이어가 트리거 안에 있는지 확인하기 위한 변수
    private bool playerIsInside = false;

    // 플레이어가 트리거 영역에 들어왔을 때 호출됩니다.
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 들어온 오브젝트가 "Player" 태그를 가지고 있는지 확인
        if (other.CompareTag("Player"))
        {
            playerIsInside = true;
        }
    }

    // 플레이어가 트리거 영역에서 나갔을 때 호출됩니다.
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsInside = false;
        }
    }

    // 매 프레임마다 키 입력을 확인합니다.
    private void Update()
    {
        // 만약 플레이어가 트리거 안에 있고, W키나 위쪽 화살표 키를 눌렀다면
        if (playerIsInside && (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)))
        {
            // 지정된 씬을 불러옵니다.
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}