using UnityEngine;

public class TaskMeleeEnemyIsStunned : Node
{
    public TaskMeleeEnemyIsStunned(BehaviourTree aTree) : base() { myTree = aTree; }
    BehaviourTree myTree;

    public override NodeState Evaluate()
    {
        MeleeEnemyBT eBT = myTree as MeleeEnemyBT;

        if (!eBT.enemy._isStunned)
        {
            return NodeState.Success;
        }
        else
        {
            return NodeState.Failure;
        }
    }
}
