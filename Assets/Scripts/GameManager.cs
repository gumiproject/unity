using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 플레이어가 호출할 함수: "delay"초 후에 씬을 재시작함
    public void RestartSceneWithDelay(float delay)
    {
        StartCoroutine(RestartCoroutine(delay));
    }

    private IEnumerator RestartCoroutine(float delay)
    {
        // 1. 요청받은 시간(1초)만큼 기다립니다.
        yield return new WaitForSeconds(delay);

        // 2. 현재 씬을 다시 로드합니다.
        Debug.Log("게임 매니저가 씬을 재시작합니다.");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}