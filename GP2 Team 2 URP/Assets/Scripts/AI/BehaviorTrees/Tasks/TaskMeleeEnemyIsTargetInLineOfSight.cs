using UnityEngine;
using System.Collections.Generic;

public class TaskMeleeEnemyIsTargetInLineOfSight : Node
{
    public TaskMeleeEnemyIsTargetInLineOfSight(BehaviourTree aTree) : base() { myTree = aTree; }
    BehaviourTree myTree;

    private GameObject _target;

    public override NodeState Evaluate()
    {
        MeleeEnemyBT eBT = myTree as MeleeEnemyBT;
        _target = (GameObject)GetData("Target");
        float _distanceFromPlayer = Vector3.Distance(eBT.transform.position, _target.transform.position);

        if (_target != null && _distanceFromPlayer <= eBT.enemy._lineOfSight && !eBT.animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            eBT.enemy.LookAtPlayer(_target.transform);
            return NodeState.Success;
        }
        else
        {
            return NodeState.Failure;
        }
    }
}
