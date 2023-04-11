using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI_BehaviorTree_AIImplementation_0
{
    internal class Node
    {
        public enum currentState
        {
            NotExecuted,
            Running,
            Failure,
            Success
        }

        public enum type
        {
            NotExecuted,
            Running,
            Failure,
            Success
        }

        public 
        public currentState state;
        public List<Node> nodes = new List<Node>();

        public Node()
        {
            state = currentState.NotExecuted;
        }
    }
}
