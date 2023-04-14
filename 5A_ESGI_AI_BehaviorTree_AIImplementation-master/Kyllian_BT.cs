using AI_BehaviorTree_AIGameUtility;
using BehaviorTree_ESGI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Windows.UI.Xaml.Media;
using static UnityEngine.GraphicsBuffer;

namespace Kyllian_AI
{
    public static class Blackboard
    {

        public static int AIId;
        public static float BestDistanceToFire = 20.0f;
        public static Node startNode;

        public static List<AIAction> actions;

        public static GameWorldUtils AIGameWorldUtils = new GameWorldUtils();

        public static List<PlayerInformations> playerInfos = AIGameWorldUtils.GetPlayerInfosList();
        public static List<BonusInformations> bonusInfos = AIGameWorldUtils.GetBonusInfosList();
        public static PlayerInformations target = null;
        public static PlayerInformations myPlayerInfo = null;

        public static void Initialize(Node sn, int id)
        {
            AIId = id;
            actions = new List<AIAction>();
            startNode = sn;
        }

        public static void SetAIGameWorld(GameWorldUtils AIGW)
        {
            AIGameWorldUtils = AIGW;
        }

        public static void Execute(Func<int, List<PlayerInformations>, PlayerInformations> GetPlayerInfos)
        {
            playerInfos = AIGameWorldUtils.GetPlayerInfosList();
            bonusInfos = AIGameWorldUtils.GetBonusInfosList();
            myPlayerInfo = GetPlayerInfos(AIId, playerInfos);
            target = null;
            float targetDistance = -1;
            foreach (PlayerInformations playerInfo in playerInfos)
            {
                if (!playerInfo.IsActive)
                    continue;

                if (playerInfo.PlayerId == AIId)
                    continue;

                if(targetDistance < 0)
                {
                    target = playerInfo;
                    targetDistance = Vector3.Distance(myPlayerInfo.Transform.Position, playerInfo.Transform.Position);
                }
                    

                if(targetDistance > Vector3.Distance(myPlayerInfo.Transform.Position, playerInfo.Transform.Position))
                {
                    target = playerInfo;
                    targetDistance = Vector3.Distance(myPlayerInfo.Transform.Position, playerInfo.Transform.Position);
                }
            }       
            actions = new List<AIAction>();
            startNode.Execute();
        }

        public static void Add(AIAction action)
        {
            actions.Add(action);
        }

        //Utils

        public static Vector3 GetBestDirection(LayerMask layerMask)
        {

            Vector3 dir = Vector3.left;
            RaycastHit lefthit;
            Physics.Raycast(myPlayerInfo.Transform.Position, myPlayerInfo.Transform.Rotation * Vector3.left, out lefthit, Mathf.Infinity, ~layerMask);
            RaycastHit righthit;
            Physics.Raycast(myPlayerInfo.Transform.Position, myPlayerInfo.Transform.Rotation * Vector3.right, out righthit, Mathf.Infinity, ~layerMask);

            if (Vector3.Distance(lefthit.collider.transform.position, myPlayerInfo.Transform.Position) < Vector3.Distance(righthit.collider.transform.position, myPlayerInfo.Transform.Position))
            {
                dir = Vector3.right;
            }

            return dir;
        }
    }

    // Sequences

    public class AttackSequence : Sequence
    {
        public AttackSequence() : base() {
            AimAction aimAction = new AimAction();
            LookAtTarget lookAtTarget = new LookAtTarget();
            ShootAction shootAction = new ShootAction();
            nodes.Add(aimAction);
            nodes.Add(lookAtTarget);
            nodes.Add(shootAction);
        }
    }

    public class StopSequence : Sequence
    {
        public StopSequence() : base()
        {
            HasBestRangeToShoot hasBestRangeToShoot = new HasBestRangeToShoot();
            StopAction stopAction = new StopAction();
            nodes.Add(hasBestRangeToShoot);
            nodes.Add(stopAction);
        }
    }

    public class GetAwaySequence : Sequence
    {
        public GetAwaySequence() : base()
        {
            TargetGetSight targetTooClose = new TargetGetSight();
            GetAwaySelector getAwaySelector = new GetAwaySelector();
            nodes.Add(targetTooClose);
            nodes.Add(getAwaySelector);
        }
    }

    public class IsDashAvailableSequence : Sequence
    {
        public IsDashAvailableSequence() : base()
        {
            IsDashAvailable isDashAvailable = new IsDashAvailable();
            DashSelector dashSelector = new DashSelector();
            nodes.Add(isDashAvailable);
            nodes.Add(dashSelector);

        }
    }

    public class AwayDashSequence : Sequence
    {
        public AwayDashSequence() : base()
        {
            TargetTooClose targetTooClose = new TargetTooClose();
            AwayDash awayDash = new AwayDash();
            nodes.Add(targetTooClose);
            nodes.Add(awayDash);
        }
    }

    public class LowHealthSequence : Sequence
    {
        public LowHealthSequence() : base()
        {
            IsBonusAvailable isBonusAvailable = new IsBonusAvailable();
            LowInHealth lowInHealth = new LowInHealth();
            GetHealthBonus getHealthBonus = new GetHealthBonus();
            HasTargetCondition hasTargetCondition = new HasTargetCondition(); 
            AttackSequence shoot = new AttackSequence();
            nodes.Add(isBonusAvailable);
            nodes.Add(lowInHealth);
            nodes.Add(getHealthBonus);
            nodes.Add(hasTargetCondition);
            nodes.Add(shoot);
        }
    }

    public class GetBonusSequence : Sequence
    {
        public GetBonusSequence(): base()
        {
            IsBonusAvailable isBonusAvailable = new IsBonusAvailable();
            GetNearestBonus getNearestBonus = new GetNearestBonus();
            nodes.Add(isBonusAvailable);
            nodes.Add(getNearestBonus);
        }
    }

    public class NoTargetSequence : Sequence
    {
        public NoTargetSequence() : base()
        {
            HasNotTargetCondition hasNotTargetCondition = new HasNotTargetCondition();
            GetBonusSequence getNearestBonus = new GetBonusSequence();
            nodes.Add(hasNotTargetCondition);
            nodes.Add(getNearestBonus);
        }
    }


    //Selectors

    public class DashSelector : Selector
    {
        public DashSelector() : base()
        {
            AwayDashSequence awayDashSequence = new AwayDashSequence();
            LateralDash lateralDash = new LateralDash();
            nodes.Add(awayDashSequence); 
            nodes.Add(lateralDash);
        }
    }

    public class GetAwaySelector : Selector
    {
        public GetAwaySelector(): base()
        {
            IsDashAvailableSequence isDashAvailableSequence = new IsDashAvailableSequence();
            GetAwayAction getAwayAction = new GetAwayAction();
            nodes.Add(isDashAvailableSequence);
            nodes.Add(getAwayAction);
        }
    }

    public class MoveSelector : Selector
    {
        public MoveSelector() : base()
        {
            
            GetAwaySequence getAwaySequence = new GetAwaySequence();
            StopSequence stopSequence = new StopSequence();
            MoveAction moveAction = new MoveAction();
            nodes.Add(getAwaySequence);
            nodes.Add(stopSequence);
            nodes.Add(moveAction);
        }
    }

    //Actions

    public class AimAction : Node
    {
        public AimAction() { }

        override
        public void Execute()
        {
            AIActionLookAtPosition actionLookAt = new AIActionLookAtPosition();
            actionLookAt.Position = Blackboard.target.Transform.Position;
            Blackboard.Add(actionLookAt);
        }
    }

    public class ShootAction : Node
    {
        public ShootAction() { }

        override
        public void Execute()
        {
            state = NodeState.Success;
            Blackboard.Add(new AIActionFire());
        }
    }

    public class MoveAction : Node
    {
        public MoveAction() { }

        override
        public void Execute()
        {
            state = NodeState.Success;
            AIActionMoveToDestination actionMove = new AIActionMoveToDestination();
            actionMove.Position = Blackboard.target.Transform.Position;
            Blackboard.Add(actionMove);
        }
    }

    public class StopAction : Node
    {
        public StopAction() { }

        override
        public void Execute()
        {
            state = NodeState.Success;
            Blackboard.Add(new AIActionStopMovement());
        }
    }

    public class GetAwayAction : Node
    {
        public GetAwayAction() {
        }

        public override void Execute()
        {

            Vector3 positionFinal = Vector3.zero;
            Collider[] colliders = Physics.OverlapSphere(Blackboard.myPlayerInfo.Transform.Position, 15.0f, Blackboard.AIGameWorldUtils.ProjectileLayerMask);
            foreach(Collider collider in colliders)
            {
                var posProj = collider.transform.position;
                var distProj = Vector3.Distance(posProj , Blackboard.myPlayerInfo.Transform.Position);
                var factDist = 15.0f - distProj;
                factDist = Mathf.Clamp(factDist, 5.0f, 15.0f);

                Vector3 dir = Blackboard.GetBestDirection(Blackboard.AIGameWorldUtils.BonusLayerMask);
                positionFinal += Blackboard.myPlayerInfo.Transform.Rotation * dir * factDist;
            }


            state = NodeState.Failure;
            AIActionMoveToDestination actionMove = new AIActionMoveToDestination();
            actionMove.Position = Blackboard.target.Transform.Position + positionFinal;
            Blackboard.Add(actionMove);
        }
    }

    public class AwayDash : Node
    {
        public AwayDash() { }

        public override void Execute()
        {
            state = NodeState.Success;
            AIActionDash actionDash = new AIActionDash();

            Vector3 vecTargPlayer = Blackboard.myPlayerInfo.Transform.Position - Blackboard.target.Transform.Position;

            int layerMask = Blackboard.AIGameWorldUtils.PlayerLayerMask;
            layerMask |= Blackboard.AIGameWorldUtils.BonusLayerMask;
            layerMask |= Blackboard.AIGameWorldUtils.ProjectileLayerMask;

            Vector3 dir = Blackboard.GetBestDirection(layerMask);

            actionDash.Direction = vecTargPlayer + (Blackboard.myPlayerInfo.Transform.Rotation * dir * 5);
            Blackboard.Add(actionDash);
        }
    }

    public class LateralDash : Node
    {
        public LateralDash() { }

        public override void Execute()
        {
            state = NodeState.Success;
            AIActionDash actionDash = new AIActionDash();

            int layerMask = Blackboard.AIGameWorldUtils.PlayerLayerMask;
            layerMask |= Blackboard.AIGameWorldUtils.BonusLayerMask;
            layerMask |= Blackboard.AIGameWorldUtils.ProjectileLayerMask; 
            Vector3 dir = Blackboard.GetBestDirection(layerMask);

            actionDash.Direction = Blackboard.myPlayerInfo.Transform.Rotation * dir;
            Blackboard.Add(actionDash);
        }
    }

    public class GetHealthBonus : Node 
    { 
        public GetHealthBonus() { }

        public override void Execute()
        {
           
            BonusInformations nearestHealthBonus = null;
            float dist = float.MaxValue;
            for (int i = 0; i < Blackboard.bonusInfos.Count; i++)
            {
                if(Blackboard.bonusInfos[i].Type.Equals(EBonusType.Health))
                {
                    float tempDist = Vector3.Distance(Blackboard.bonusInfos[i].Position, Blackboard.myPlayerInfo.Transform.Position);
                    if (tempDist <= dist)
                    {
                        nearestHealthBonus = Blackboard.bonusInfos[i];
                        dist = tempDist;
                    }
                }
                
            }

            if(nearestHealthBonus == null)
            {
                state = NodeState.Failure; return;
            }

            AIActionMoveToDestination actionMove = new AIActionMoveToDestination();
            actionMove.Position = nearestHealthBonus.Position;
            Blackboard.Add(actionMove);
            state = NodeState.Success;
        }
    }

    public class GetNearestBonus : Node
    {
        public GetNearestBonus() { }

        public override void Execute()
        {
            BonusInformations nearesthBonus = Blackboard.bonusInfos[0];
            float dist = Vector3.Distance(nearesthBonus.Position, Blackboard.myPlayerInfo.Transform.Position);
            for(int i = 1; i < Blackboard.bonusInfos.Count; i++)
            {
                float tempDist = Vector3.Distance(Blackboard.bonusInfos[i].Position, Blackboard.myPlayerInfo.Transform.Position);
                if (tempDist <= dist)
                {
                    nearesthBonus = Blackboard.bonusInfos[i];
                    dist = tempDist;
                }
            }

            AIActionMoveToDestination actionMove = new AIActionMoveToDestination();
            actionMove.Position = nearesthBonus.Position;
            Blackboard.Add(actionMove);
            state = NodeState.Success;
        }
    }

    //Conditions

    public class HasNotTargetCondition : Condition
    {
        public HasNotTargetCondition() { }

        public override bool Check()
        {
            return Blackboard.target == null;
        }
    }

    public class HasTargetCondition : Condition
    {
        public HasTargetCondition() { }

        public override bool Check()
        {
            return Blackboard.target != null;
        }
    }

    public class IsPlayerInvalidCondition : Condition
    {
        public IsPlayerInvalidCondition() { }

        public override bool Check()
        {
            return Blackboard.myPlayerInfo == null;
        }
    }

    public class HasBestRangeToShoot : Condition
    {
        public HasBestRangeToShoot() { }

        public override bool Check()
        {
            return Vector3.Distance(Blackboard.myPlayerInfo.Transform.Position, Blackboard.target.Transform.Position) < Blackboard.BestDistanceToFire;
        }
    }

    public class TargetGetSight : Condition
    {
        public TargetGetSight() { }

        public override bool Check()
        {
            int layers = Blackboard.AIGameWorldUtils.BonusLayerMask;
            layers |= Blackboard.AIGameWorldUtils.ProjectileLayerMask;

            RaycastHit hit;
            if (Physics.Raycast(Blackboard.target.Transform.Position, Blackboard.target.Transform.Rotation * Vector3.forward, out hit, Mathf.Infinity, ~layers))
            {
                return Vector3.Distance(hit.collider.transform.position, Blackboard.myPlayerInfo.Transform.Position) < 0.00001f;
            }

            return false;
        }
    }

    public class IsDashAvailable : Condition
    { 
        public IsDashAvailable() { }
        
        public override bool Check()
        {
            return Blackboard.myPlayerInfo.IsDashAvailable;
        }
    
    }

    public class TargetTooClose : Condition
    {
        public TargetTooClose() { }

        public override bool Check()
        {
            //return Vector3.Distance(Blackboard.myPlayerInfo.Transform.Position, Blackboard.target.Transform.Position) < 18.0f;
            return Physics.OverlapSphere(Blackboard.myPlayerInfo.Transform.Position, 15.0f, Blackboard.AIGameWorldUtils.ProjectileLayerMask).Length > 0;
        }
    }

    public class LookAtTarget : Condition
    {
        public LookAtTarget() { }

        public override bool Check()
        {

            int layers = Blackboard.AIGameWorldUtils.BonusLayerMask;
            layers |= Blackboard.AIGameWorldUtils.ProjectileLayerMask;
            
            RaycastHit hit;
            if (Physics.Raycast(Blackboard.myPlayerInfo.Transform.Position, Blackboard.myPlayerInfo.Transform.Rotation * Vector3.forward, out hit, Mathf.Infinity, ~layers))
            {
                return Vector3.Distance(hit.collider.transform.position, Blackboard.target.Transform.Position) < 0.00001f;
            }

            return false;
        }
    }

    public class LowInHealth : Condition { 
        public LowInHealth() { } 

        public override bool Check()
        {
            return (Blackboard.myPlayerInfo.CurrentHealth / Blackboard.myPlayerInfo.MaxHealth) < 0.3;
        }
    }

    public class IsHealthBonus : Condition
    {
        public IsHealthBonus() { }

        public override bool Check()
        {
            foreach(var bi in Blackboard.bonusInfos)
            {
                if(bi.Type.Equals(EBonusType.Health))
                {
                    return true;
                }
            }

            return false;
        }
    }

    public class IsBonusAvailable : Condition
    { 
        public IsBonusAvailable() { }

        public override bool Check()
        {
            return Blackboard.bonusInfos.Count > 0;
        }
    }

}