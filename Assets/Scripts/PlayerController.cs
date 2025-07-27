using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

// MonoBehaviour 옆에 , IDamageable 을 추가합니다.
public class PlayerController : MonoBehaviour, IDamageable
{
    public static GameObject instance;

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
        originalScale = transform.localScale;
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (isDashing || isInvincible) return;

        HandleCrouchState();
        HandleFacingDirection();
        ApplyVisuals();
    }

    void FixedUpdate()
    {
        if (isDashing) return;
        HandleMovement();
    }

    public void TakeDamage(Vector2 knockbackDirection)
    {
        if (isInvincible) return;

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

    private IEnumerator InvincibilityCoroutine(Vector2 knockbackDirection)
    {
        isInvincible = true;
        
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

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

    private void Die()
    {
        Debug.Log("플레이어 사망!");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f && collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
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
                    Debug.Log("몬스터를 밟았다!");
                    Destroy(collision.gameObject);
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * 0.7f);
                    return;
                }
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

    private void HandleCrouchState()
    {
        bool wantsToCrouch = GetComponent<PlayerInput>().actions["Crouch"].IsPressed();
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