using UnityEngine;
using System.Collections.Generic;

public class TaskIsStunned : Node
{
    public TaskIsStunned(BehaviourTree aTree) : base() { myTree = aTree; }
    BehaviourTree myTree;

    public override NodeState Evaluate()
    {
        EnemyBT eBT = myTree as EnemyBT;

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
