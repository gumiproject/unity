using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float runSpeed = 8f;     // 달리기 속도
    public float jumpForce = 12f;
    private Animator animator;
    private Rigidbody2D rb;

    private Vector2 moveInput;
    private bool isGrounded = true;
    private bool isRunning = false;

    private float lastInputTime = 0f;
    private float doubleTapThreshold = 0.3f;  // 더블탭 허용 시간

    private int lastDirection = 0; // -1: 왼쪽, 1: 오른쪽

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
            }
            else
            {
                isRunning = false;
            }

            lastInputTime = Time.time;
            lastDirection = currentDirection;
        }

        moveInput = input;
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed && isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isGrounded = false;
            animator.SetTrigger("jump");
        }
    }

    void FixedUpdate()
    {
        float currentSpeed = isRunning ? runSpeed : moveSpeed;
        rb.linearVelocity = new Vector2(moveInput.x * currentSpeed, rb.linearVelocity.y);
        Flip();
        animator.SetBool("isMoving", moveInput.x != 0);
    }

    private void Flip()
    {
        Vector3 scale = transform.localScale;
        if (moveInput.x > 0)
            scale.x = Mathf.Abs(scale.x);
        else if (moveInput.x < 0)
            scale.x = -Mathf.Abs(scale.x);
        transform.localScale = scale;
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
