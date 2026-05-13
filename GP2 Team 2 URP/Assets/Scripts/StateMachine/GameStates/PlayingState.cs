using RoomSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayingState : GameState
{

    public override void EnterState()
    {
        PlayerInputMapSwapper inputMapSwapper = myStateMachine.player.GetComponent<PlayerInputMapSwapper>();
        if (inputMapSwapper != null)
        {
            inputMapSwapper.EnableGameplayInputs();
        }

        base.EnterState();

        GameState previousState = myStateMachine.previousState;
        
        if (previousState != null 
            && (previousState.GetType() == typeof(StartState) 
                || previousState.GetType() == typeof(EndState))
                || previousState.GetType() == typeof(RestartState)) 
        {

            NewRoomNavigator navigator = myStateMachine.GetComponent<NewRoomNavigator>();
            //RoomNavigator navigator = myStateMachine.GetComponent<RoomNavigator>();
            
            if (navigator == null )
            {
                //Scene gameplayScene = SceneManager.GetSceneByName(myStateMachine.GameplaySceneName);
                //if (gameplayScene != null)
                //{
                //    SceneManager.LoadScene(myStateMachine.GameplaySceneName);
                //    //SceneManager.LoadScene("BDsTestPlayerOnly", LoadSceneMode.Additive);
                //}

                return;
            }

            if (navigator.NavigationEnabled)
            {
                navigator.EnterFirstRoom();
            }
        }


        Time.timeScale = 1.0f;

        
    }
    public override void UpdateState()
    {
        base.UpdateState();
       /*if (Input.GetKeyDown(KeyCode.Escape))
        {
            GameManager.Instance.SwitchState<PauseState>();
        }*/
    }
    public override void FixedUpdateState()
    {
        base.FixedUpdateState();
    }
    public override void ExitState()
    {
        base.ExitState();
        //myStateMachine.UiController.ExitUiState("PlayingState");
    }

    
}
