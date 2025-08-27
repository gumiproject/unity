using System.Collections;
using UnityEngine;

public class ActivateTilemap : MonoBehaviour
{
    public GameObject targetTilemap;
    private Animator buttonAnimator;

    void Start()
    {
        buttonAnimator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 한 번 눌린 후에는 다시 작동하지 않게
            // 버튼의 콜라이더를 비활성화합니다.
            GetComponent<Collider2D>().enabled = false;
            
            // 버튼이 눌린 애니메이션을 실행합니다.
            if (buttonAnimator != null)
            {
                buttonAnimator.SetTrigger("PressButton");
            }
            
            // Tilemap을 활성화하고 5초 뒤에 비활성화하는 코루틴을 시작합니다.
            if (targetTilemap != null)
            {
                targetTilemap.SetActive(true);
                StartCoroutine(DeactivateTilemapAfterDelay(5f));
            }
        }
    }

    // 타일맵을 일정 시간 뒤에 비활성화하는 코루틴
    IEnumerator DeactivateTilemapAfterDelay(float delay)
    {
        // 지정된 시간(delay)만큼 기다립니다.
        yield return new WaitForSeconds(delay);

        // 기다린 후 타일맵을 비활성화합니다.
        if (targetTilemap != null)
        {
            targetTilemap.SetActive(false);
        }

        // 버튼 애니메이션을 원래 상태(Idle)로 되돌립니다.
        if (buttonAnimator != null)
        {
            // "PressButton" 트리거를 다시 비활성화 (선택 사항)
            buttonAnimator.ResetTrigger("PressButton");

            // 버튼 애니메이터의 상태를 "Idle"로 되돌리는 트리거를 호출합니다.
            // Animator에 "ReturnToIdle" 트리거를 추가해야 합니다.
            buttonAnimator.SetTrigger("ReturnToIdle");
        }
    }
}