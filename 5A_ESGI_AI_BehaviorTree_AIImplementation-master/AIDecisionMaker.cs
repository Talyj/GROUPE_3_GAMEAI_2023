using AI_BehaviorTree_AIGameUtility;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using BehaviorTree_ESGI;
using JetBrains.Annotations;

namespace AI_BehaviorTree_AIImplementation
{
    public class AIDecisionMaker
    {
        /// <summary>
        /// Ne pas supprimer des fonctions, ou changer leur signature sinon la DLL ne fonctionnera plus
        /// Vous pouvez unitquement modifier l'intérieur des fonctions si nécessaire (par exemple le nom)
        /// ComputeAIDecision en fait partit
        /// </summary>
        private int AIId = -1;
        public GameWorldUtils AIGameWorldUtils = new GameWorldUtils();
        private Blackboard blackboard;
        private Selector rootNode;
        // Ne pas utiliser cette fonction, elle n'est utile que pour le jeu qui vous Set votre Id, si vous voulez votre Id utilisez AIId
        public void SetAIId(int parAIId) 
        { 
            AIId = parAIId;

            blackboard = new Blackboard();
            rootNode = new Selector();

            Sequence cibleActiveSeq = new Sequence();
            cibleActiveSeq.nodes.Add(new IsTargetActive(blackboard));
            cibleActiveSeq.nodes.Add(new LookAtTargetAction(blackboard));
            cibleActiveSeq.nodes.Add(new ShootAction(blackboard));
            cibleActiveSeq.nodes.Add(new ForceFailure());

            rootNode.nodes.Add(cibleActiveSeq);

            Sequence criticalSequence = new Sequence();
            criticalSequence.nodes.Add(new IsCriticalState(blackboard));
            criticalSequence.nodes.Add(new IsBonusLifeAvailable(blackboard));
            criticalSequence.nodes.Add(new PatrolToBonusAction(blackboard));

            rootNode.nodes.Add(criticalSequence);

            Sequence bonusSequence = new Sequence();
            bonusSequence.nodes.Add(new IsBonusIsAvailable(blackboard));
            bonusSequence.nodes.Add(new PatrolToBonusAction(blackboard));

            /*Selector bonusSelector = new Selector();
            Sequence bonusTargetActiveSeq = new Sequence();
            bonusTargetActiveSeq.nodes.Add(new IsTargetActive(blackboard));
            bonusTargetActiveSeq.nodes.Add(new IsMoreCloseBonus(blackboard));
            bonusTargetActiveSeq.nodes.Add(new PatrolToBonusAction(blackboard));
            bonusSelector.nodes.Add(bonusTargetActiveSeq);

            Sequence bonusNoTargetSeq = new Sequence();
            bonusNoTargetSeq.nodes.Add(new PatrolToBonusAction(blackboard));
            bonusSelector.nodes.Add(bonusNoTargetSeq);*/

            rootNode.nodes.Add(bonusSequence);
            
            Sequence dodgeSeq = new Sequence();
            dodgeSeq.nodes.Add(new IsTargetActive(blackboard));
            dodgeSeq.nodes.Add(new DodgeAction(blackboard));
            dodgeSeq.nodes.Add(new IsShootedOn(blackboard));
            dodgeSeq.nodes.Add(new IsDashAvailable(blackboard));
            dodgeSeq.nodes.Add(new DashOut(blackboard));

            rootNode.nodes.Add(dodgeSeq);
        }

        // Vous pouvez modifier le contenu de cette fonction pour modifier votre nom en jeu
        public string GetName() { return "SEPIIROTH"; }

        public void SetAIGameWorldUtils(GameWorldUtils parGameWorldUtils) { AIGameWorldUtils = parGameWorldUtils; }

        //Fin du bloc de fonction nécessaire (Attention ComputeAIDecision en fait aussi partit)

        public void UpdateBlackboard(List<ProjectileInformations> pI, List<BonusInformations> bI, PlayerInformations player, PlayerInformations targ, BonusInformations bonus)
        {
            blackboard.actionList = new List<AIAction>();
            blackboard.projectileInformations = pI;
            blackboard.bonusInformations = bI;
            blackboard.player = player;
            blackboard.target = targ;
            blackboard.bonusTarget = bonus;
        }

        public List<AIAction> ComputeAIDecision()
        {
            List<PlayerInformations> playerInfos = AIGameWorldUtils.GetPlayerInfosList();
            List<ProjectileInformations> projectileInformations = AIGameWorldUtils.GetProjectileInfosList();
            List<BonusInformations> bonusInfos = AIGameWorldUtils.GetBonusInfosList();

            PlayerInformations target = null;
            List<ProjectileInformations> projInf = new List<ProjectileInformations>();
            BonusInformations bonusTarget = new BonusInformations();

            PlayerInformations myPlayerInfo = GetPlayerInfos(AIId, playerInfos);
            if (myPlayerInfo == null)
                return blackboard.actionList;

            PlayerInformations closestTarget = null;
            float minDistance = float.MaxValue;

            Vector3 yourPosition = myPlayerInfo.Transform.Position;

            foreach (PlayerInformations playerInfo in playerInfos)
            {
                if (!playerInfo.IsActive)
                    continue;

                if (playerInfo.PlayerId == AIId)
                    continue;

                float distance = Vector3.Distance(myPlayerInfo.Transform.Position, playerInfo.Transform.Position);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestTarget = playerInfo;
                }
            }

            if (closestTarget != null)
            {
                // Faire le target ici
                target = closestTarget;
            }            

            //Initialisation d'une liste des projectiles qui ne font pas parti de ceux tiré par le personnage
            foreach (ProjectileInformations projInformations in projectileInformations)
            {
                if (projInformations.PlayerId != AIId)
                {
                    projInf.Add(projInformations);
                }
            }

            //Initialisation du bonusTarget
            if (bonusInfos.Count != 0)
            {
                bonusTarget.Position = bonusInfos[0].Position;
                foreach (BonusInformations bonus in bonusInfos)
                {
                    if (Vector3.Distance(myPlayerInfo.Transform.Position, bonus.Position) < Vector3.Distance(myPlayerInfo.Transform.Position, bonusTarget.Position))
                    {
                        bonusTarget.Position = bonus.Position;
                    }
                    
                    if(bonus.Type == EBonusType.Health) // Si Bonus de vie => Prend le dessus car + important
                    {
                        bonusTarget.Position = bonus.Position;
                        break;
                    }
                }
            }

            //Update 
            UpdateBlackboard(projInf, bonusInfos, myPlayerInfo, target, bonusTarget);  
            rootNode.Execute();

            return blackboard.actionList;
        }

        public PlayerInformations GetPlayerInfos(int parPlayerId, List<PlayerInformations> parPlayerInfosList)
        {
            foreach (PlayerInformations playerInfo in parPlayerInfosList)
            {
                if (playerInfo.PlayerId == parPlayerId)
                    return playerInfo;
            }

            Assert.IsTrue(false, "GetPlayerInfos : PlayerId not Found");
            return null;
        }
    }

}
