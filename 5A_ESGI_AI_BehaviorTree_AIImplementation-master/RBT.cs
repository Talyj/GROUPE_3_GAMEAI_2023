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

public class Blackboard
{
    public static GameWorldUtils gameWorldUtils;
    public static List<Node> nodeList;
    public static List<AIAction> actionList;
    public static PlayerInformations target;
    public static PlayerInformations myInfo;
    public static int AIId;
    public static Selector firstNode;
    public static float BestDistanceToFire;


    public static void Initialize(int aiid, GameWorldUtils _gameWorldUtils)
    {
        nodeList = new List<Node>();
        gameWorldUtils = _gameWorldUtils;
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

            if (playerInfo.PlayerId == myInfo.PlayerId)
                continue;

            target = playerInfo;
            break;
        }
        myInfo = GetPlayerInfos(AIId, playerInfos);
        actionList = new List<AIAction>();
        firstNode.Execute();
    }
}
