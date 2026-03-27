using UnityEngine;

public class TaskAvoidTarget : Node
{
    public TaskAvoidTarget(BehaviourTree aTree) : base() { myTree = aTree; }
    BehaviourTree myTree;

    private GameObject _target;

    public override NodeState Evaluate()
    {
        EnemyBT eBT = myTree as EnemyBT;
        _target = (GameObject)GetData("Target");
        float _distanceFromPlayer = Vector3.Distance(eBT.transform.position, _target.transform.position);

        if (_target != null && _distanceFromPlayer <= eBT.enemy._avoidPlayer)
        {
            eBT.enemy.AvoidPlayer(_target.transform);
            return NodeState.Success;
        }
        else
        {
            return NodeState.Failure;
        }
    }
}
