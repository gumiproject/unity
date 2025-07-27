using UnityEngine;

public class MonsterAI : MonoBehaviour
{
    public float moveSpeed = 2f;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private int moveDirection = 1; // 1은 오른쪽, -1은 왼쪽

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void FixedUpdate()
    {
        // 설정된 방향으로 몬스터를 계속 움직입니다.
        rb.velocity = new Vector2(moveDirection * moveSpeed, rb.velocity.y);
    }

    // 다른 콜라이더와 부딪혔을 때 호출되는 함수
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 부딪힌 오브젝트의 레이어가 "Ground"라면 (벽에 부딪혔다면)
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            // 부딪힌 방향을 확인하여 방향 전환이 필요한지 결정
            foreach (ContactPoint2D contact in collision.contacts)
            {
                // 오른쪽으로 가다가 오른쪽 벽에 부딪혔을 때
                if (moveDirection == 1 && contact.normal.x < -0.5f)
                {
                    Flip();
                    break;
                }
                // 왼쪽으로 가다가 왼쪽 벽에 부딪혔을 때
                else if (moveDirection == -1 && contact.normal.x > 0.5f)
                {
                    Flip();
                    break;
                }
            }
        }
    }

    // 방향을 바꾸는 함수
    private void Flip()
    {
        // 이동 방향을 반대로 바꿉니다.
        moveDirection *= -1;
        // 스프라이트의 좌우를 뒤집습니다.
        spriteRenderer.flipX = (moveDirection == -1);
    }   
}