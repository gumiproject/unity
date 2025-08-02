using UnityEngine;

public class MovingWalkway : MonoBehaviour
{
    [Header("설정")]
    [Tooltip("플레이어를 밀어낼 방향과 힘. Y값을 0으로 두면 아래의 '띄우기'가 적용됩니다.")]
    public Vector2 moveForce = new Vector2(5f, 0f);

    [Header("Y축 힘이 없을 때만 적용")]
    [Tooltip("플레이어를 무빙워크 표면에서 띄울 높이")]
    public float liftHeight = 0.1f;

    [Tooltip("플레이어를 띄우는 힘의 세기")]
    public float liftForce = 50f;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody2D playerRb = other.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                // Y축 힘이 0에 가까운지 확인합니다 (부동 소수점 오차를 고려).
                if (Mathf.Approximately(moveForce.y, 0f))
                {
                    // Y축 힘이 0일 때: 수평으로만 밀고, 위로 띄웁니다.
                    
                    // 1. 수평 힘 적용
                    playerRb.AddForce(new Vector2(moveForce.x, 0f));

                    // 2. 목표 높이 계산
                    float targetY = transform.position.y + GetComponent<Collider2D>().offset.y + liftHeight;
                    
                    // 3. 현재 플레이어가 목표보다 낮으면 위로 띄우는 힘 적용
                    if (other.transform.position.y < targetY)
                    {
                        playerRb.AddForce(Vector2.up * liftForce);
                    }
                }
                else
                {
                    // Y축 힘이 있을 때: 설정된 방향과 힘으로 그대로 밉니다 (띄우지 않음).
                    playerRb.AddForce(moveForce);
                }
            }
        }
    }
}