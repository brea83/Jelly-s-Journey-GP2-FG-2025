using UnityEngine;
using System.Collections.Generic;
using static UnityEngine.GraphicsBuffer;
using Unity.VisualScripting;

public class TaskIsTargetInLineOfSight : Node
{
    public TaskIsTargetInLineOfSight(BehaviourTree aTree) : base() { myTree = aTree; }
    BehaviourTree myTree;

    private GameObject _target;

    public override NodeState Evaluate()
    {
        EnemyBT eBT = myTree as EnemyBT;
        _target = (GameObject)GetData("Target");
        float _distanceFromPlayer = Vector3.Distance(eBT.transform.position, _target.transform.position);

        if(_target != null && _distanceFromPlayer <= eBT.enemy._lineOfSight)
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
