using UnityEngine;

// Turner added this 2/11/2025
public class ChangeBgmOnLoad : MonoBehaviour
{
    [SerializeField] private MusicState StartMusicState;
    [SerializeField] private MusicState EndMusicState;


    // Music state to transition to when this is loaded
    void Start()
    {
        switch (StartMusicState)
        {
            case MusicState.Main:
                GameManager.Instance.BgmManager.ChangeBgmToMain();
                break;
            case MusicState.Battle:
                GameManager.Instance.BgmManager.ChangeBgmToBattle();
                break;
            case MusicState.Boss:
                GameManager.Instance.BgmManager.ChangeBgmToBoss();
                break;
            default:
                Debug.LogError("Invalid MusicState");
                GameManager.Instance.BgmManager.ChangeBgmToMain();
                break;
        }
        
    }

    // Music state to transition to when this is destroyed
    private void OnDestroy()
    {
        switch (EndMusicState)
        {
            case MusicState.Main:
                GameManager.Instance.BgmManager.ChangeBgmToMain();
                break;
            case MusicState.Battle:
                GameManager.Instance.BgmManager.ChangeBgmToBattle();
                break;
            case MusicState.Boss:
                GameManager.Instance.BgmManager.ChangeBgmToBoss();
                break;
            default:
                Debug.LogError("Invalid MusicState");
                GameManager.Instance.BgmManager.ChangeBgmToMain();
                break;
        }
    }
}

// End Turner Add