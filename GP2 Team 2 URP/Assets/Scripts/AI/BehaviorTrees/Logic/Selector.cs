using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class Selector : Node
{
    public Selector() : base() { }

    public Selector(List<Node> nodes) : base(nodes) { }

    public override NodeState Evaluate()
    {
        foreach (Node node in Children)
        {
            switch (node.Evaluate())
            { 
                case NodeState.Running:
                    State = NodeState.Running;
                    return State;
                case NodeState.Success:
                    State = NodeState.Success;
                    return State;
                case NodeState.Failure:
                    continue;
            }
        }

        State = NodeState.Failure;
        return State;
    }
}
