// UIManager.cs

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    private Image[] healthHearts;
    private GameObject doubleJumpIcon;
    private GameObject doubleDashIcon; // [추가] 더블 대시 아이콘 변수

    [Header("하트 스프라이트")]
    public Sprite fullHeartSprite;
    public Sprite emptyHeartSprite;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindUIElements();
        
        if (PlayerController.instance != null)
        {
            PlayerController player = PlayerController.instance.GetComponent<PlayerController>();
            if (player != null)
            {
                UpdateHealthUI(player.GetCurrentHealth());
                SetDoubleJumpIconActive(player.CanDoubleJumpStatus());
                SetDoubleDashIconActive(player.HasDashPowerUp()); // [추가] 씬 로드 시 대시 아이콘 상태 업데이트
            }
        }
    }

    private void FindUIElements()
    {
        GameObject uiCanvas = GameObject.FindGameObjectWithTag("UICanvas");
        if (uiCanvas == null)
        {
            Debug.LogError("씬에 'UICanvas' 태그를 가진 Canvas 오브젝트가 없습니다!");
           return;
        }

        // Canvas의 모든 자식 컴포넌트들(비활성화된 것 포함)을 가져옵니다.
        Transform[] allChildren = uiCanvas.GetComponentsInChildren<Transform>(true);

        GameObject healthBarObj = GameObject.Find("HealthBar");
        if (healthBarObj != null)
        {
            healthHearts = healthBarObj.GetComponentsInChildren<Image>();
        }
        else
        {
            healthHearts = null;
        }
        
        doubleJumpIcon = null;
        doubleDashIcon = null;
        foreach (Transform child in allChildren)
        {
            if (child.name == "DoubleJumpIcon")
            {
                doubleJumpIcon = child.gameObject;
            }
            else if (child.name == "DoubleDashIcon")
            {
                doubleDashIcon = child.gameObject;
            }
        }

        // 만약 못 찾았다면 경고를 출력해줍니다.
        if (doubleJumpIcon == null) Debug.LogWarning("UICanvas 자식 중에 DoubleJumpIcon을 찾지 못했습니다.");
        if (doubleDashIcon == null) Debug.LogWarning("UICanvas 자식 중에 DoubleDashIcon을 찾지 못했습니다.");
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
    
    // [추가] 더블 대시 아이콘을 켜고 끄는 함수
    public void SetDoubleDashIconActive(bool isActive)
    {
        if (doubleDashIcon != null)
        {
            doubleDashIcon.SetActive(isActive);
        }
    }
    
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}