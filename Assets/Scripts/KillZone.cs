using UnityEngine;

public class KillZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("플레이어가 즉사 영역에 닿았습니다!");

            // 플레이어 컨트롤러를 찾아서
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                // 씬 재시작 대신, 리스폰 함수를 호출합니다.
                player.Die();
            }
        }
    }
}