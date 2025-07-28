using UnityEngine;
using UnityEngine.SceneManagement;

public class SuccessScreenUI : MonoBehaviour
{
    // ▼▼▼ 이 변수를 추가 ▼▼▼
    // 인스펙터 창에서 이동할 씬의 이름을 직접 입력받습니다.
    public string nextSceneName;

    // 버튼을 눌렀을 때 이 함수를 실행시킬 겁니다.
    public void LoadNextScene()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            Destroy(playerObject);
        }

        // ▼▼▼ 이 부분을 변수로 변경 ▼▼▼
        // 입력된 이름의 씬으로 이동합니다.
        SceneManager.LoadScene(nextSceneName);
    }
}