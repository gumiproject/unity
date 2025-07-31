using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, IDamageable
{
    public static GameObject instance;

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

    // --- Private 변수 ---
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private PlayerInput playerInput;
    private AudioSource audioSource;
    private Vector2 moveInput;
    private Vector2 originalScale;

    // --- 상태 변수 ---
    private int currentHealth;
    private bool isInvincible = false;
    private bool isDead = false;
    private bool isGrounded = true;
    private bool isRunning = false;
    private float doubleTapThreshold = 0.3f;
    private bool isDashing = false;
    private bool canFire = true;
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
        audioSource = GetComponent<AudioSource>();
        originalScale = transform.localScale;
        currentHealth = maxHealth;
    }

    void Update()
    {
        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);

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

    private void HandleCrouchState()
    {
        bool wantsToCrouch = playerInput.actions["Crouch"].IsPressed();
        if (wantsToCrouch && isGrounded) isCrouching = true;
        else if (!wantsToCrouch && CanStandUp()) isCrouching = false;
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
        if (isCrouching || isDashing || isInvincible) isRunning = false;
        else if (input.x != 0)
        {
            int currentDirection = input.x > 0 ? 1 : -1;
            if (currentDirection == lastDirection && Time.time - lastInputTime < doubleTapThreshold) isRunning = true;
            lastInputTime = Time.time;
            lastDirection = currentDirection;
        }
        else isRunning = false;
        moveInput = input;
    }

    public void OnJump(InputValue value)
    {
        if (isDead) return;
        if (value.isPressed && isGrounded && !isDashing && !isCrouching)
        {
            if (jumpSound != null) audioSource.PlayOneShot(jumpSound);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isGrounded = false;
            animator.SetTrigger("jump");
        }
    }

    public void OnDash(InputValue value)
    {
        if (isDead) return;
        if (value.isPressed && canDash && !isCrouching) StartCoroutine(Dash());
    }
    
    public void OnFire(InputValue value)
    {
        if (!canFire || isDead || isCrouching || isDashing || !value.isPressed)
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
}