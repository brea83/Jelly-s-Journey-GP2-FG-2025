using UnityEngine;
using System.Collections.Generic;

public class TaskFindTarget : Node
{
    public TaskFindTarget(BehaviourTree aTree) : base() { myTree = aTree; }
    BehaviourTree myTree;

    public override NodeState Evaluate()
    {
        EnemyBT eBT = myTree as EnemyBT;
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
