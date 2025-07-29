using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class SceneToggle : MonoBehaviour
{
    // 인스펙터에서 지정할 씬 이름 변수
    public string sceneA;
    public string sceneB;

    void Update()
    {
        if (Keyboard.current.nKey.wasPressedThisFrame)
        {
            string currentScene = SceneManager.GetActiveScene().name;

            // 현재 씬이 sceneA와 같다면 sceneB를, 아니면 sceneA를 불러옴
            if (currentScene == sceneA)
            {
                SceneManager.LoadScene(sceneB);
            }
            else if (currentScene == sceneB)
            {
                SceneManager.LoadScene(sceneA);
            }
        }
    }
}
