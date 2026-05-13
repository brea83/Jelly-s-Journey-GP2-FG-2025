using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.EventSystems;
using Unity.VisualScripting;

public class PlayerPickUp : MonoBehaviour , IGameStateMachineListener
{
    [SerializeField] float _detectionRange = 5f;
    public LayerMask obstacleMask;
    public GameObject closestWeapon;

    protected PlayingState _updateState;
    private void Start()
    {
        // moved to the interface IGameStateMAchineListener func OnGameStateMachineInitialized which is called from the GameManager's events
        //_updateState = GameManager.Instance.GetState<PlayingState>();


    }

    protected void OnDisable()
    {
        if (_updateState != null)
        {
            _updateState.StateUpdate.RemoveListener(ManagedUpdate);
        }
    }

    protected void OnEnable()
    {
        if (_updateState != null)
        {
            _updateState.StateUpdate.AddListener(ManagedUpdate);
        }
        else
        {
            Debug.Log("tried to add PlayerPickUp's listener but myUpdateState == null");
        }
    }

    public void OnGameStateMachineInitialized(GameManager gameManager)
    {
        _updateState = gameManager.GetState<PlayingState>();
        if (_updateState != null)
        {
            _updateState.StateUpdate.AddListener(ManagedUpdate);
        }
        else
        {
            Debug.Log("tried to add PlayerPickUp's listener but myUpdateState == null");
        }
    }

    protected void ManagedUpdate()
    {


        Collider[] hits = Physics.OverlapSphere(transform.position, _detectionRange);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Weapon"))
            {
                Vector3 direction = hit.transform.position - transform.position;
                float distance = direction.magnitude;

                if (!Physics.Raycast(transform.position, direction.normalized, distance, obstacleMask))
                {
                    closestWeapon = hit.gameObject;
                }
            }
        }
    }
}
