using NGAME;
using RoomSystem;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLoader : MonoBehaviour
{
    public Vector3 NoDoorsFallbackEntrance = new Vector3(0f, 0.5f, 0f);
    [Header("Debug stuff")]
    public bool PrintDebugLogs = false;

    private GameObject _Player;
    private PlayerInputMapSwapper _PlayerInputMapSwapper;

    private void Start()
    {
        //GameManager.Instance.StateEnter.AddListener(OnStateEnter);
        _Player = GameManager.Instance.player;
        _PlayerInputMapSwapper = _Player.GetComponent<PlayerInputMapSwapper>();

    }
    public void OnSceneLoadStart()
    {
        //PlayerInputMapSwapper _playerInputMapSwapper = _player.GetComponent<PlayerInputMapSwapper>();//<PlayerInput>();
        if (_PlayerInputMapSwapper != null)
        {
            if (PrintDebugLogs) Debug.Log($" room navigator is trying to disable  current actionmap {_PlayerInputMapSwapper.Input.currentActionMap?.name}");
            _PlayerInputMapSwapper.ToggleInputsEnabled(false);
        }
        if (_Player == null)
            return;
        _Player.GetComponent<PlayerController>().ChangeDash();
        _Player.SetActive(false);
        if (PrintDebugLogs) Debug.Log($"disabling player to leave current room");
    }

    public void OnSceneLoadComplete(IEncounterRegionConnector traversedConnector)
    {
        if (_Player == null)
        {
            _Player = GameManager.Instance.player;
            _PlayerInputMapSwapper = _Player.GetComponent<PlayerInputMapSwapper>();
            if (_Player == null)
                return;
        }

        if(traversedConnector == null)
        {
            _Player.transform.SetPositionAndRotation(NoDoorsFallbackEntrance, Quaternion.identity);
            _Player.SetActive(true);

        }
        else
        {
            Door entrance = traversedConnector as Door;
            if(entrance != null)
            {
                _Player.transform.SetPositionAndRotation(entrance.Data.EntrancePosition, entrance.Data.EntranceRotation);
                _Player.SetActive(true);
               
            }
        }
        //PlayerInputMapSwapper _playerInputMapSwapper = _player.GetComponent<PlayerInputMapSwapper>();//<PlayerInput>();
        if (_PlayerInputMapSwapper != null && _PlayerInputMapSwapper.Input != null)
        {
            if (PrintDebugLogs) Debug.Log($"room navigator is trying to enable current actionmap {_PlayerInputMapSwapper.Input.currentActionMap?.name}");
            _PlayerInputMapSwapper.ToggleInputsEnabled();
        }
    }
}
