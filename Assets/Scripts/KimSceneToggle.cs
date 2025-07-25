using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
public class KimSceneToggle : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Update()
    {
        if (Keyboard.current.nKey.wasPressedThisFrame)
        {
            string currentScene = SceneManager.GetActiveScene().name;

            if (currentScene == "kim_post")
            {
                SceneManager.LoadScene("kim_future");
            }
            else if (currentScene == "kim_future")
            {
                SceneManager.LoadScene("kim_post");
            }
        }
    }
}
