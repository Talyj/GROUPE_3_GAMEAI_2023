using AI_BehaviorTree_AIGameUtility;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
public class Sequence : Node
{
    public List<Node> nodes;

    public Sequence()
    {
        nodes = new List<Node>();
    }

    public void Execute()
    {
        foreach (Node n in nodes)
        {
            if (n.state == NodeState.Failure)
            {
                this.state = NodeState.Failure;
            }


        }
    }
}

public class Selector : Node
{

}

public class Condition : Node
{
    public Condition()
    {

    }
}

public class Action : Node
{
    public Action()
    {

    }
}

public enum NodeState
{
    NotExecuted = 0, Failure = 1, Success = 2, Running = 4
}

public class Node
{
    public NodeState state;
    public Node()
    {
        state = NodeState.NotExecuted;
    }

}