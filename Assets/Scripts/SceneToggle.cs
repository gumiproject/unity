using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class SceneToggle : MonoBehaviour
{
    void Update()
    {
        if (Keyboard.current.nKey.wasPressedThisFrame)
        {
            string currentScene = SceneManager.GetActiveScene().name;

            if (currentScene == "semple_post")
            {
                SceneManager.LoadScene("semple_future");
            }
            else if (currentScene == "semple_future")
            {
                SceneManager.LoadScene("semple_post");
            }
        }
    }
}
