using UnityEngine;

using UnityEngine.SceneManagement;



public class GoalFlag : MonoBehaviour

{

    // 인스펙터 창에서 이동할 씬의 이름을 직접 입력받습니다.

    public string kim_succes1;



    // Is Trigger가 켜진 Collider에 다른 Collider가 들어왔을 때 자동으로 실행되는 함수입니다.

    private void OnTriggerEnter2D(Collider2D other)

    {

        // 들어온 오브젝트의 태그가 "Player"인지 확인합니다.

        if (other.CompareTag("Player"))

        {

            Debug.Log("플레이어가 도착했습니다!"); // 테스트용 로그



            // 입력된 이름의 씬으로 이동합니다.

            SceneManager.LoadScene(kim_succes1);

        }

    }

} 
