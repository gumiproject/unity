using UnityEngine;

public class MonsterAI : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 2f;

    [Header("감지 설정")]
    public Transform groundCheck; // 땅을 감지할 센서의 위치
    public LayerMask groundLayer; // 땅으로 인식할 레이어
    public float groundCheckDistance = 0.6f; // 센서의 감지 거리

    // --- Private 변수 ---
    private Rigidbody2D rb;
    private int moveDirection = -1; // 1은 오른쪽, -1은 왼쪽

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        // 1. 앞에 땅이 있는지 확인 (절벽 감지)
        Vector2 groundCheckPos = groundCheck.position;
        RaycastHit2D groundHit = Physics2D.Raycast(groundCheckPos, Vector2.down, groundCheckDistance, groundLayer);

        // 2. 진행 방향에 벽이 있는지 확인 (벽 감지)
        Vector2 wallCheckPos = (Vector2)transform.position + new Vector2(moveDirection * 0.5f, 0);
        RaycastHit2D wallHit = Physics2D.Raycast(wallCheckPos, new Vector2(moveDirection, 0), 0.1f, groundLayer);

        // 만약 앞에 땅이 없거나(절벽), 앞에 벽이 있다면 방향 전환
        if (groundHit.collider == null || wallHit.collider != null)
        {
            Flip();
        }

        // 3. 결정된 방향으로 몬스터 이동
        rb.linearVelocity = new Vector2(moveDirection * moveSpeed, rb.linearVelocity.y);
    }

    // 방향을 바꾸는 함수
    private void Flip()
    {
        // 이동 방향을 반대로 바꿉니다.
        moveDirection *= -1;
        // 몬스터의 좌우를 뒤집습니다.
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
    }

    // 디버깅을 위해 센서 위치와 감지 거리를 씬 뷰에 표시합니다.
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            // 땅 감지 센서 위치에서 아래로 선을 그립니다.
            Gizmos.DrawLine(groundCheck.position, (Vector2)groundCheck.position + Vector2.down * groundCheckDistance);

            Gizmos.color = Color.blue;
            // 벽 감지 센서 위치에서 앞으로 선을 그립니다.
            Vector2 wallCheckPos = (Vector2)transform.position + new Vector2(moveDirection * 0.5f, 0);
            Gizmos.DrawLine(wallCheckPos, wallCheckPos + new Vector2(moveDirection, 0) * 0.1f);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // 부딪힌 오브젝트의 태그가 "Player"인지 확인합니다.
        if (collision.gameObject.CompareTag("Player"))
        {
            // 플레이어에게 데미지를 줄 수 있는지 (IDamageable) 확인합니다.
            IDamageable damageableObject = collision.gameObject.GetComponent<IDamageable>();
            if (damageableObject != null)
            {
                // 넉백 방향을 계산합니다.
                Vector2 knockbackDirection = (collision.transform.position - transform.position).normalized;
                knockbackDirection.y = Mathf.Max(knockbackDirection.y, 0.5f);

                // 플레이어의 TakeDamage 함수를 호출합니다.
                damageableObject.TakeDamage(knockbackDirection);
            }
        }
    }
}