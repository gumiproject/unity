using UnityEngine;
using UnityEngine.SceneManagement;
public class StartScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void TutorialNextScene()
    {
        SceneManager.LoadScene("1-1_post"); // 실제 다음 씬 이름으로 변경하세요.
    }

    public void StartNextScene()
    {
        SceneManager.LoadScene("1-1_post"); // 실제 다음 씬 이름으로 변경하세요.
    }
}
