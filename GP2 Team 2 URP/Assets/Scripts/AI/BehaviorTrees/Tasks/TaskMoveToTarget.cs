using UnityEngine;
using System.Collections.Generic;

public class TaskMoveToTarget : Node
{
    public TaskMoveToTarget(BehaviourTree aTree) : base() { myTree = aTree; }
    BehaviourTree myTree;

    private GameObject _target;

    public override NodeState Evaluate()
    {
        EnemyBT eBT = myTree as EnemyBT;
        _target = (GameObject)GetData("Target");
        float _distanceFromPlayer = Vector3.Distance(eBT.transform.position, _target.transform.position);

        if (_target != null && !eBT.animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            Transform _targetTransform = _target.transform;
            eBT.enemy.MoveToTarget(_targetTransform);
            return NodeState.Success;
        }
        else
        {
            return NodeState.Failure;
        }
    }
}
