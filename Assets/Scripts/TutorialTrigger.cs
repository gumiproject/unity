using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    [TextArea(3, 10)]
    public string tutorialMessage;
    
    private bool hasBeenTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
         Debug.Log(other.name + " 오브젝트가 트리거에 들어왔습니다!");
        if (other.CompareTag("Player") && !hasBeenTriggered)
        {
             Debug.Log(other.name + " 플레이어가 들어왔다. 말풍선.");
            hasBeenTriggered = true;
            // 플레이어의 PlayerController 스크립트를 가져와서 말풍선을 띄우도록 명령
            other.GetComponent<PlayerController>()?.ShowSpeechBubble(tutorialMessage);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 플레이어의 PlayerController 스크립트를 가져와서 말풍선을 숨기도록 명령
            other.GetComponent<PlayerController>()?.HideSpeechBubble();
        }
    }
}