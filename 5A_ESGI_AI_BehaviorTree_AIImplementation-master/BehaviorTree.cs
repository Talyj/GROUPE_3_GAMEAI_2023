﻿using AI_BehaviorTree_AIGameUtility;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace BehaviorTree_ESGI
{
    public class Sequence : Node
    {
        public List<Node> nodes;
        public Sequence()
        {
            nodes = new List<Node>();
        }

        public override void Execute()
        {   
            foreach (Node n in nodes)
            {
                n.Execute();
                if (n.state == NodeState.Failure)
                {
                    this.state = NodeState.Failure;
                    return;
                }

                if (n.state != NodeState.NotExecuted)
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
            lastRunningNode = null;
        }

        public override void Execute()
        {

            if (lastRunningNode != null)
            {
                lastRunningNode.Execute();
                if (lastRunningNode.state == NodeState.Success)
                {
                    return;
                }
                else if (lastRunningNode.state == NodeState.Failure)
                {
                    lastRunningNode = null;
                }
            }

            foreach (var node in nodes)
            {
                node.Execute();

                if (node.state == NodeState.Running)
                {
                    lastRunningNode = node;
                    return;
                }

                if (node.state == NodeState.Success)
                {
                    return;
                }
            }
        }
    }

    public class Condition : Node
    {
        public Condition()
        {
         
        }

        public override void Execute()
        {
            if (Check())
            {
                this.state = NodeState.Success;
            }
            else
            {
                this.state = NodeState.Failure;
            }
        }

        public virtual bool Check() { return true; }
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

        public virtual void Execute()
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
}