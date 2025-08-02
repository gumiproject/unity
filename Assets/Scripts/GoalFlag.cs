using UnityEngine;
using UnityEngine.SceneManagement; 
using UnityEngine.InputSystem;

public class GoalFlag : MonoBehaviour
{
    [Header("설정")]
    public string nextSceneName; // 이동할 씬 이름
    public GameObject goalPromptUI; // 활성화할 UI 오브젝트

    // --- Private 변수 ---
    private bool playerIsInRange = false; // 플레이어가 범위 안에 있는지 여부

    void Start()
    {
        if (goalPromptUI != null)
        {
            goalPromptUI.SetActive(false);
        }
    }

    void Update()
    {
        // 플레이어가 범위 안에 있고, 점프 키(스페이스바)를 눌렀을 때
        if (playerIsInRange && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            // [핵심 수정] PlayerController 컴포넌트를 직접 찾아 함수를 호출합니다.
            if (PlayerController.instance != null)
            {
                PlayerController player = PlayerController.instance.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.PrepareForSceneChange();
                }
            }

            // 다음 씬 로드
            SceneManager.LoadScene(nextSceneName);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsInRange = true;
            if (goalPromptUI != null)
            {
                goalPromptUI.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsInRange = false;
            if (goalPromptUI != null)
            {
                goalPromptUI.SetActive(false);
            }
        }
    }
}