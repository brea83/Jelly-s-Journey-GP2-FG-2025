using System.Collections.Generic;
using UnityEngine;

public abstract class BehaviourTree : MonoBehaviour
{
    protected PlayingState _updateState;
    protected Node Root = null;

    protected virtual void Start()
    {
        Root = SetUpTree();
        _updateState = GameManager.Instance.GetState<PlayingState>();
        if (_updateState != null)
        {
            _updateState.StateUpdate.AddListener(ManagedUpdate);
            _updateState.StateFixedUpdate.AddListener(ManagedFixedUpdate);
        }
        else
        {
            Debug.Log("tried to add listener but myUpdateState == null");
        }
    }

    protected virtual void OnDisable()
    {
        if (_updateState != null)
        {
            _updateState.StateUpdate.RemoveListener(ManagedUpdate);
        }
    }
    protected virtual void ManagedUpdate()
    {
        if(Root != null)
        {
            Root.Evaluate();
        }
    }
    protected virtual void ManagedFixedUpdate()
    {

    }
    protected abstract Node SetUpTree();
}
