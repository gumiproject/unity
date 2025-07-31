using UnityEngine;

public class DoubleJumpItem : MonoBehaviour
{
    // 아이템을 먹었을 때 재생할 사운드 (선택 사항)
    public AudioClip pickupSound;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 닿은 대상이 플레이어일 경우
        if (other.CompareTag("Player"))
        {
            // 플레이어 컨트롤러를 찾아서 2단 점프 활성화 함수를 호출
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.ActivateDoubleJump();

                // 사운드가 있다면 재생
                if (pickupSound != null)
                {
                    AudioSource.PlayClipAtPoint(pickupSound, transform.position);
                }

                // 아이템 오브젝트 자신을 파괴
                Destroy(gameObject);
            }
        }
    }
}