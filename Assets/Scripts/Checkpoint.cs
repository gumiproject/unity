// Checkpoint.cs

using UnityEngine;
using UnityEngine.SceneManagement; // SceneManager를 사용하기 위해 추가

public class Checkpoint : MonoBehaviour
{
    private bool isActivated = false; // 중복 저장을 방지하기 위한 플래그

    // 플레이어가 콜라이더 영역 안으로 들어왔을 때 호출됩니다.
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 들어온 오브젝트가 "Player" 태그를 가지고 있고, 아직 활성화되지 않았다면
        if (other.CompareTag("Player") && !isActivated)
        {
            isActivated = true;
            
            // GameManager에 현재 체크포인트의 위치와 씬 이름을 전달하여 저장합니다.
            GameManager.instance.UpdateRespawnPoint(transform.position, SceneManager.GetActiveScene().name);

            // [선택 사항] 체크포인트가 활성화되었다는 시각적 피드백
            // 예: 색깔을 녹색으로 변경
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = Color.green;
            }
        }
    }
}