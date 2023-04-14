using AI_BehaviorTree_AIGameUtility;
using BehaviorTree_ESGI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class RBT : Node
{
    public RBT() { }

    
}

public static class Blackboard
{
    public static GameWorldUtils gameWorldUtils;
    public static List<Node> nodeList;
    public static List<AIAction> actionList;
    public static PlayerInformations target;
    public static PlayerInformations myInfo;
    public static int AIId;
    public static Node firstNode;
    public static float BestDistanceToFire;


    public static void Initialize(int _AIId, GameWorldUtils _gameWorldUtils)
    {
        nodeList = new List<Node>();
        actionList = new List<AIAction>();
        gameWorldUtils = _gameWorldUtils;
        AIId = _AIId;
        firstNode = new Node();
        BestDistanceToFire = 1550.0f;
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
        myInfo = GetPlayerInfos(AIId, playerInfos);
        actionList = new List<AIAction>();
        firstNode.Execute();
    }
}

public class MoveToNearestTarget : Node
{
    //cherche le jouer le plus proche et tire
    public MoveToNearestTarget()
    {
        
    }

    public override void Execute()
    {

        if (Vector3.Distance(Blackboard.myInfo.Transform.Position, Blackboard.target.Transform.Position) < 1550f)
        {
            AIActionMoveToDestination actionMove = new AIActionMoveToDestination();
            actionMove.Position = Blackboard.target.Transform.Position;

            AIActionLookAtPosition actionLookAt = new AIActionLookAtPosition();
            actionLookAt.Position = Blackboard.target.Transform.Position;
            Blackboard.actionList.Add(actionLookAt);
            Blackboard.actionList.Add(actionMove);
        }



        if (Vector3.Distance(Blackboard.myInfo.Transform.Position, Blackboard.target.Transform.Position) < Blackboard.BestDistanceToFire)
        {
            AIActionStopMovement actionStop = new AIActionStopMovement();
            Blackboard.actionList.Add(actionStop);
            Blackboard.actionList.Add(new AIActionFire());
        }
    }

}

public class FindTargetLowestHealth : Node
{
    //trouver le joueur avec le moins de vie
    public FindTargetLowestHealth()
    {
        
    }

    public override void Execute()
    {
        base.Execute();
    }
}

public class FindBonus : Node
{
    //cherche un bonus en fonction de certaines conditions (si pas en duel,si bonus dispo)
}

public class NeedDash : Node
{
    //creer un sphere virtuel et si ball dedans dash direction libre
    public NeedDash()
    {

    }

    public override void Execute()
    {
        Collider[] hitColliders = Physics.OverlapSphere(Blackboard.myInfo.Transform.Position, 800f);
        if (hitColliders != null)
        {
            foreach (var col in hitColliders)
            {
                //var collider = Blackboard.gameWorldUtils ;
                //if (collider.)
                //{
                //    AIActionDash dash = new AIActionDash();
                //    dash.Direction = Vector3.forward; // test 
                //    Blackboard.actionList.Add(dash);
                //}
            }
        }
    }

}
