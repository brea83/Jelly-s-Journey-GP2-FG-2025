using System.Linq;
using UnityEngine;
using UnityEngine.Events;
public class GameManager : StateMachine
{
    public static GameManager Instance;
    // private fields
    [SerializeField]
    private string _gameplaySceneName;              // A note about the play and start states loading scenes.
    [SerializeField]                                // the strings here need to match a name in the project's build settings
    private string _titleSceneName;
    [SerializeField]
    private UiController _uiController;


    // Turner added this 2/11/2025
    // Adding for BGM mangement
    [SerializeField] private BgmManager _bgmManager;
    public BgmManager BgmManager { get { return _bgmManager; } }
    // End Turner added this 2/11/2025


    public GameObject player;
    // public getters
    public string GameplaySceneName { get { return _gameplaySceneName; } }
    public string TitleSceneName {  get { return _titleSceneName; } }
    public UiController UiController { get { return _uiController; } }

    // EVENTS
    public UnityEvent<GameManager> StateMachineInitialized;
    public UnityEvent<GameState> StateEnter;
    public UnityEvent<GameState> StateExit;

    public GameState previousState {  get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject); 
        }

        _states = GetComponentsInChildren<State>().ToList();
        if (StateMachineInitialized != null) StateMachineInitialized.Invoke(this);
    }
    public override void SwitchState<T>()
    {
        previousState = (GameState)_currentState;
        base.SwitchState<T>();
    }

    void Start()
    {
        if (CurrentState != null
            && States.Contains(CurrentState))
        {
            System.Type type = CurrentState.GetType();
            SwitchState((GameState)CurrentState);
        }
        else
        {
            SwitchState<StartState>();
        }
    }
    void Update()
    {
        UpdateStateMachine();  
    }

    private void FixedUpdate()
    {
        FixedUpdateStateMachine();
    }

    public GameState GetGameState<T>() where T : GameState
    {
        foreach (State state in States)
        {
            if (state.GetType() == typeof(T))
            {
                return (GameState)state;
            }
        }
        Debug.LogWarning("GameState does not exist in GameManager");
        return null;
    }

    public void SwitchState(GameState state)
    {
        if (States.Contains(state))
        {
            
            if(state.GetType() == typeof(StartState))
            {
                SwitchState<StartState>();
            }
            else if (state.GetType() == typeof(PlayingState))
            {
                SwitchState<PlayingState>();
            }
            else if (state.GetType() == typeof(PauseState))
            {
                SwitchState<PauseState>();
            }
            else if (state.GetType() == typeof(EndState))
            {
                SwitchState<EndState>();
            } 
            else
            {
                Debug.LogWarning($"Tried to switch to state of type {state}, but could not find a matching state in the switch logic. check if new state needs to be added or if wrong state is being requested");
            }
        }
        else
        {
            Debug.LogWarning($"Tried to switch to state of type {state}, but could not find a matching state in GameManager's list of states. check if new state needs to be added or if wrong state is being requested");
        }
    }
}
