using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerController : MonoBehaviour, IDamageable
{
    public static GameObject instance;

    #region Public 변수 (인스펙터 설정)
    [Header("사운드")]
    public AudioClip deathSound;
    public AudioClip jumpSound;
    public AudioClip fireSound;
    public AudioClip damageSound;

    [Header("말풍선 설정")]
    public GameObject speechBubble; // 2단계에서 만든 SpeechBubble 오브젝트
    public TextMeshProUGUI dialogueText;  // SpeechBubble의 자식인 TextMeshPro 오브젝트

    [Header("상태")]
    public int maxHealth = 3;


    [Header("움직임")]
    public float moveSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpForce = 12f;
    public float knockbackForce = 10f;
    public float invincibilityDuration = 1.5f;
    public float jumpCooldown = 0.2f; // [추가] 일반 점프 쿨타임

    [Header("대쉬")]
    public float dashPower = 24f;
    public float dashingTime = 0.2f;
    public float dashingCooldown = 1f;
    private float oriDashingCooldown;

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
    public float swimJumpCooldown = 1f; // [추가] 수중 점프 쿨타임

    [Header("독 설정")]
    public float poisonDamageInterval = 1f; // 1초마다 데미지
    public LayerMask poisonLayer; // [추가] 독 지형 레이어
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
    private float poisonDamageTimer; // [추가] 독 데미지 타이머
    private bool canJump = true; // [추가] 현재 점프 가능한지 여부
    private bool isWaterJumping = false;
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
    private bool isInPoison = false; // [추가] 독 지형에 있는지 여부
    private bool isFacingRight = true;
    private float lastInputTime = 0f;
    private int lastDirection = 0;
    private bool isForceMoving = false;
    private bool isDashPowerUpActive = false; // 대시 강화 아이템 효과가 활성화되었는지 여부
    private float dashPowerUpStartTime = 0f;  // 대시 강화 아이템을 먹은 시간
    private int deathCount = 0; // [추가] 사망 횟수 카운트

    private int dashPowerUpState = 0;
    private float firstDashTime = 0f; // 첫 번째 대시를 사용한 시간
    #endregion


    #region Unity 생명주기 함수 (Awake, Update, FixedUpdate)
    void Awake()
    {


        if (instance == null)
        {
            instance = this.gameObject;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded; // 씬 로드 이벤트에 등록
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
        oriDashingCooldown = dashingCooldown;
        if (GameManager.instance != null && GameManager.instance.isRespawnPointSet)
        {
            transform.position = GameManager.instance.respawnPoint;
        }
        //StartCoroutine(InitializeUIWithDelay());
    }

    private IEnumerator InitializeUIWithDelay()
    {
        // 0.1초 기다립니다.
        yield return new WaitForSeconds(0.1f);

        // UIManager가 준비되었는지 확인 후 UI 업데이트
        if (UIManager.instance != null)
        {
            UIManager.instance.UpdateHealthUI(currentHealth);
            UIManager.instance.SetDoubleJumpIconActive(false);
        }
    }

    void Update()
    {

        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);

        HandleClimbingAndSwimmingState();

        if (isDead || isDashing || isInvincible || isClimbing || isSwimming || isForceMoving || isInPoison) return;

        HandleCrouchState();
        HandleFacingDirection();
        ApplyVisuals();


    }

    void FixedUpdate()
    {
        if (isDead || isDashing || isForceMoving) return;

        if (isClimbing) HandleClimbingMovement();
        else if (isSwimming || isInPoison) HandleSwimmingMovement();
        else HandleNormalMovement();
    }
    #endregion

    #region 데미지 및 사망 처리
    public void TakeDamage(Vector2 knockbackDirection)
    {
        if (isInvincible || isDead) return;
        currentHealth--;
        if (damageSound != null) audioSource.PlayOneShot(damageSound);
        UIManager.instance.UpdateHealthUI(currentHealth);
        if (currentHealth <= 0) Die();
        else StartCoroutine(InvincibilityCoroutine(knockbackDirection));
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        UIManager.instance.UpdateHealthUI(0);
        if (deathSound != null) audioSource.PlayOneShot(deathSound);
        StartCoroutine(DieSequence());
    }

    private IEnumerator DieSequence()
    {
        // 1. 모든 입력을 즉시 중단합니다.
        playerInput.enabled = false;

        // 2. 다른 오브젝트와 충돌하지 않도록 콜라이더를 비활성화합니다.
        GetComponent<Collider2D>().enabled = false;

        // 3. [핵심 수정] 물리적 움직임을 완전히 초기화합니다.
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Static; // 모든 물리 효과를 완전히 끔

        // 물리 상태가 업데이트될 때까지 한 프레임 기다립니다.
        yield return new WaitForFixedUpdate();

        // 4. 다시 물리 효과를 켜서 튀어 오를 준비를 합니다.
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.AddForce(Vector2.up * jumpForce * 0.7f, ForceMode2D.Impulse); // 위로 튀어 오릅니다.

        // 5. 씬 재시작 및 오브젝트 파괴를 예약합니다.
        if (deathCount > 3)
        {
            Destroy(gameObject, 1.5f);
        }
        else
        {
            GameManager.instance.RestartSceneWithDelay(1.5f);
            Destroy(gameObject, 1.5f);
        }
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

        if (isForceMoving)
        {
            // 1. 몬스터("Enemy" 태그)와 부딪혔을 경우
            if (collision.gameObject.CompareTag("Enemy"))
            {
                isForceMoving = false;
                Die();
            }
            // 2. 땅(groundLayer)과 부딪혔을 경우 (Tilemap)
            // groundLayer에 포함된 레이어와 부딪혔는지 비트 연산으로 확인
            else if (((1 << collision.gameObject.layer) & groundLayer) != 0)
            {
                // 강제 이동 상태를 해제 (이렇게 하면 코루틴의 while 루프가 중단됨)
                isForceMoving = false;
            }
            return; // 강제 이동 중에는 아래의 일반 충돌 로직을 실행하지 않음
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
    #endregion

    #region 핵심 로직 함수들
    private void HandleClimbingAndSwimmingState()
    {
        if (isDashing)
        {
            // 대쉬 중에는 Dash() 코루틴이 중력을 0으로 제어하고 있으므로,
            // 이 함수에서는 아무런 물리 관련 작업을 수행하지 않고 즉시 반환합니다.
            // 이렇게 함으로써 Dash() 코루틴의 rb.gravityScale = 0f 설정이 유지됩니다.
            return;
        }
        bool isTouchingClimbable = playerCollider.IsTouchingLayers(LayerMask.GetMask("Climbable"));
        bool isTouchingWater = playerCollider.IsTouchingLayers(waterLayer);
        bool isTouchingPoison = playerCollider.IsTouchingLayers(poisonLayer); // [추가]

        // 상태 결정 (우선순위: 독 > 물 > 사다리)
        isInPoison = isTouchingPoison;
        isSwimming = isTouchingWater && !isInPoison;
        isClimbing = isTouchingClimbable && !isSwimming && !isInPoison;

        if (isClimbing)
        {
            rb.gravityScale = 0f;
            rb.linearDamping = 0f;
        }
        else if (isSwimming || isInPoison) // [수정] 독에서도 수영과 같은 물리 효과
        {
            rb.gravityScale = originalGravityScale * 0.4f;
            rb.linearDamping = 3f;

            if (isSwimming)
            {
                swimTimeRemaining -= Time.deltaTime;
                if (swimTimeRemaining <= 0) Die();
            }
            else if (isInPoison)
            {
                // 독 데미지 처리
                poisonDamageTimer -= Time.deltaTime;
                if (poisonDamageTimer <= 0)
                {
                    TakeDamage(Vector2.zero); // 넉백 없이 데미지만 받음
                    poisonDamageTimer = poisonDamageInterval; // 타이머 초기화
                }
            }

        }
        else
        {
            rb.gravityScale = originalGravityScale;
            rb.linearDamping = 0f;
            swimTimeRemaining = maxSwimTime; // 물에서 나오면 잠수 시간 초기화
            poisonDamageTimer = 0; // 독에서 나오면 데미지 타이머 초기화
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
        // 물에서 점프하여 솟아오르는 중일 때
        if (isWaterJumping)
        {
            // Y축 속도가 0 이하로 떨어지면 (정점에 도달했거나 하강 중이면) 점프 상태 해제
            if (rb.linearVelocity.y <= 0)
            {
                isWaterJumping = false;
            }
            // 솟아오르는 동안에는 X축(좌우) 이동만 허용하고, Y축 속도는 그대로 둠
            rb.linearVelocity = new Vector2(moveInput.x * swimSpeed, rb.linearVelocity.y);
        }
        else
        {
            // 평소 수영 상태에서는 모든 방향으로 자유롭게 이동
            rb.linearVelocity = moveInput * swimSpeed;
        }
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
        // 쿨타임 중이거나, 죽었거나, 버튼을 누른 순간이 아니면 점프 불가
        if (!canJump || isDead || !value.isPressed) return;

        // 수영 중 점프 (위로 떠오르기)
        if (isSwimming || isInPoison)
        {
            canJump = false; // 점프 후 즉시 쿨타임 시작
            isWaterJumping = true; // [핵심 수정] 물 점프 상태로 전환합니다.
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, swimJumpForce);
            StartCoroutine(JumpCooldownCoroutine());
            return;
        }

        // 사다리 타다 점프
        if (isClimbing)
        {
            canJump = false;
            isClimbing = false;
            if (jumpSound != null) audioSource.PlayOneShot(jumpSound);
            rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, jumpForce);
            StartCoroutine(JumpCooldownCoroutine());
            return;
        }

        // 지상 점프
        if (isGrounded && !isDashing && !isCrouching)
        {
            canJump = false;
            if (jumpSound != null) audioSource.PlayOneShot(jumpSound);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            animator.SetTrigger("jump");
            StartCoroutine(JumpCooldownCoroutine());
        }
        // 2단 점프
        else if (!isGrounded && canDoubleJump && !isDashing && !isCrouching)
        {
            canJump = false;
            if (jumpSound != null) audioSource.PlayOneShot(jumpSound);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            canDoubleJump = false;
            UIManager.instance.SetDoubleJumpIconActive(false);
            animator.SetTrigger("jump");
            StartCoroutine(JumpCooldownCoroutine());
        }
    }

    public void OnDash(InputValue value)
    {
        if (isDead || isSwimming || isClimbing) return;
        if (value.isPressed && canDash && !isCrouching) StartCoroutine(Dash());
    }

    public void OnFire(InputValue value)
    {
        if (!canFire || isDead || isCrouching || isDashing || isClimbing || isSwimming || !value.isPressed || isInPoison)
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
        UIManager.instance.SetDoubleJumpIconActive(true);
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
        // --- 새로운 동적 쿨타임 판정 로직 ---

        // [상태 1] 아이템 먹고 "첫 번째 대시"를 사용한 경우
        if (dashPowerUpState == 1)
        {
            // 2단계: '두 번째 대시'를 기다리는 상태로 변경
            dashPowerUpState = 2;
            // 시간 측정을 위해 첫 대시 사용 시간을 기록
            firstDashTime = Time.time;
            // 두 번째 대시도 즉시 사용할 수 있도록 쿨타임은 0으로 유지
            Debug.Log("첫 강화 대시 사용! 두 번째 대시 타이머가 시작됩니다.");
        }
        // [상태 2] 첫 대시 후 "두 번째 대시"를 사용한 경우 (결판의 시간)
        else if (dashPowerUpState == 2)
        {
            // 첫 대시와 두 번째 대시 사이의 시간 간격을 계산
            float timeBetweenDashes = Time.time - firstDashTime;

            // [조건 분기 1] 시간 간격이 원래 쿨타임보다 짧았다면 (빠르게 연계)
            if (timeBetweenDashes <= oriDashingCooldown)
            {
                // 쿨타임을 원래대로 복구
                dashingCooldown = oriDashingCooldown;
                Debug.Log("빠른 연계 성공! 대시 쿨타임이 원래대로 돌아옵니다.");
                UIManager.instance.SetDoubleDashIconActive(false);
            }
            // [조건 분기 2] 시간 간격이 원래 쿨타임보다 길었다면 (느리게 사용)
            else
            {
                // 쿨타임 0초 유지를 위해 값을 그대로 둡니다.
                Debug.Log("느린 연계! 대시 쿨타임 0초 효과가 유지됩니다.");
            }

            // 모든 시퀀스가 끝났으므로 일반 상태로 복귀
            dashPowerUpState = 0;
            UIManager.instance.SetDoubleDashIconActive(false);
        }

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

    public void StartForcedMove(float targetY)
    {
        if (isForceMoving || isDead) return;

        StartCoroutine(ForcedMoveCoroutine(targetY));
    }

    private IEnumerator ForcedMoveCoroutine(float targetY)
    {
        isForceMoving = true;
        playerInput.enabled = false;

        transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
        float moveDirection = 1f;

        while (isForceMoving)
        {
            if (isDead) break;
            rb.linearVelocity = new Vector2(moveDirection * 10f, 1f);
            yield return new WaitForFixedUpdate();
        }

        rb.linearVelocity = Vector2.zero;
        playerInput.enabled = true;
    }

    private IEnumerator JumpCooldownCoroutine()
    {
        // 수영 중이면 swimJumpCooldown을, 아니면 일반 jumpCooldown을 사용
        float cooldown = isSwimming ? swimJumpCooldown : jumpCooldown;
        yield return new WaitForSeconds(cooldown);
        canJump = true;
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public bool CanDoubleJump()
    {
        return canDoubleJump;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // [수정] 씬 로드 시 체크포인트 위치로 이동하는 로직
        if (GameManager.instance != null && scene.name == GameManager.instance.GetRespawnSceneName())
        {
            // GameManager에 저장된 체크포인트가 있다면 해당 위치로 플레이어를 이동시킵니다.
            if (GameManager.instance.isRespawnPointSet)
            {
                transform.position = GameManager.instance.respawnPoint;
                Debug.Log("저장된 체크포인트에서 부활합니다.");
            }
        }

        // 죽고 나서 부활한 경우, 상태를 초기화합니다.
        if (isDead)
        {
            ResetPlayerStateAfterDeath();
        }
    }


    private void ResetPlayerStateAfterDeath()
    {
        isDead = false;
        playerInput.enabled = true;
        GetComponent<Collider2D>().enabled = true;
        rb.bodyType = RigidbodyType2D.Dynamic;
        currentHealth = maxHealth;
        canDoubleJump = false; // 2단 점프 초기화

        // UIManager에게 UI 업데이트를 다시 요청
        if (UIManager.instance != null)
        {
            UIManager.instance.UpdateHealthUI(currentHealth);
            UIManager.instance.SetDoubleJumpIconActive(canDoubleJump);
            UIManager.instance.SetDoubleDashIconActive(false);
        }
    }

    // [추가] UIManager가 호출할 수 있도록 상태를 알려주는 함수들


    public bool CanDoubleJumpStatus()
    {
        return canDoubleJump;
    }

    public bool HasDashPowerUp()
    {
        // 대시 강화 시퀀스가 진행 중일 때 true를 반환
        return dashPowerUpState > 0;
    }

    void OnDestroy()
    {
        // 오브젝트 파괴 시 이벤트 구독 해제
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }


    public void ActivateDoubleDash()
    {
        UIManager.instance.SetDoubleDashIconActive(true);
        // 1단계: 아이템을 먹고 '첫 번째 대시'를 기다리는 상태로 변경
        dashPowerUpState = 1;
        // 첫 번째 대시는 즉시 사용할 수 있도록 쿨타임을 0으로 설정
        dashingCooldown = 0f;
        Debug.Log("대시 강화 시퀀스 시작! 첫 번째 대시는 쿨타임이 없습니다.");
    }

    public void OnSuicide(InputValue value)
    {
        // 버튼이 눌리는 순간에만 Die() 함수를 호출합니다.
        if (value.isPressed)
        {
            Die();
        }
    }

    public void ShowSpeechBubble(string message)
    {
        if (speechBubble != null && dialogueText != null)
        {
            dialogueText.text = message;
            speechBubble.SetActive(true);
        }
    }

    public void HideSpeechBubble()
    {
        if (speechBubble != null)
        {
            speechBubble.SetActive(false);
        }
    }
    
}
