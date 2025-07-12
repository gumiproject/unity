using UnityEngine;
using UnityEngine.InputSystem; // Input System을 사용하기 위해 꼭 필요합니다!

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Animator animator;
    private Rigidbody2D rb;
    
    private Vector2 moveInput; // 키보드 입력을 저장할 변수

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    // Player Input 컴포넌트가 'Move' 액션을 감지하면 이 함수를 자동으로 실행합니다.
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void FixedUpdate() // 물리 업데이트는 FixedUpdate에서 하는 것이 더 안정적입니다.
    {
        // 1. 실제 이동 처리
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
        
        // 2. 방향에 따라 캐릭터 뒤집기
        if (moveInput.x > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (moveInput.x < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }

        // 3. 애니메이터의 isMoving 파라미터 값 변경
        // moveInput.x가 0이 아니면(움직이면) true, 0이면(멈추면) false
        animator.SetBool("isMoving", moveInput.x != 0);
    }
}