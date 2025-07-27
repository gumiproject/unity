using UnityEngine;
using UnityEngine.SceneManagement;

public class SuccessScreenUI : MonoBehaviour
{
    // 버튼을 눌렀을 때 이 함수를 실행시킬 겁니다.
    public void LoadNextScene()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        // 2. 만약 playerObject를 찾았다면, 그 오브젝트를 파괴합니다.
        if (playerObject != null)
        {
            Destroy(playerObject);
        }

        // 3. 모든 준비가 끝났으니 다음 씬을 불러옵니다.
        SceneManager.LoadScene("semple_post"); // 실제 다음 씬 이름으로 변경하세요.
    }
}