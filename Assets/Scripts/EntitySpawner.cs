using System.Collections;
using UnityEngine;

public class EntitySpawner : MonoBehaviour
{
    [Header("설정")]
    [Tooltip("생성할 엔티티의 프리팹")]
    public GameObject entityPrefab;

    [Tooltip("엔티티가 생성되는 시간 간격(초)")]
    public float spawnInterval = 2f;

    // Start is called before the first frame update
    void Start()
    {
        // 게임이 시작되면, 엔티티를 계속 생성하는 코루틴을 시작합니다.
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        // 게임이 실행되는 동안 무한히 반복합니다.
        while (true)
        {
            // 1. 설정된 시간만큼 기다립니다.
            yield return new WaitForSeconds(spawnInterval);

            // 2. 엔티티 프리팹이 설정되어 있는지 확인합니다.
            if (entityPrefab != null)
            {
                // 3. 이 스포너의 위치에 새로운 엔티티를 생성합니다.
                Instantiate(entityPrefab, transform.position, Quaternion.identity);
            }
        }
    }
}