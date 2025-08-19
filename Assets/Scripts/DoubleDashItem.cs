using System.Collections;
using UnityEngine;

public class DoubleDashItem : MonoBehaviour
{
    [Header("설정")]
    [Tooltip("아이템을 먹었을 때 재생할 사운드")]
    public AudioClip pickupSound;

    [Header("리스폰 설정")]
    [Tooltip("아이템이 다시 생성될지 여부")]
    public bool canRespawn = false;

    [Tooltip("아이템이 다시 생성되기까지 걸리는 시간(초)")]
    public float respawnCooldown = 5f;

    // --- Private 변수 ---
    private SpriteRenderer spriteRenderer;
    private Collider2D itemCollider;

    void Awake()
    {
        // 시작할 때 컴포넌트를 미리 찾아둡니다.
        spriteRenderer = GetComponent<SpriteRenderer>();
        itemCollider = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 닿은 대상이 플레이어이고, 아이템이 활성화 상태일 때
        if (other.CompareTag("Player") && itemCollider.enabled)
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                // 1. 플레이어에게 2단 점프 능력 부여
                player.ActivateDoubleDash();

                // 2. 사운드 재생
                if (pickupSound != null)
                {
                    AudioSource.PlayClipAtPoint(pickupSound, transform.position);
                }

                // 3. 리스폰 처리 시작
                StartCoroutine(HandlePickup());
            }
        }
    }

    // 아이템을 먹었을 때의 처리를 담당하는 코루틴
    private IEnumerator HandlePickup()
    {
        // 1. 아이템을 비활성화 (보이지 않고, 닿지 않게)
        spriteRenderer.enabled = false;
        itemCollider.enabled = false;

        // 2. 리스폰이 가능한지 확인
        if (canRespawn)
        {
            // 3. 정해진 쿨타임만큼 기다립니다.
            yield return new WaitForSeconds(respawnCooldown);

            // 4. 시간이 지나면 다시 아이템을 활성화합니다.
            spriteRenderer.enabled = true;
            itemCollider.enabled = true;
        }
        else
        {
            // 리스폰이 불가능하면, 오브젝트를 완전히 파괴합니다.
            Destroy(gameObject);
        }
    }
}