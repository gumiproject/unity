using UnityEngine;

public interface IDamageable
{
    // 이 규칙을 따르는 모든 클래스는 이 함수를 반드시 가져야 합니다.
    void TakeDamage(Vector2 knockbackDirection);
}