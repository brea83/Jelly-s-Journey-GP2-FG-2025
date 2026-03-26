using UnityEngine;

public class CutScene : MonoBehaviour
{
    Animator _animator;
    Canvas _cutsceneCanvas;
    public UiMenuController _menuController;
    public string IntroTriggerTag;
    public string OutroTriggerTag;
    [Header("Debug Stuff")]
    public bool PrintDebugLogs = false;

    void Start()
    {
        _animator = GetComponent<Animator>();
        _cutsceneCanvas = GetComponent<Canvas>();
        //_menuController = GetComponentInParent<UiMenuController>();
        _cutsceneCanvas.enabled = false;
        if (PrintDebugLogs) Debug.Log($"--- CutScene.Start() found animator ({_animator}) and canvas ({_cutsceneCanvas}) ---");
    }
    public void StartIntro()
    {
        _menuController.HideCurrentMenu();
        //_menuController.mainMenu.SetActive(false);
        _cutsceneCanvas.enabled = true;
        _cutsceneCanvas.sortingOrder = 10;
        if (PrintDebugLogs) Debug.Log($"--- CutScene.StartIntro() SETTING SORTING ORDER TO -* 10 *- IT IS ACTUALLY ({_cutsceneCanvas.sortingOrder}) ---");
        _animator.SetTrigger(IntroTriggerTag);
        Time.timeScale = 1.0f;
        if (PrintDebugLogs) Debug.Log($"--- CutScene.StartIntro() ({_animator}) setTrigger by string ({IntroTriggerTag}) ---");
    }
    private void FinishedIntro()
    {
        if (PrintDebugLogs) Debug.Log($"--- CutScene.FinishedIntro() Called) ---");
        _cutsceneCanvas.sortingOrder = 0;
        _cutsceneCanvas.enabled = false;
        _menuController.StartGameLevelOne();
    }
    public void StartOutro()
    {
        _cutsceneCanvas.enabled = true;
        _cutsceneCanvas.sortingOrder = 10;
        Time.timeScale = 1.0f;
        _animator.SetTrigger(OutroTriggerTag);
        if (PrintDebugLogs) Debug.Log($"--- CutScene.StartOutro() ({_animator}) setTrigger by string ({OutroTriggerTag}) ---");
        // open credits? go to title?
    }
    private void FinishedOutro()
    {
        if (PrintDebugLogs) Debug.Log($"--- CutScene.FinishedOutro() Called) ---");
        _cutsceneCanvas.sortingOrder = 0;
        _cutsceneCanvas.enabled = false;
        GameManager.Instance.player.GetComponent<PlayerController>().Initialize();
        GameManager.Instance.player.GetComponent<PlayerInventory>().Initialize();
        GameManager.Instance.player.GetComponent<PlayerHealth>().Initialize();
        GameManager.Instance.switchState<StartState>();
        //Debug.LogWarning("--- WE HAVE NOT IMPLEMENTED WHAT HAPPENS WHEN THE OUTRO ENDS YET ---");
        // open credits? go to title?
    }
    public void SkipCutscene()
    {
        Debug.LogWarning("--- SKIP CUTSCENE NOT IMPLEMENTED YET ---");
        // can we tell the animation to go to it's final frames?
    }
}
