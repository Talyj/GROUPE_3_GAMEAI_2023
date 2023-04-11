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

    public new void Execute()
    {
        foreach (Node n in nodes)
        {
            if (n.state == NodeState.Failure)
            {
                this.state = NodeState.Failure;
                return;
            }

            if(n.state != NodeState.NotExecuted)
            {
                this.state = n.state;
            }
        }
    
    
    }
}

public class Selector : Node
{
    public Node lastRunningNode;
    public List<Node> nodes;

    public Selector()
    {
        nodes = new List<Node>();
        lastRunningNode= null;
    }

    public new void Execute()
    {
        foreach(var i in nodes)
        {
            if (i.state == NodeState.Running)
            {
                lastRunningNode = i;
            }

            if (i.state == NodeState.Success)
            {
                return ;
            }

        }
    }
}

public class Condition : Node
{
    public Func<bool> conditionMethod;
    public Condition(Func<bool> conditionMethod)
    {
        this.conditionMethod = conditionMethod; 
    }

    public new void Execute()
    {
        if (conditionMethod())
        {
            this.state = NodeState.Success;
        }
        else
        {
            this.state = NodeState.Failure;
        }
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

    public void Execute()
    {
        //TODO : Exe the differents nodes in the list
    }

    public void ForceSuccess()
    {
        this.state = NodeState.Success;
    }

    public void ForceFailure()
    {
        this.state = NodeState.Failure;
    }
}