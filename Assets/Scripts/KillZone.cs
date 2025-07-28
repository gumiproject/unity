using UnityEngine;

public class KillZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("플레이어가 즉사 영역에 닿았습니다!");

            // 씬을 직접 재시작하는 대신 플레이어의 Die() 함수를 호출
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.Die();
            }
        }
    }
}