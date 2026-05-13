using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class CutScene : MonoBehaviour
{
    Animator _animator;
    Canvas _cutsceneCanvas;
    public UiMenuController _menuController;
    public string IntroTriggerTag;
    public string OutroTriggerTag;
    [Header("Debug Stuff")]
    public bool PrintDebugLogs = false;

    bool m_CutSceneStarted = false;
    InputAction m_UiScrollWheel;
    float m_TimeMultiplier = 1.0f;
    float m_TimeMultIncrement = 0.5f;

    private void OnEnable()
    {
        if(m_UiScrollWheel != null)
        {
            m_UiScrollWheel.performed -= OnScrollWheelPerformed;
            m_UiScrollWheel.performed += OnScrollWheelPerformed;
        }
    }
    void Start()
    {
        _animator = GetComponent<Animator>();
        _cutsceneCanvas = GetComponent<Canvas>();
        //_menuController = GetComponentInParent<UiMenuController>();
        _cutsceneCanvas.enabled = false;
        if (PrintDebugLogs) Debug.Log($"--- CutScene.Start() found animator ({_animator}) and canvas ({_cutsceneCanvas}) ---");

        GameObject playerObject = GameManager.Instance.player;
        if (playerObject == null)
            return;

        PlayerInput input = playerObject.GetComponent<PlayerInput>();
        if (input == null)
            return;

        m_UiScrollWheel = input.actions.FindAction("ScrollWheel");
        if (m_UiScrollWheel == null)
            return;

        m_UiScrollWheel.performed += OnScrollWheelPerformed;
    }

    private void OnDisable()
    {
        if( m_UiScrollWheel != null )
            m_UiScrollWheel.performed -= OnScrollWheelPerformed;
    }

    private void OnScrollWheelPerformed(InputAction.CallbackContext context)
    {
        Vector2 inputValue = context.ReadValue<Vector2>();

        if (!m_CutSceneStarted || inputValue.y == 0)
            return;

        if (inputValue.y > 0) 
            m_TimeMultiplier += m_TimeMultIncrement;
        else if (inputValue.y < 0)
            m_TimeMultiplier -= m_TimeMultIncrement;

        if(PrintDebugLogs) Debug.Log($"Current time multiplier is: {m_TimeMultiplier}.");
        Time.timeScale = 1.0f * m_TimeMultiplier;
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
        m_CutSceneStarted = true;
        if (PrintDebugLogs) Debug.Log($"--- CutScene.StartIntro() ({_animator}) setTrigger by string ({IntroTriggerTag}) ---");
    }
    private void FinishedIntro()
    {
        if (PrintDebugLogs) Debug.Log($"--- CutScene.FinishedIntro() Called) ---");
        _cutsceneCanvas.sortingOrder = 0;
        _cutsceneCanvas.enabled = false;

        m_CutSceneStarted = false;
        m_TimeMultiplier = 1.0f;
        Time.timeScale = 1.0f;

        _menuController.StartGameLevelOne();
    }
    public void StartOutro()
    {
        _cutsceneCanvas.enabled = true;
        _cutsceneCanvas.sortingOrder = 10;
        Time.timeScale = 1.0f;
        _animator.SetTrigger(OutroTriggerTag);
        m_CutSceneStarted = true;
        if (PrintDebugLogs) Debug.Log($"--- CutScene.StartOutro() ({_animator}) setTrigger by string ({OutroTriggerTag}) ---");
        // open credits? go to title?
    }
    private void FinishedOutro()
    {
        if (PrintDebugLogs) Debug.Log($"--- CutScene.FinishedOutro() Called) ---");
        _cutsceneCanvas.sortingOrder = 0;
        _cutsceneCanvas.enabled = false;

        m_CutSceneStarted = false;
        m_TimeMultiplier = 1.0f;
        Time.timeScale = 1.0f;

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
