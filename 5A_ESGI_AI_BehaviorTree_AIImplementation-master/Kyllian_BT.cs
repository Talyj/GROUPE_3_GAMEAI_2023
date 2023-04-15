using AI_BehaviorTree_AIGameUtility;
using BehaviorTree_ESGI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
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

        public static float overlap = 10.0f;

        public static Vector3 lastTargetPosition = Vector3.zero;
        public static bool isSetTargetPositon = false;
        public static int lastIDTarget = -1;

        public static void Initialize(int id)
        {
            AIId = id;
            actions = new List<AIAction>();
            startNode = new StartSelector();
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
    #region Sequences

    public class AttackSequence : Sequence
    {
        public AttackSequence(): base() { 
            AimAction aimAction = new AimAction();
            IsTargetVisible isTargetVisible = new IsTargetVisible();
            ShootAction shootAction = new ShootAction();
            ForceFailure forceFailure = new ForceFailure();
            nodes.Add(aimAction);
            nodes.Add(isTargetVisible);
            nodes.Add(shootAction);
            nodes.Add(forceFailure);
        }
    }

    public class LowHealthSequence : Sequence
    {
        public LowHealthSequence() : base()
        {
            IsPlayerLowInHealth isPlayerLowInHealth = new IsPlayerLowInHealth();
            IsBonusAvailable isBonusAvailable = new IsBonusAvailable();
            GetHealthBonus getHealthBonus = new GetHealthBonus();
            nodes.Add(isPlayerLowInHealth);
            nodes.Add(isBonusAvailable);
            nodes.Add(getHealthBonus);
        }
    }

    public class GetBonusSequence : Sequence
    {
        public GetBonusSequence() : base()
        {
            IsBonusAvailable isBonusAvailable = new IsBonusAvailable();
            GetNearestBonus getNearestBonus = new GetNearestBonus();
            nodes.Add(isBonusAvailable);
            nodes.Add(getNearestBonus);
        }
    }

    public class DashAwaySequence : Sequence
    {
        public DashAwaySequence() : base()
        {
            IsDashAvailable isDashAvailable = new IsDashAvailable();
            IsTargetTooClose isTargetTooClose = new IsTargetTooClose();
            DashAwayAction dashAwayAction = new DashAwayAction();
            nodes.Add(isDashAvailable);
            nodes.Add(isTargetTooClose);
            nodes.Add(dashAwayAction);
        }
    }

    #endregion


    //Selectors
    #region Selector
    public class StartSelector : Selector
    {
        public StartSelector() : base()
        {
            Inverter PlayerValidity = new Inverter(new PlayerValid());
            Inverter TargetValidity = new Inverter(new TargetValid());
            AttackSequence attackSequence = new AttackSequence();
            LowHealthSequence lowHealthSequence = new LowHealthSequence();
            GetBonusSequence getBonusSequence = new GetBonusSequence();
            DashAwaySequence dashAwaySequence = new DashAwaySequence();
            MoveAction moveAction = new MoveAction();
            nodes.Add(PlayerValidity);
            nodes.Add(TargetValidity);
            nodes.Add(attackSequence);
            nodes.Add(lowHealthSequence);
            nodes.Add(getBonusSequence);
            nodes.Add(dashAwaySequence);
            nodes.Add(moveAction);

        }
    }

    #endregion

    //Actions
    #region Actions
    public class AimAction : Node
    {
        
        public AimAction()
        {
        }

        override
        public void Execute()
        {
            if (Blackboard.isSetTargetPositon)
            {
                Blackboard.lastTargetPosition = Blackboard.target.Transform.Position;
                Blackboard.lastIDTarget = Blackboard.target.PlayerId;
                state = NodeState.Running;
                return;
            }

            if(Blackboard.lastIDTarget != Blackboard.target.PlayerId)
            {
                Blackboard.lastTargetPosition = Blackboard.target.Transform.Position;
                Blackboard.lastIDTarget = Blackboard.target.PlayerId;
                state = NodeState.Failure;
                return;
            }

            AIActionLookAtPosition actionLookAt = new AIActionLookAtPosition();

            actionLookAt.Position = Blackboard.target.Transform.Position;

            float distance = Vector3.Distance(Blackboard.lastTargetPosition, Blackboard.target.Transform.Position);

            if (distance > 0.1f)
            {
                Vector3 offset = Blackboard.target.Transform.Position - Blackboard.lastTargetPosition;
                Vector3 newPostion = Blackboard.target.Transform.Position + offset;

                float distanceToTarget = Vector3.Distance(newPostion, Blackboard.myPlayerInfo.Transform.Position);

                float projectileSpeed = 5.0f;
                float travelTime = distanceToTarget / projectileSpeed;

                float angle = 5.0f * travelTime;

                actionLookAt.Position = newPostion + (Quaternion.Euler(0.0f, angle, 0.0f) * offset); ;

            }

            Blackboard.lastTargetPosition = Blackboard.target.Transform.Position;

            Blackboard.Add(actionLookAt);
            state = NodeState.Success;
        }
    }

    public class ShootAction : Node
    {
        public ShootAction() : base() { }

        public override void Execute()
        {
            AIActionFire aIActionFire = new AIActionFire();
            Blackboard.Add(aIActionFire);
        }
    }

    public class GetHealthBonus : Node
    {
        public GetHealthBonus() : base() { }

        public override void Execute()
        {
            BonusInformations nearestHealthBonus = null;
            float dist = float.MaxValue;
            for (int i = 0; i < Blackboard.bonusInfos.Count; i++)
            {
                if (Blackboard.bonusInfos[i].Type.Equals(EBonusType.Health))
                {
                    float tempDist = Vector3.Distance(Blackboard.bonusInfos[i].Position, Blackboard.myPlayerInfo.Transform.Position);
                    if (tempDist <= dist)
                    {
                        nearestHealthBonus = Blackboard.bonusInfos[i];
                        dist = tempDist;
                    }
                }

            }

            if (nearestHealthBonus == null)
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
            for (int i = 1; i < Blackboard.bonusInfos.Count; i++)
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

    public class MoveAction : Node
    {
        public MoveAction() { }

        public override void Execute()
        {
            int layerMask = Blackboard.AIGameWorldUtils.BonusLayerMask;
            Vector3 dir = Blackboard.GetBestDirection(layerMask);

            Vector3 dirToTarget = Blackboard.target.Transform.Position - Blackboard.myPlayerInfo.Transform.Position;

            Vector3 pos = dirToTarget + dir;

            pos.Normalize();

            var distance = Vector3.Distance(Blackboard.target.Transform.Position, Blackboard.myPlayerInfo.Transform.Position) - Blackboard.overlap;

            AIActionMoveToDestination aIActionMoveTo = new AIActionMoveToDestination();
            aIActionMoveTo.Position = pos * distance;
            Blackboard.Add(aIActionMoveTo);

        }
    }

    public class DashAwayAction : Node
    {
        public DashAwayAction() { }

        public override void Execute()
        {
            int layerMask = Blackboard.AIGameWorldUtils.BonusLayerMask;
            Vector3 dir = Blackboard.GetBestDirection(layerMask);

            Vector3 dirToTarget = Blackboard.myPlayerInfo.Transform.Position - Blackboard.target.Transform.Position;

            Vector3 pos = dirToTarget + dir;

            AIActionDash aIActionDash = new AIActionDash();
            aIActionDash.Direction = pos;
            Blackboard.Add(aIActionDash);
        }
    }

    #endregion

    //Conditions
    #region Condition

    public class PlayerValid : Condition
    {
        public PlayerValid() { }

        public override bool Check()
        {
            return Blackboard.myPlayerInfo != null;
        }
    }

    public class TargetValid : Condition
    {
        public TargetValid() { }

        public override bool Check()
        {
            return Blackboard.target != null;
        }
    }

    public class IsTargetVisible : Condition
    {
        public IsTargetVisible() { }

        public override bool Check()
        {
            int layers = Blackboard.AIGameWorldUtils.BonusLayerMask;
            layers |= Blackboard.AIGameWorldUtils.ProjectileLayerMask;

            RaycastHit hit;
            if (Physics.Raycast(Blackboard.myPlayerInfo.Transform.Position, Blackboard.target.Transform.Position - Blackboard.myPlayerInfo.Transform.Position, out hit, Mathf.Infinity, ~layers))
            {
                return Vector3.Distance(hit.collider.transform.position, Blackboard.target.Transform.Position) < 0.00001f;
            }

            return false;
        }
    }

    public class IsPlayerLowInHealth : Condition
    {
        public IsPlayerLowInHealth() { }

        public override bool Check()
        {
            return Blackboard.myPlayerInfo.CurrentHealth / Blackboard.myPlayerInfo.MaxHealth < 0.3;
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

    public class IsDashAvailable : Condition
    {
        public IsDashAvailable() { }

        public override bool Check()
        {
            return Blackboard.myPlayerInfo.IsDashAvailable;
        }
    }

    public class IsTargetTooClose : Condition
    {
        public IsTargetTooClose() { }

        public override bool Check()
        {
            return Vector3.Distance(Blackboard.target.Transform.Position, Blackboard.myPlayerInfo.Transform.Position) < 7.0f;
        }
    }

    #endregion

}