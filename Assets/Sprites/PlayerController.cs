using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    // --- Public 변수 ---
    [Header("이동 및 점프")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;

    [Header("달리기")]
    public float runSpeed = 8f;

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
    private PlayerInput playerInput;
    private Vector2 moveInput;
    private Vector2 originalScale;

    // --- 상태 변수 ---
    private bool isGrounded = true;
    private bool isRunning = false;
    private float doubleTapThreshold = 0.3f;
    private bool isDashing = false;
    private bool canDash = true;
    private bool isCrouching = false;
    private bool isFacingRight = true;
    private float lastInputTime = 0f;
    private int lastDirection = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();
        originalScale = transform.localScale;
    }

    void Update()
    {
        if (isDashing) return;

        // 1. 입력에 따라 상태 결정
        HandleCrouchState();
        HandleFacingDirection();

        // 2. 결정된 상태를 바탕으로 크기(Scale)와 애니메이션 최종 적용
        ApplyVisuals();
    }

    void FixedUpdate()
    {
        if (isDashing) return;
        HandleMovement();
    }

    // --- 핵심 로직 함수들 ---

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
        // 엎드리기 상태와 방향에 따라 최종 크기를 계산
        float targetYScale = isCrouching ? originalScale.y * 0.5f : originalScale.y;
        float targetXScale = isFacingRight ? originalScale.x : -originalScale.x;
        transform.localScale = new Vector3(targetXScale, targetYScale, transform.localScale.z);

        // 애니메이터 업데이트
        animator.SetBool("isMoving", moveInput.x != 0);
        animator.SetBool("isRunning", isRunning);
        animator.SetBool("isCrouching", isCrouching);
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

    // --- 기존 Input System 이벤트 함수들 (최소한으로 수정) ---

    public void OnMove(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();

        if (isCrouching || isDashing)
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
        if (value.isPressed && isGrounded && !isDashing && !isCrouching)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isGrounded = false;
            animator.SetTrigger("jump");
        }
    }

    public void OnDash(InputValue value)
    {
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
        animator.SetBool("isRunning", false);
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

    // --- 기존 땅 감지 로직 ---
    private void OnCollisionEnter2D(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f && collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                isGrounded = true; break;
            }
        }
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f && collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                isGrounded = true; break;
            }
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = false;
        }
    } 
    
    void Awake()
    {
        if (FindObjectsOfType<PlayerMovement>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        // PlayerInput 디바이스 재연결
        var input = GetComponent<PlayerInput>();
        if (input != null)
        {
           input.SwitchCurrentControlScheme("Keyboard&Mouse", Keyboard.current, Mouse.current);
        }
    }
}