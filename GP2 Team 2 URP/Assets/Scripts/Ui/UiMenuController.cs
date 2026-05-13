using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using RoomSystem;
using System;

public class UiMenuController : MonoBehaviour
{
    public TMP_Dropdown resDropDown;
    bool isfullscren;
    public enum UIState { Title, Pause, Gameplay, Settings, Credits, Death }
    public UIState CurrentState { get { return _currentState; } }
    private UIState _currentState;
    private UIState _previousState;
    private Dictionary<UIState, GameObject> _uiStateLookup = new Dictionary<UIState, GameObject>();

    private Resolution[] allResolutions;
    private int _selectedResolution;
    List<Resolution> _selectedResolutionList = new List<Resolution>();

    [SerializeField] private GameObject _playerUI;
    [SerializeField] private GameObject _mainMenu;
    [SerializeField] private GameObject _mainMenuButtons;
    [SerializeField] private GameObject _pauseCanvas;
    [SerializeField] private GameObject _settingsCanvas;
    [SerializeField] private GameObject _settingsBackgroundIngame;
    [SerializeField] private GameObject _gameOver;
    [SerializeField] private GameObject _creditsCanvas;

    [SerializeField] private Toggle gamePadToggle;

    private void Start()
    {
        SetupResolutions();

        GameManager.Instance.StateEnter.AddListener(OnStateEnter);

        gamePadToggle.isOn = PlayerPrefs.GetInt("isUsingGamepad", 0) == 1;
        SetupUiLookup();
    }

    private void SetupResolutions()
    {
        allResolutions = Screen.resolutions;
        int currentResolutionIndex = 0;

        List<string> resolutionStringList = new List<string>();
        string newRes;
        for (int i = 0; i < allResolutions.Length; i++)
        {
            newRes = allResolutions[i].width + " x " + allResolutions[i].height;
            if (!resolutionStringList.Contains(newRes))
            {
                resolutionStringList.Add(newRes);
                _selectedResolutionList.Add(allResolutions[i]);

                if (allResolutions[i].width == Screen.currentResolution.width &&
                    allResolutions[i].height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = _selectedResolutionList.Count - 1;
                }
            }
        }
        resDropDown.AddOptions(resolutionStringList);

        resDropDown.value = currentResolutionIndex;
        resDropDown.RefreshShownValue();
    }

    private void SetupUiLookup()
    {
        _uiStateLookup = new Dictionary<UIState, GameObject>
        {
            { UIState.Title, _mainMenu },
            { UIState.Pause , _pauseCanvas },
            { UIState.Gameplay, _playerUI },
            { UIState.Settings, _settingsCanvas },
            { UIState.Credits, _creditsCanvas },
            { UIState.Death, _gameOver }
        };
    }
    public void ToggleGamePad()
    {
        PlayerPrefs.SetInt("isUsingGamepad", gamePadToggle.isOn ? 1 : 0);
    }

    public void ChangeResolution()
    {
        _selectedResolution = resDropDown.value;
        Screen.SetResolution(_selectedResolutionList[_selectedResolution].width, _selectedResolutionList[_selectedResolution].height, Screen.fullScreen);
    }

    public void StartGameLevelOne()
    {
        //GameManager.Instance.GetComponent<RoomNavigator>().Initialize();
        GameManager.Instance.player.GetComponent<PlayerController>().Initialize();
        GameManager.Instance.player.GetComponent<PlayerInventory>().Initialize();
        GameManager.Instance.player.GetComponent<PlayerHealth>().Initialize();
        GameManager.Instance.SwitchState<PlayingState>();
    }

    public void ResumeGame()
    {
        GenericBack();
        //GameManager.Instance.SwitchState<PlayingState>();
    }

    public void MainMenu()
    {
        GameManager.Instance.SwitchState<StartState>();
    }

    public void SettingsIngame()
    {
        _settingsCanvas.SetActive(true);
        _settingsBackgroundIngame.SetActive(true);

        _pauseCanvas.SetActive(false);
        _previousState = _currentState;
        _currentState = UIState.Settings;
    }
    public void Settings()
    {
        _settingsCanvas.SetActive(true);
        _mainMenuButtons.SetActive(false);

        _previousState = _currentState;
        _currentState = UIState.Settings;
    }

    public void SettingsBack()
    {
        if (GameManager.Instance.CurrentState is PauseState)
        {
            _settingsCanvas.SetActive(false);
            _settingsBackgroundIngame.SetActive(false);

            _pauseCanvas.SetActive(true);

            _currentState = UIState.Pause;
            _previousState = _currentState;
        }
        else
        {
            _settingsCanvas.SetActive(false);
            _mainMenuButtons.SetActive(true);

            _currentState = UIState.Title;
            _previousState = _currentState;
        }
    }
    public void OpenCredits()
    {
        _previousState = _currentState;
        if(_previousState is UIState.Pause)
        {
            _pauseCanvas.SetActive(false);
        }
        else
        {
            _mainMenuButtons.SetActive(false);
        }
        _currentState = UIState.Credits;
        _creditsCanvas.SetActive(true);
    }
    private GameObject GetMenuFromUiState(UIState state)
    {

        if (_uiStateLookup.ContainsKey(state))
        {
            return _uiStateLookup[state];
        }
        return null;
    }
    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void Fullscreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
    }

    public void OnStateEnter(GameState state)
    {

        if (state is StartState)
        {
            _currentState = UIState.Title;

            _playerUI.SetActive(false);
            _mainMenu.SetActive(true);
            _pauseCanvas.SetActive(false);
            _gameOver.SetActive(false);
        }
        else if (state is PlayingState)
        {
            _currentState = UIState.Gameplay;

            _mainMenu.SetActive(false);
            _playerUI.SetActive(true);
            _pauseCanvas.SetActive(false);

            _gameOver.SetActive(false);
        }
        else if (state is PauseState)
        {
            _currentState = UIState.Pause;
            _pauseCanvas.SetActive(true);
        }
        else if (state is EndState)
        {
            _currentState = UIState.Death;
            _gameOver.SetActive(true);
        }
        //game state change sets what the base of the ui stack is
        _previousState = _currentState;
    }

    public void HandleOnMenuExit()
    {
        GenericBack();
    }
    public void GenericBack()
    {
        if (IsUiEscapeTransitionValid(_currentState, _previousState))
        {
            GetMenuFromUiState(_currentState)?.SetActive(false);

            if (_currentState is UIState.Pause)
            {
                GameManager.Instance.SwitchState<PlayingState>();
            }
            else
            {
                //we don't want input.OnMenuExit() to be able to back back into settings and sub menus so only the submenus use the _previous state
                if(_previousState is UIState.Pause)
                {
                    _pauseCanvas.SetActive(true);

                }
                else
                {
                    _mainMenuButtons.SetActive(true);
                }
                _currentState = _previousState;
            }

        }
    }
    private bool IsUiEscapeTransitionValid(UIState currentState, UIState nextState)
    {
        //bool result = false;
        switch (currentState)
        {
            case UIState.Title:
                return false;
            case UIState.Pause:
                return true;
            case UIState.Gameplay:
                return false;
            case UIState.Settings:
                if (nextState is UIState.Title || nextState is UIState.Pause)
                    return true;
                break;
            case UIState.Credits:
                if (nextState is UIState.Title || nextState is UIState.Pause)
                    return true;
                break;
            case UIState.Death:
                return false;
        }
        return false;
    }
    public void HideCurrentMenu()
    {
        GetMenuFromUiState(_currentState).SetActive(false);
    }
}
