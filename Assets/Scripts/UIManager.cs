// UIManager.cs 스크립트 전체를 아래 코드로 교체하세요.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    // 인스펙터에서 직접 연결하는 대신, 코드가 직접 찾도록 private으로 변경
    private Image[] healthHearts;
    private GameObject doubleJumpIcon;

    [Header("하트 스프라이트")]
    public Sprite fullHeartSprite;
    public Sprite emptyHeartSprite;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            // 씬이 로드될 때마다 'OnSceneLoaded' 함수를 실행하도록 등록
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 씬이 로드될 때마다 자동으로 호출되는 함수
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬에 있는 UI 요소들을 다시 찾아옵니다.
        FindUIElements();
        
        // 플레이어의 현재 상태에 맞게 UI를 즉시 업데이트합니다.
        if (PlayerController.instance != null)
        {
            PlayerController player = PlayerController.instance.GetComponent<PlayerController>();
            if (player != null)
            {
                UpdateHealthUI(player.GetCurrentHealth());
                SetDoubleJumpIconActive(player.CanDoubleJumpStatus());
            }
        }
    }

    // UI 요소를 이름으로 찾는 함수
    private void FindUIElements()
    {
        GameObject healthBarObj = GameObject.Find("HealthBar");
        if (healthBarObj != null)
        {
            healthHearts = healthBarObj.GetComponentsInChildren<Image>();
        }
        else
        {
            healthHearts = null; // 이전 참조를 비워줍니다.
        }
        
        doubleJumpIcon = GameObject.Find("DoubleJumpIcon");
    }

    public void UpdateHealthUI(int currentHealth)
    {
        if (healthHearts == null || healthHearts.Length == 0) return;

        for (int i = 0; i < healthHearts.Length; i++)
        {
            if (i < currentHealth)
            {
                healthHearts[i].sprite = fullHeartSprite;
            }
            else
            {
                healthHearts[i].sprite = emptyHeartSprite;
            }
        }
    }

    public void SetDoubleJumpIconActive(bool isActive)
    {
        if (doubleJumpIcon != null)
        {
            doubleJumpIcon.SetActive(isActive);
        }
    }
    
    // 오브젝트가 파괴될 때 이벤트 구독을 해제하여 메모리 누수를 방지합니다.
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}