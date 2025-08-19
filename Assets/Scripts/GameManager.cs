// GameManager.cs

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    // [수정] respawnSceneName은 public일 필요가 없어졌습니다.
    public string respawnSceneName; 
    
    // [추가] 리스폰 위치와 체크포인트 저장 여부 변수
    public Vector3 respawnPoint { get; private set; }
    public bool isRespawnPointSet { get; private set; } = false;

    public int deathCount = 0;
    public int maxdeathCount = 3;

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

    // [추가] 체크포인트가 호출할 함수
    public void UpdateRespawnPoint(Vector3 newPosition, string sceneName)
    {
        respawnPoint = newPosition;
        respawnSceneName = sceneName;
        isRespawnPointSet = true;
        Debug.Log("체크포인트 저장 완료! 씬: " + sceneName + ", 위치: " + newPosition);
    }
    
    public string GetRespawnSceneName()
    {
        return respawnSceneName;
    }

    public void RestartSceneWithDelay(float delay)
    {
        StartCoroutine(RestartCoroutine(delay));
    }

    private IEnumerator RestartCoroutine(float delay)
    {
        deathCount++;
        yield return new WaitForSeconds(delay);

        Debug.Log("현재 사망 횟수: " + deathCount);
        if (deathCount >= maxdeathCount)
        {
            // [수정] 게임 오버 시 DontDestroyOnLoad 해제 및 상태 초기화 고려
            SceneManager.LoadScene("Start");
            Destroy(gameObject); // GameManager도 파괴
        }
        else
        {
            // [수정] 저장된 respawnSceneName으로 씬 로드
            SceneManager.LoadScene(respawnSceneName);
        }
    }
}