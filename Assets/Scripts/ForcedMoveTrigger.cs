using UnityEngine;

public class ForcedMoveTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 부딪힌 오브젝트가 "Player" 태그를 가지고 있는지 확인
        if (other.CompareTag("Player"))
        {
            // 플레이어의 PlayerController 스크립트를 가져옴
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                // PlayerController에 있는 강제 이동 시작 함수를 호출 (2단계에서 만들 예정)
                player.StartForcedMove(transform.position.y);
            }

            // 한 번 발동되면 비활성화하여 중복 실행 방지
            gameObject.SetActive(false);
        }
    }
}