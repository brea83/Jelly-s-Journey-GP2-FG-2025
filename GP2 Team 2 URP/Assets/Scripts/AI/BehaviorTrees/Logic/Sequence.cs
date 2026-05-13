using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class Sequence : Node
{
    public Sequence() : base() { }

    public Sequence(List<Node> children) : base(children) { }

    public override NodeState Evaluate()
    {
        bool anyChildrenRunning = false;

        foreach (Node node in Children)
        {
            switch (node.Evaluate())
            {
                case NodeState.Running:
                    anyChildrenRunning = true;
                    break;
                case NodeState.Success:
                    continue;
                case NodeState.Failure:
                    State = NodeState.Failure;
                    return State;
            }
        }

        State = anyChildrenRunning ? NodeState.Running : NodeState.Success;
        return State;
    }
}
