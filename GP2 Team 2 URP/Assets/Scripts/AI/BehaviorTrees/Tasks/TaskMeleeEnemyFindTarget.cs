using UnityEngine;

public class TaskMeleeEnemyFindTarget : Node
{
    public TaskMeleeEnemyFindTarget(BehaviourTree aTree) : base() { myTree = aTree; }
    BehaviourTree myTree;

    public override NodeState Evaluate()
    {
        MeleeEnemyBT eBT = myTree as MeleeEnemyBT;
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            Parent.SetData("Target", player);
            return NodeState.Success;
        }
        else
        {
            return NodeState.Failure;
        }
    }
}
