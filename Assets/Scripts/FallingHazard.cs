using UnityEngine;

public class FallingHazard : MonoBehaviour
{
    // 이 오브젝트가 다른 콜라이더와 물리적 충돌을 시작할 때 호출됩니다.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 부딪힌 대상이 "Player" 태그를 가졌거나 "Ground" 레이어라면
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            // 이 오브젝트(위험물)를 즉시 파괴합니다.
            Destroy(gameObject);
        }
    }
}