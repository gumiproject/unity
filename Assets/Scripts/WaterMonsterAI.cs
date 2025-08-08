using UnityEngine;

public class WaterMonsterAI : MonoBehaviour
{
    [Header("이동 설정")]
    public float swimSpeed = 2f;
    [Tooltip("5초마다 방향을 바꿉니다.")]
    public float turnInterval = 5f;

    [Header("헤엄 효과 설정")]
    [Tooltip("위아래로 움직이는 속도")]
    public float verticalBobSpeed = 1f;
    [Tooltip("위아래로 움직이는 폭")]
    public float verticalBobAmount = 0.2f;

    // --- Private 변수 ---
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private int moveDirection = 1;
    private float turnTimer;
    private float initialYPosition;
    private bool isInWater = false; // [추가] 물 속에 있는지 확인하는 상태 변수
    private float originalGravityScale; // [추가] 원래 중력 값을 저장

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalGravityScale = rb.gravityScale; // [추가] 시작 시 중력 값 저장
        turnTimer = turnInterval;
        initialYPosition = transform.position.y;
    }

    void FixedUpdate()
    {
        // [핵심 수정] 물 속에 있을 때만 수영 로직을 실행합니다.
        if (isInWater)
        {
            // 1. 위아래로 헤엄치는 움직임 계산
            float verticalOffset = Mathf.Sin(Time.time * verticalBobSpeed) * verticalBobAmount;
            Vector2 targetPosition = new Vector2(transform.position.x, initialYPosition + verticalOffset);
            
            // 2. 좌우 이동과 위아래 움직임을 합쳐서 속도 설정
            rb.linearVelocity = new Vector2(moveDirection * swimSpeed, (targetPosition.y - transform.position.y) * 10f);

            // 3. 타이머 업데이트
            turnTimer += Time.deltaTime;
            if (turnTimer >= turnInterval)
            {
                Flip();
            }
        }
    }

    // 방향을 바꾸는 함수
    private void Flip()
    {
        moveDirection *= -1;
        spriteRenderer.flipX = (moveDirection == -1);
        turnTimer = 0f;
    }

    // 다른 콜라이더와 '물리적 충돌'이 일어났을 때
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 물속에 있고, 부딪힌 대상이 "Water" 레이어가 아니라면 (벽, 땅 등)
        if (isInWater && collision.gameObject.layer != LayerMask.NameToLayer("Water"))
        {
            Flip();
        }
        
        if (collision.gameObject.CompareTag("Player"))
        {
            IDamageable damageableObject = collision.gameObject.GetComponent<IDamageable>();
            if (damageableObject != null)
            {
                Vector2 knockbackDirection = (collision.transform.position - transform.position).normalized;
                knockbackDirection.y = Mathf.Max(knockbackDirection.y, 0.5f);
                damageableObject.TakeDamage(knockbackDirection);
            }
        }
    }

    // [추가] '트리거' 영역에 들어왔을 때와 나갔을 때를 감지
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Water"))
        {
            isInWater = true;
            rb.gravityScale = 0f; // 물에 들어가면 중력 비활성화
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Water"))
        {
            isInWater = false;
            rb.linearVelocity = Vector2.zero; // 물에서 나오면 속도 초기화
            rb.gravityScale = originalGravityScale; // 원래 중력으로 복구
        }
    }
}