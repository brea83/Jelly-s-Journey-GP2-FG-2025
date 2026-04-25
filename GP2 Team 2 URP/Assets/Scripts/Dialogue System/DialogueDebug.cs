using UnityEngine;

public class DialogueDebug : MonoBehaviour
{

    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private string dialogueName;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        Debug.Log("DialogueDebug Awake, running test dialogue " + dialogueName);
        dialogueManager.StartDialogueByName(dialogueName);
    }

}
