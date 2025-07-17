using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    // --- 이동 및 점프 변수 ---
    public float moveSpeed = 5f;
    public float jumpForce = 12f;
    private Animator animator;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool isGrounded = true;

    // --- 대쉬 변수 ---
    [Header("대쉬 설정")]
    public float dashPower = 24f;
    public float dashingTime = 0.2f;
    public float dashingCooldown = 1f;
    private bool canDash = true;
    private bool isDashing;

    // --- 달리기 변수 ---
    [Header("달리기 설정")]
    public float runSpeed = 8f;
    private bool isRunning = false;
    private float lastInputTime = 0f;
    private float doubleTapThreshold = 0.3f;
    private int lastDirection = 0;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    public void OnMove(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();

        if (input.x != 0)
        {
            int currentDirection = input.x > 0 ? 1 : -1;
            if (currentDirection == lastDirection && Time.time - lastInputTime < doubleTapThreshold)
            {
                isRunning = true;
                animator.SetBool("isRunning", true);
            }
            
            lastInputTime = Time.time;
            lastDirection = currentDirection;
        }
        else
        {
            isRunning = false;
            animator.SetBool("isRunning", false);
        }

        moveInput = input;
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed && isGrounded && !isDashing)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isGrounded = false;
            animator.SetTrigger("jump");
        }
    }

    public void OnDash(InputValue value)
    {
        if (value.isPressed && canDash)
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
        rb.linearVelocity = new Vector2(transform.localScale.x * dashPower, 0f);

        yield return new WaitForSeconds(dashingTime);

        rb.gravityScale = originalGravity;
        isDashing = false;
        animator.SetBool("isDashing", false);

        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }

    void FixedUpdate()
    {
        if (isDashing)
        {
            return;
        }

        float currentSpeed = isRunning ? runSpeed : moveSpeed;
        rb.linearVelocity = new Vector2(moveInput.x * currentSpeed, rb.linearVelocity.y);
        
        Flip();
        
        animator.SetBool("isMoving", moveInput.x != 0);
    }

    // ### ▼▼▼ 이 부분을 올바른 코드로 다시 수정했습니다 ▼▼▼ ###
    private void Flip()
    {
        if (isDashing) return;

        // 현재 스케일 값을 가져와서 방향만 바꾸도록 수정
        Vector3 scale = transform.localScale;
        if (moveInput.x > 0)
            scale.x = Mathf.Abs(scale.x);
        else if (moveInput.x < 0)
            scale.x = -Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    // --- 기존 땅 감지 로직 ---
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
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f && collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                isGrounded = true;
                break;
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
}