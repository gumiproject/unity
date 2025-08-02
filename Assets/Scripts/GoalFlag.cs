using UnityEngine;
using UnityEngine.SceneManagement; 

public class GoalFlag : MonoBehaviour
{
    // 인스펙터 창에서 이동할 씬의 이름을 직접 입력받습니다.
    public string nextSceneName;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 들어온 오브젝트의 태그가 "Player"인지 확인합니다.
        if (other.CompareTag("Player"))
        {
            Debug.Log("플레이어가 골인 지점에 도착! DontDestroyOnLoad를 해제합니다.");

            // 1. DontDestroyOnLoad로 보호받고 있는 플레이어 오브젝트를 찾습니다.
            GameObject playerObject = PlayerController.instance.gameObject;

            if (playerObject != null)
            {
                // 2. [핵심] 플레이어를 DontDestroyOnLoad 씬에서 현재 활성화된 씬으로 다시 옮깁니다.
                // 이렇게 하면, 다음 씬이 로드될 때 이 플레이어는 정상적으로 파괴됩니다.
                SceneManager.MoveGameObjectToScene(playerObject, SceneManager.GetActiveScene());
            }

            // 3. 이제 플레이어가 파괴될 준비가 되었으니, 다음 씬을 불러옵니다.
            SceneManager.LoadScene(nextSceneName);
        }
    }
}