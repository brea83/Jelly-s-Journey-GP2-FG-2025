using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public enum NodeState
{
    Running,
    Success,
    Failure
}
public class Node
{
    protected NodeState State;
    public Node Parent;
    protected List<Node> Children = new List<Node>();

    private Dictionary<string, object> Context = new Dictionary<string, object>();

    public Node()
    {
        Parent = null;
    }

    public Node(List<Node> children)
    {
        foreach (Node node in children)
        { 
            AddChild(node);
        }
    }

    public void AddChild(Node aNode)
    {
        aNode.Parent = this;
        Children.Add(aNode);
    }

    public virtual NodeState Evaluate() => NodeState.Failure;

    public void SetData(string key, object value)
    {
        Context[key] = value;
    }

    public object GetData(string key)
    { 
        object value = null;
        if (Context.TryGetValue(key, out value))
        { 
            return value;
        }

        Node node = Parent;
        while (node != null)
        { 
            value = node.GetData(key);
            if (value != null)
            {
                return value;
            }
            node = node.Parent;
        }
        return null;
    }

    public bool ClearData(string key)
    { 
        if(Context.ContainsKey(key))
        {
            Context.Remove(key);
            return true;
        }

        Node node = Parent;
        while (node != null)
        { 
            bool cleared = node.ClearData(key);
            if(cleared)
            {
                return true;
            }
            node = node.Parent;
        }
        return false;
    }
}
