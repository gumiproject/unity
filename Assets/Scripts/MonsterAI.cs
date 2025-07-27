using UnityEngine;

public class MonsterAI : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 2f;

    [Header("감지 설정")]
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.6f;

    // --- Private 변수 ---
    private Rigidbody2D rb;
    private int moveDirection = -1;

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
        moveDirection *= -1;
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
    }

    // 플레이어와 충돌 시 데미지를 주는 함수
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // 부딪힌 오브젝트가 데미지를 받을 수 있는지 (IDamageable) 확인
            IDamageable damageableObject = collision.gameObject.GetComponent<IDamageable>();
            if (damageableObject != null)
            {
                // 넉백 방향 계산
                Vector2 knockbackDirection = (collision.transform.position - transform.position).normalized;
                knockbackDirection.y = Mathf.Max(knockbackDirection.y, 0.5f);
                
                // 플레이어의 TakeDamage 함수 호출
                damageableObject.TakeDamage(knockbackDirection);
            }
        }
    }

    // 디버깅을 위해 센서 위치와 감지 거리를 씬 뷰에 표시
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(groundCheck.position, (Vector2)groundCheck.position + Vector2.down * groundCheckDistance);
            
            Gizmos.color = Color.blue;
            Vector2 wallCheckPos = (Vector2)transform.position + new Vector2(moveDirection * 0.5f, 0);
            Gizmos.DrawLine(wallCheckPos, wallCheckPos + new Vector2(moveDirection, 0) * 0.1f);
        }
    }
}