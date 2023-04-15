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
    public static float BestDistanceToDash;
    public static List<Vector3> dd = new List<Vector3>();
    


    public static void Initialize(int _AIId, GameWorldUtils _gameWorldUtils)
    {
        nodeList = new List<Node>();
        actionList = new List<AIAction>();
        gameWorldUtils = _gameWorldUtils;
        AIId = _AIId;
        firstNode = new Node();
        BestDistanceToFire = 45.0f;
        BestDistanceToDash = 65.0f;
        dd.Add(new Vector3(1, 0, 0));
        dd.Add(new Vector3(-1, 0, 0));
        dd.Add(new Vector3(0, 0, 1));
        dd.Add(new Vector3(0, 0, -1));
        dd.Add(new Vector3(1, 0, -1));
        dd.Add(new Vector3(-1, 0, -1));
        dd.Add(new Vector3(-1, 0, 1));
        dd.Add(new Vector3(1, 0, 1));
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
            
        }
        myInfo = GetPlayerInfos(AIId, playerInfos);
        actionList = new List<AIAction>();
        firstNode.Execute();
    }
}

public class MoveToNearestTarget : Selector
{
    //cherche le jouer le plus proche et tire
    public MoveToNearestTarget()
    {
        
    }

    public override void Execute()
    {
        if (Vector3.Distance(Blackboard.myInfo.Transform.Position, Blackboard.target.Transform.Position) < Blackboard.BestDistanceToFire)
        {
            AIActionMoveToDestination actionMove = new AIActionMoveToDestination();
            actionMove.Position = Blackboard.target.Transform.Position;

            AIActionLookAtPosition actionLookAt = new AIActionLookAtPosition();
            actionLookAt.Position = Blackboard.target.Transform.Position;
            Blackboard.actionList.Add(actionMove);
            Blackboard.actionList.Add(actionLookAt);
            Blackboard.actionList.Add(new AIActionFire());
            FindTargetLowestHealth kill = new FindTargetLowestHealth();
            Blackboard.nodeList.Add(kill);
            Blackboard.nodeList[0].Execute();
            Blackboard.nodeList[1].Execute();
        }
        else
        {
            AIActionMoveToDestination actionMove = new AIActionMoveToDestination();
            actionMove.Position = Blackboard.target.Transform.Position;
            AIActionLookAtPosition actionLookAt = new AIActionLookAtPosition();
            actionLookAt.Position = Blackboard.target.Transform.Position;
            Blackboard.actionList.Add(actionMove);
            Blackboard.actionList.Add(actionLookAt);
            Blackboard.actionList.Add(new AIActionFire());


            
            Blackboard.nodeList[0].Execute();
            Blackboard.nodeList[1].Execute();
            Blackboard.nodeList[2].Execute();
        }

    }

}

public class FindTargetLowestHealth : Node
{
    //trouver le joueur avec le moins de vie
    
    public FindTargetLowestHealth()
    {
        float curHealth = 999999999999999f;
        List<PlayerInformations> playerInfos = Blackboard.gameWorldUtils.GetPlayerInfosList();
        
        foreach (PlayerInformations playerInfo in playerInfos)
        {
            if (Blackboard.target.CurrentHealth < (playerInfo.MaxHealth * 30 / 100))
            {
                Blackboard.target = playerInfo;
                Blackboard.actionList.Add(new AIActionFire());
            }else if (Vector3.Distance(Blackboard.myInfo.Transform.Position, playerInfo.Transform.Position) < 60f)
            {
                Blackboard.target = playerInfo;
                Blackboard.actionList.Add(new AIActionFire());
            }
            

        }
        
    }

    public override void Execute()
    {
        
    }
}

public class NeedHealth : Sequence
{
    public NeedHealth():base()
    {
        FindBonus findHealth = new FindBonus();
        Blackboard.nodeList.Add(findHealth);
    }
}

public class FindBonus : Node
{
    //cherche un bonus en fonction de certaines conditions (si pas en duel,si bonus dispo)

    public override void Execute()
    {
        List<BonusInformations> bonusInfos = Blackboard.gameWorldUtils.GetBonusInfosList();
        //Debug.LogError("nd");
        foreach (BonusInformations bonus in bonusInfos)
        {
            if(bonus.Type == EBonusType.Health && Blackboard.myInfo.CurrentHealth < (Blackboard.myInfo.MaxHealth * 35 / 100))
            {
                AIActionMoveToDestination actionMove = new AIActionMoveToDestination();
                actionMove.Position = bonus.Position;
                if (Vector3.Distance(Blackboard.myInfo.Transform.Position, bonus.Position) < 55.0f)
                {
                    AIActionDash dash = new AIActionDash();
                    dash.Direction = bonus.Position;
                    Blackboard.actionList.Add(dash);
                }
                Blackboard.actionList.Add(actionMove);
            }
            else
            {
                AIActionMoveToDestination actionMove = new AIActionMoveToDestination();
                actionMove.Position = bonus.Position;
                if(Vector3.Distance(Blackboard.myInfo.Transform.Position, bonus.Position) < 25.0f)
                {
                    AIActionDash dash = new AIActionDash();
                    dash.Direction = bonus.Position;
                Blackboard.actionList.Add(dash);
                }
                Blackboard.actionList.Add(actionMove);
            }

        }
            
    }
}

public class NeedDash : Condition
{
    //creer un sphere virtuel et si ball dedans dash direction libre
    public NeedDash()
    {

    }

    public override void Execute()
    {
        
        var hitColliders = Physics.OverlapSphere(Blackboard.myInfo.Transform.Position,Blackboard.BestDistanceToDash);
        var balls = Blackboard.gameWorldUtils.GetProjectileInfosList();
        if (hitColliders != null)
        {
            foreach (var col in hitColliders)
            {
                foreach(var bullet in balls)
                {
                    float angle = 0f;
                    float radius = 20f;
                    float perSecond = 30f;
                    angle += perSecond * Time.deltaTime;
                    if (angle > 360)
                    {
                        angle = -360;
                    }

                    int rd = UnityEngine.Random.Range(0, Blackboard.dd.Count);
                    var orbit = Vector3.forward * radius;
                    orbit = Quaternion.Euler(0, angle, 0) * orbit;
                    AIActionMoveToDestination actionMove = new AIActionMoveToDestination();
                    actionMove.Position = bullet.Transform.Position + orbit;
                    AIActionLookAtPosition actionLookAt = new AIActionLookAtPosition();
                        actionLookAt.Position = Blackboard.target.Transform.Position;
                    if (Blackboard.myInfo.IsDashAvailable == true)
                    {
                        AIActionDash dash = new AIActionDash();
                        //verifier direction possible
                        //test droit
                        //RaycastHit hit;
                        //if(Physics.Raycast(Blackboard.myInfo.Transform.Position, Vector3.right,out hit,10f))
                        //{
                        //    if(hit.collider.gameObject.layer != Blackboard.gameWorldUtils.BonusLayerMask &&
                        //        hit.collider.gameObject.layer != Blackboard.gameWorldUtils.PlayerLayerMask &&
                        //        hit.collider.gameObject.layer != Blackboard.gameWorldUtils.ProjectileLayerMask)
                        //    {
                        //        Debug.LogError("mur a droite");
                        //    }
                        //    else
                        //    {
                        //        Debug.LogError("mur pas a droite");

                        //    }
                        //}
                        dash.Direction = (bullet.Transform.Rotation * Blackboard.dd[rd]).normalized;
                        Blackboard.actionList.Add(dash);
                    }
                    else
                    {
                        //actionMove.Position = (bullet.Transform.Rotation * dd[rd]).normalized;
                        //actionLookAt.Position = Blackboard.target.Transform.Position;
                        DodgeWithoutDash wdd = new DodgeWithoutDash();
                        Blackboard.nodeList.Add(wdd);
                    }
                    
                    Blackboard.actionList.Add(actionMove);
                    Blackboard.actionList.Add(actionLookAt);
                    Blackboard.actionList.Add(new AIActionFire());
                }
            }
        }
    }

}

public class DodgeWithoutDash : Sequence
{
    public DodgeWithoutDash()
    {

    }


    public override void Execute()
    {
        if (Vector3.Distance(Blackboard.myInfo.Transform.Position, Blackboard.target.Transform.Position) < 550.0f)
        {
            float angle = 0f;
            float radius = 20f;
            float perSecond = 30f;
            AIActionMoveToDestination actionMove = new AIActionMoveToDestination();
            angle += perSecond * Time.deltaTime;
            if(angle > 360)
            {
                angle = -360;
            }
            
            var orbit = Vector3.forward * radius;
            orbit = Quaternion.Euler(0, angle, 0) * orbit;
            actionMove.Position = Blackboard.target.Transform.Position + orbit;
            



            AIActionLookAtPosition actionLookAt = new AIActionLookAtPosition();
            actionLookAt.Position = Blackboard.target.Transform.Position;
            Blackboard.actionList.Add(actionMove);
            Blackboard.actionList.Add(actionLookAt);
            Blackboard.actionList.Add(new AIActionFire());
        }

    }
}