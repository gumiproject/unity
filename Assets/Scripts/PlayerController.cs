using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, IDamageable
{
    public static GameObject instance;

    #region Public 변수 (인스펙터 설정)
    [Header("사운드")]
    public AudioClip deathSound;
    public AudioClip jumpSound;
    public AudioClip fireSound;

    [Header("상태")]
    public int maxHealth = 3;

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

    [Header("충돌 확인")]
    public Transform groundCheck;
    public Vector2 groundCheckSize = new Vector2(0.8f, 0.1f);
    public LayerMask groundLayer;

    [Header("공격 설정")]
    public GameObject fireballPrefab;
    public Transform firePoint;
    public float fireCooldown = 0.1f;
    
    [Header("타기(Climbing)")]
    public float climbSpeed = 4f;

    [Header("수영 설정")]
    public float swimSpeed = 3f;
    public float swimJumpForce = 8f;
    public float maxSwimTime = 5f;
    public LayerMask waterLayer;
    #endregion

    #region Private 변수
    // --- 컴포넌트 변수 ---
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private PlayerInput playerInput;
    private AudioSource audioSource;
    private Collider2D playerCollider;

    // --- 상태 변수 ---
    private Vector2 moveInput;
    private Vector2 originalScale;
    private float originalGravityScale;
    private int currentHealth;
    private float swimTimeRemaining;
    private bool canDoubleJump = false;
    private bool isInvincible = false;
    private bool isDead = false;
    private bool isGrounded = true;
    private bool isRunning = false;
    private float doubleTapThreshold = 0.3f;
    private bool isDashing = false;
    private bool canFire = true;
    private bool canDash = true;
    private bool isCrouching = false;
    private bool isClimbing = false;
    private bool isSwimming = false;
    private bool isFacingRight = true;
    private float lastInputTime = 0f;
    private int lastDirection = 0;
    #endregion

    #region Unity 생명주기 함수 (Awake, Update, FixedUpdate)
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
        audioSource = GetComponent<AudioSource>();
        playerCollider = GetComponent<Collider2D>();
        originalScale = transform.localScale;
        originalGravityScale = rb.gravityScale;
        currentHealth = maxHealth;
        swimTimeRemaining = maxSwimTime;
    }

    void Update()
    {
        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);
        
        HandleClimbingAndSwimmingState();

        if (isDead || isDashing || isInvincible || isClimbing || isSwimming) return;

        HandleCrouchState();
        HandleFacingDirection();
        ApplyVisuals();
    }

    void FixedUpdate()
    {
        if (isDead || isDashing) return;

        if (isClimbing) HandleClimbingMovement();
        else if (isSwimming) HandleSwimmingMovement();
        else HandleNormalMovement();
    }
    #endregion

    #region 데미지 및 사망 처리
    public void TakeDamage(Vector2 knockbackDirection)
    {
        if (isInvincible || isDead) return;
        currentHealth--;
        if (currentHealth <= 0) Die();
        else StartCoroutine(InvincibilityCoroutine(knockbackDirection));
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        if (deathSound != null) audioSource.PlayOneShot(deathSound);
        StartCoroutine(DieSequence());
    }

    private IEnumerator DieSequence()
    {
        playerInput.enabled = false;
        GetComponent<Collider2D>().enabled = false;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.AddForce(Vector2.up * jumpForce * 0.7f, ForceMode2D.Impulse);
        yield return new WaitForSeconds(1.5f);
        GameManager.instance.RestartSceneWithDelay(0f);
        Destroy(gameObject);
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
    #endregion

    #region 물리 충돌 처리
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

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
    #endregion
    
    #region 핵심 로직 함수들
    private void HandleClimbingAndSwimmingState()
{
    bool isTouchingClimbable = playerCollider.IsTouchingLayers(LayerMask.GetMask("Climbable"));
    bool isTouchingWater = playerCollider.IsTouchingLayers(waterLayer);
    
    // 수영 상태 결정
    if (isTouchingWater &&!isSwimming)
    {
        isSwimming = true;
        isClimbing = false;
    }
    else if (!isTouchingWater && isSwimming)
    {
        isSwimming = false;
        swimTimeRemaining = maxSwimTime;
    }

    // 사다리 타기 상태 결정
    if (!isSwimming && isTouchingClimbable && Mathf.Abs(moveInput.y) > 0.1f)
    {
        isClimbing = true;
    }
    
    if (!isTouchingClimbable && isClimbing)
    {
        isClimbing = false;
    }
    
    // [수정된 핵심 로직]
    // 대쉬 상태를 최우선으로 확인하여, 대쉬 중에는 이 함수가 물리 상태를 변경하지 못하도록 합니다.
    if (isDashing)
    {
        // 대쉬 중에는 Dash() 코루틴이 중력을 0으로 제어하고 있으므로,
        // 이 함수에서는 아무런 물리 관련 작업을 수행하지 않고 즉시 반환합니다.
        // 이렇게 함으로써 Dash() 코루틴의 rb.gravityScale = 0f 설정이 유지됩니다.
        return; 
    }
    
    // 상태에 따른 물리 효과 적용
    if (isClimbing)
    {
        rb.gravityScale = 0f;
        rb.linearDamping = 0f; // 일관성을 위해 사다리에서도 Damping 초기화
    }
    else if (isSwimming)
    {
        rb.gravityScale = originalGravityScale * 0.4f;
        rb.linearDamping = 3f;
        
        swimTimeRemaining -= Time.deltaTime;
        if (swimTimeRemaining <= 0) Die();
    }
    else
    {
        // 기본 상태: 등반, 수영, 대쉬가 아닐 때만 실행됩니다.
        rb.gravityScale = originalGravityScale;
        rb.linearDamping = 0f;
    }

    Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Ground"), isClimbing);
}
    
    private void HandleCrouchState()
    {
        bool wantsToCrouch = playerInput.actions["Crouch"].IsPressed();
        if (wantsToCrouch && isGrounded) isCrouching = true;
        else if (!wantsToCrouch && CanStandUp()) isCrouching = false;
    }

    private void HandleFacingDirection()
    {
        if (isClimbing) return;
        if (moveInput.x > 0 && !isFacingRight) Flip();
        else if (moveInput.x < 0 && isFacingRight) Flip();
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 newScale = transform.localScale;
        newScale.x *= -1;
        transform.localScale = newScale;
    }

    private void ApplyVisuals()
    {
        float targetYScale = isCrouching ? originalScale.y * 0.5f : originalScale.y;
        transform.localScale = new Vector3(transform.localScale.x, targetYScale, transform.localScale.z);
        
        animator.SetBool("isMoving", moveInput.x != 0);
        animator.SetBool("isRunning", isRunning);
        animator.SetBool("isCrouching", isCrouching);
        animator.SetBool("isJumping", !isGrounded);
        animator.SetBool("isClimbing", isClimbing);
        animator.SetBool("isSwimming", isSwimming);
    }
    
    private void HandleNormalMovement()
    {
        float currentSpeed = moveSpeed;
        if (isRunning && !isCrouching) currentSpeed = runSpeed;
        if (isCrouching) currentSpeed = crouchSpeed;
        rb.linearVelocity = new Vector2(moveInput.x * currentSpeed, rb.linearVelocity.y);
    }

    private void HandleClimbingMovement()
    {
        float verticalInput = moveInput.y;
        float horizontalInput = moveInput.x;
        rb.linearVelocity = new Vector2(horizontalInput * (moveSpeed / 2), verticalInput * climbSpeed);
    }

    private void HandleSwimmingMovement()
    {
        rb.linearVelocity = moveInput * swimSpeed;
    }

    private bool CanStandUp()
    {
        return !Physics2D.OverlapBox(ceilingCheck.position, ceilingCheckSize, 0f, groundLayer);
    }
    #endregion
    
    #region Input System 이벤트 함수들
    public void OnMove(InputValue value)
    {
        if (isDead) { moveInput = Vector2.zero; return; }
        Vector2 input = value.Get<Vector2>();

        if (isClimbing || isSwimming)
        {
            moveInput = input;
            return;
        }
        
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
        if (isDead || !value.isPressed) return;
        
        if (isSwimming)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, swimJumpForce);
            return;
        }

        if (isClimbing)
        {
            isClimbing = false;
            if (jumpSound != null) audioSource.PlayOneShot(jumpSound);
            rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, jumpForce);
            return;
        }
        
        if (isGrounded && !isDashing && !isCrouching)
        {
            if (jumpSound != null) audioSource.PlayOneShot(jumpSound);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            animator.SetTrigger("jump");
        }
        else if (!isGrounded && canDoubleJump && !isDashing && !isCrouching)
        {
            if (jumpSound != null) audioSource.PlayOneShot(jumpSound);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            canDoubleJump = false;
            animator.SetTrigger("jump");
        }
    }

    public void OnDash(InputValue value)
    {
        if (isDead || isSwimming || isClimbing) return;
        if (value.isPressed && canDash && !isCrouching) StartCoroutine(Dash());
    }
    
    public void OnFire(InputValue value)
    {
        if (!canFire || isDead || isCrouching || isDashing || isClimbing || isSwimming || !value.isPressed)
        {
            return;
        }
        canFire = false;

        if (fireSound != null)
        {
            audioSource.PlayOneShot(fireSound);
        }
        
        GameObject fireballObject = Instantiate(fireballPrefab, firePoint.position, Quaternion.identity);
        Fireball fireball = fireballObject.GetComponent<Fireball>();
        if (fireball != null)
        {
            fireball.Launch(isFacingRight);
        }
        StartCoroutine(FireCooldownCoroutine());
    }
    
    // [추가] 씬 전환 직전에 GoalFlag가 호출할 함수
    public void PrepareForSceneChange()
    {
        SceneManager.MoveGameObjectToScene(this.gameObject, SceneManager.GetActiveScene());
    }
    #endregion

    #region 코루틴 (Coroutines)
    public void ActivateDoubleJump()
    {
        canDoubleJump = true;
    }
    
    private IEnumerator FireCooldownCoroutine()
    {
        yield return new WaitForSeconds(fireCooldown);
        canFire = true;
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        isRunning = false;
        isClimbing = false;
        animator.SetBool("isDashing", true);
        
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        
        rb.linearVelocity = new Vector2(transform.localScale.x > 0 ? dashPower : -dashPower, 0f);

        yield return new WaitForSeconds(dashingTime);
        
        rb.gravityScale = originalGravity;
        isDashing = false;
        animator.SetBool("isDashing", false);
        
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }
    #endregion
    
    #region 디버깅용 Gizmo
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
        }
        if (ceilingCheck != null)
        {
            Gizmos.color = CanStandUp() ? Color.red : Color.green;
            Gizmos.DrawWireCube(ceilingCheck.position, ceilingCheckSize);
        }
    }
    #endregion
}