using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public int deathCount = 0;
    public int maxdeathCount = 3; // 최대 사망 횟수
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
        deathCount++;
        // 1. 요청받은 시간(1초)만큼 기다립니다.
        yield return new WaitForSeconds(delay);
        Debug.Log("현재 사망 횟수: " + deathCount);
        if (deathCount > maxdeathCount)
        {
            SceneManager.LoadScene("Start");
            Destroy(gameObject);
        }
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}