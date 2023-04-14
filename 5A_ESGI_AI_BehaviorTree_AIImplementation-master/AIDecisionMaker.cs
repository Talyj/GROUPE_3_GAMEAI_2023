using AI_BehaviorTree_AIGameUtility;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

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
        // Ne pas utiliser cette fonction, elle n'est utile que pour le jeu qui vous Set votre Id, si vous voulez votre Id utilisez AIId
        public void SetAIId(int parAIId) { AIId = parAIId;
            Blackboard.Initialize(AIId, AIGameWorldUtils);

            MoveToNearestTarget moveTo = new MoveToNearestTarget();
            Blackboard.firstNode = moveTo;
        }

        // Vous pouvez modifier le contenu de cette fonction pour modifier votre nom en jeu
        public string GetName() { return "Le_R"; }

        public void SetAIGameWorldUtils(GameWorldUtils parGameWorldUtils) { AIGameWorldUtils = parGameWorldUtils;Blackboard.gameWorldUtils = parGameWorldUtils; }

        //Fin du bloc de fonction nécessaire (Attention ComputeAIDecision en fait aussi partit)

        

        public List<AIAction> ComputeAIDecision()
        {


            Blackboard.UpdateBlackboard(GetPlayerInfos);
            return Blackboard.actionList;
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
