using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AI_BehaviorTree_AIGameUtility;
using BehaviorTree_ESGI;
using UnityEngine;
using UnityEngine.Assertions;

namespace Julien_BT
{
    public class JulienBT : Node
    {
        public AIAction ExecuteAction(int typeAction, Vector3 positionTarg = new Vector3())
        {
            switch (typeAction)
            {
                //Move
                case 0:
                    Debug.Log("ça bouge");
                    return new AIActionMoveToDestination()
                    {
                        Position = positionTarg
                    };
                //Stop
                case 1:
                    Debug.Log("ça stop");
                    return new AIActionStopMovement();
                //Shoot
                case 2:
                    Debug.Log("ça tire");
                    return new AIActionFire();
                //LookAt
                case 3:
                    Debug.Log("ça regarde");
                    return new AIActionLookAtPosition()
                    {
                        Position = positionTarg
                    };
                //Dash
                case 4:
                    Debug.Log("ça dash");
                    return new AIActionDash();
            }
            return new AIActionStopMovement();
        }

        //TODO 
    }

    public static class Blackboard
    {
        public static GameWorldUtils gameWorldUtils;
        public static List<Node> NodeList;
        public static List<AIAction> actionList;
        public static PlayerInformations target;
        public static PlayerInformations myPlayerInfo;
        public static int AIId;
        public static Selector firstNode;
        public static float BestDistanceToFire;


        public static void Initialize(int aiid)
        {
            NodeList = new List<Node>();
            AIId = aiid;
            firstNode = new Selector();
            BestDistanceToFire = 10.0f;
        }

        public static void UpdateBlackboard(Func<int, List<PlayerInformations>, PlayerInformations> GetPlayerInfos) 
        {
            List<PlayerInformations> playerInfos = gameWorldUtils.GetPlayerInfosList();

            foreach (PlayerInformations playerInfo in playerInfos)
            {
                if (!playerInfo.IsActive)
                    continue;

                if (playerInfo.PlayerId == AIId)
                    continue;

                target = playerInfo;
                break;
            }
            myPlayerInfo = GetPlayerInfos(AIId, playerInfos);
            actionList = new List<AIAction>();
            firstNode.Execute();
        }
    }

    public class LookAt : Node
    {
        public LookAt() { }

        public override void Execute()
        {
            var actionLookAt = new AIActionLookAtPosition()
            {
                Position = Blackboard.target.Transform.Position
            };
            Blackboard.actionList.Add(actionLookAt);
            state = NodeState.Success;
        }
    }

    public class MoveToTarg : Node
    {
        public MoveToTarg() { }

        public override void Execute()
        {
            if (Vector3.Distance(Blackboard.myPlayerInfo.Transform.Position, Blackboard.target.Transform.Position) > Blackboard.BestDistanceToFire)
            {
                AIActionMoveToDestination actionMove = new AIActionMoveToDestination()
                {
                    Position = Blackboard.target.Transform.Position
                };
                Blackboard.actionList.Add(actionMove);
                state = NodeState.Success;
            }
        }
    }

    public class CanDash : Condition
    {
        public CanDash() { }
        public override bool Check()
        {
            return Blackboard.myPlayerInfo.IsDashAvailable;
        }
    }

    public class MovementSequence : Sequence
    {        
        public MovementSequence() : base ()
        {
            MoveToTarg moveToTarg = new MoveToTarg();
            nodes.Add(moveToTarg);
        }
    }

    public class DashFrom : Node
    {
        Vector3 dir;
        public DashFrom(Vector3 direction) { dir = direction; }

        public override void Execute()
        {

            if (Blackboard.myPlayerInfo.IsDashAvailable)
            {
                Debug.LogError("Oue");
                AIActionDash actionDash = new AIActionDash()
                {
                    Direction = dir
                };
                Blackboard.actionList.Add(actionDash);
                state = NodeState.Success;
            }
        }
    }

    //DashAction
    //MoveAction
    public class DodgeSequence : Sequence
    {
        public DodgeSequence() : base()
        {
            var projectifInformations = Physics.OverlapSphere(Blackboard.myPlayerInfo.Transform.Position, 50.0f);
            var bullets = new List<ProjectileInformations>();
            foreach (var obj in projectifInformations)
            {
                if (obj.GetComponent<ProjectileInformations>() != null)
                {
                    bullets.Add(obj.GetComponent<ProjectileInformations>());
                }
            }
            foreach (var bul in bullets)
            {
                if (bul.PlayerId != Blackboard.AIId)
                {
                    var dirBullet = bul.Transform.Rotation * Vector3.forward;
                    CanDash canDash = new CanDash();
                    nodes.Add(canDash);
                    DashFrom actionNode = new DashFrom(Quaternion.AngleAxis(90, Vector3.up) * dirBullet);
                    nodes.Add(actionNode);
                }
            }
        }
    }

    public class AttackSequence : Sequence
    {

        public AttackSequence() : base()
        {
            nodes = new List<Node>();
            //PAPAPAPAPA
        }
    }
}
