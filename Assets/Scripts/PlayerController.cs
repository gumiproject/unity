using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, IDamageable
{
    public static GameObject instance;

    [Header("사운드")]
    public AudioClip deathSound; // 사망 시 재생할 오디오 클립
    public AudioClip jumpSound; // 점프 시 재생할 오디오 클립


    [Header("상태")]
    public int maxHealth = 3;
    private int currentHealth;
    private bool isInvincible = false;

    [Header("움직임")]
    public float moveSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpForce = 12f;
    public float knockbackForce = 10f;
    public float invincibilityDuration = 1.5f;

    [Header("대쉬")]
    public float dashPower = 24f;
    public float dashingTime = 0.2f;
    public float dashingCooldown = 1f;

    [Header("엎드리기")]
    public float crouchSpeed = 2.5f;
    public Transform ceilingCheck;
    public Vector2 ceilingCheckSize = new Vector2(0.8f, 0.1f);
    public LayerMask groundLayer;

    // --- Private 변수 ---
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private PlayerInput playerInput;
    private Vector2 moveInput;
    private Vector2 originalScale;

    // --- 상태 변수 ---
    private bool isDead = false;
    private bool isGrounded = true;
    private bool isRunning = false;
    private float doubleTapThreshold = 0.3f;
    private bool isDashing = false;
    private bool canDash = true;
    private bool isCrouching = false;
    private bool isFacingRight = true;
    private float lastInputTime = 0f;
    private int lastDirection = 0;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this.gameObject;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this.gameObject)
        {
            Destroy(gameObject);
            return;
        }

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerInput = GetComponent<PlayerInput>();
        originalScale = transform.localScale;
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (isDead || isDashing || isInvincible) return;

        HandleCrouchState();
        HandleFacingDirection();
        ApplyVisuals();
    }

    void FixedUpdate()
    {
        if (isDead || isDashing) return;
        HandleMovement();
    }

    public void TakeDamage(Vector2 knockbackDirection)
    {
        if (isInvincible || isDead) return;

        currentHealth--;
        Debug.Log("플레이어 체력: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvincibilityCoroutine(knockbackDirection));
        }
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log("플레이어 사망!");
        
    // [추가] 사망 사운드가 지정되어 있다면, 현재 위치에서 재생합니다.
    if (deathSound != null)
    {
        AudioSource.PlayClipAtPoint(deathSound, transform.position);
    }

    StartCoroutine(DieSequence());
    }

   private IEnumerator DieSequence()
{
 // 1. 모든 물리적 움직임과 입력을 중단합니다.
 playerInput.enabled = false;
 rb.linearVelocity = Vector2.zero;
 rb.isKinematic = false; // 물리 효과 활성화

 // 2. 위로 솟아오르는 힘을 한 번 줍니다.
 rb.AddForce(Vector2.up * jumpForce * 0.7f, ForceMode2D.Impulse);

 // 3. 잠시 기다립니다. (얼마나 올라갈지 조절)
 yield return new WaitForSeconds(0.3f); // 이 시간을 조절해서 올라가는 높이를 변경하세요.

 // 4. Y축 속도를 반전시켜 아래로 떨어지게 만듭니다.
 rb.linearVelocity = new Vector2(rb.linearVelocity.x, -rb.linearVelocity.y * 0.5f); // * 0.5f는 감속 효과

 // 5. 회전 (180도) - 스프라이트의 방향을 뒤집습니다.
 isFacingRight = !isFacingRight;
 ApplyVisuals();

 // 6. 콜라이더를 비활성화하여 다른 오브젝트와 충돌하지 않도록 합니다.
 GetComponent<Collider2D>().enabled = false;

 // 7. 게임 매니저에게 씬 재시작을 1.5초 후에 하라고 명령을 보냅니다.
 GameManager.instance.RestartSceneWithDelay(0.7f);

 // 8. 플레이어 오브젝트를 1.5초 후에 파괴하도록 예약합니다.
 Destroy(gameObject, 0.5f);

 yield return null; // 코루틴 종료
}


    private IEnumerator InvincibilityCoroutine(Vector2 knockbackDirection)
    {
        isInvincible = true;
        if (knockbackDirection.magnitude > 0)
        {
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        }

        float endTime = Time.time + invincibilityDuration;
        while (Time.time < endTime)
        {
            spriteRenderer.enabled = false;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.enabled = true;
            yield return new WaitForSeconds(0.1f);
        }
        isInvincible = false;
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f && ((1 << collision.gameObject.layer) & groundLayer) != 0)
            {
                isGrounded = true; 
                break;
            }
        }
        if (collision.gameObject.CompareTag("Enemy"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y < -0.5f)
                {
                    Destroy(collision.gameObject);
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * 0.7f);
                    return;
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            isGrounded = false;
        }
    }

    private void HandleCrouchState()
    {
        bool wantsToCrouch = playerInput.actions["Crouch"].IsPressed();
        if (wantsToCrouch && isGrounded)
        {
            isCrouching = true;
        }
        else if (!wantsToCrouch)
        {
            if (CanStandUp())
            {
                isCrouching = false;
            }
        }
    }

    private void HandleFacingDirection()
    {
        if (moveInput.x > 0) isFacingRight = true;
        else if (moveInput.x < 0) isFacingRight = false;
    }

    private void ApplyVisuals()
    {
        float targetYScale = isCrouching ? originalScale.y * 0.5f : originalScale.y;
        float targetXScale = isFacingRight ? originalScale.x : -originalScale.x;
        transform.localScale = new Vector3(targetXScale, targetYScale, transform.localScale.z);
        animator.SetBool("isMoving", moveInput.x != 0);
        animator.SetBool("isRunning", isRunning);
        animator.SetBool("isCrouching", isCrouching);
        animator.SetBool("isJumping", !isGrounded);
    }

    private void HandleMovement()
    {
        float currentSpeed = moveSpeed;
        if (isRunning && !isCrouching) currentSpeed = runSpeed;
        if (isCrouching) currentSpeed = crouchSpeed;
        rb.linearVelocity = new Vector2(moveInput.x * currentSpeed, rb.linearVelocity.y);
    }

    private bool CanStandUp()
    {
        return !Physics2D.OverlapBox(ceilingCheck.position, ceilingCheckSize, 0f, groundLayer);
    }

    public void OnMove(InputValue value)
    {
        if (isDead) { moveInput = Vector2.zero; return; }
        Vector2 input = value.Get<Vector2>();
        if (isCrouching || isDashing || isInvincible)
        {
            isRunning = false;
        }
        else if (input.x != 0)
        {
            int currentDirection = input.x > 0 ? 1 : -1;
            if (currentDirection == lastDirection && Time.time - lastInputTime < doubleTapThreshold)
            {
                isRunning = true;
            }
            lastInputTime = Time.time;
            lastDirection = currentDirection;
        }
        else
        {
            isRunning = false;
        }
        moveInput = input;
    }

    public void OnJump(InputValue value)
    {
        if (isDead) return;
        if (value.isPressed && isGrounded && !isDashing && !isCrouching)
        {
                if (jumpSound != null)
    {
        AudioSource.PlayClipAtPoint(jumpSound, transform.position);
    }
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isGrounded = false;
            animator.SetTrigger("jump");
        }
    }

    public void OnDash(InputValue value)
    {
        if (isDead) return;
        if (value.isPressed && canDash && !isCrouching)
        {
            StartCoroutine(Dash());
        }
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        isRunning = false;
        animator.SetBool("isDashing", true);
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2((isFacingRight ? 1 : -1) * dashPower, 0f);
        yield return new WaitForSeconds(dashingTime);
        rb.gravityScale = originalGravity;
        isDashing = false;
        animator.SetBool("isDashing", false);
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }
}