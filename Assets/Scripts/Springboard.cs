using UnityEngine;

public class Springboard : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float jumpForce = 20f;
    // 애니메이터 컴포넌트를 담을 변수
    private Animator animator;
    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 부딪힌 오브젝트의 태그가 "Player"인지 확인
        if (other.CompareTag("Player"))
        {
            // 1. 애니메이터에게 "Jump" 신호를 보냄
            animator.SetTrigger("Jump");
            // 2. 플레이어 오브젝트를 실제로 점프시킴
            Rigidbody2D playerRb = other.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                // 플레이어의 기존 y축 속도를 없애고 새로운 힘을 가함
                playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, 0);
                playerRb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            }
        }
    }
}
