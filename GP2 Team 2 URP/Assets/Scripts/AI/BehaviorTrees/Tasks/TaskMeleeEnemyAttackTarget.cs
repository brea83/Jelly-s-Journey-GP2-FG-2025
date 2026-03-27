using UnityEngine;

public class TaskMeleeEnemyAttackTarget : Node
{
    public TaskMeleeEnemyAttackTarget(BehaviourTree aTree) : base() { myTree = aTree; }
    BehaviourTree myTree;

    private GameObject _target;

    public override NodeState Evaluate()
    {
        MeleeEnemyBT eBT = myTree as MeleeEnemyBT;
        _target = (GameObject)GetData("Target");
        float _distanceFromPlayer = Vector3.Distance(eBT.transform.position, _target.transform.position);

        if (_target != null && _distanceFromPlayer <= eBT.enemy._attackDistance)
        {
            eBT.enemy.Attack(_target);
            return NodeState.Success;
        }
        else
        {
            return NodeState.Failure;
        }
    }
}
