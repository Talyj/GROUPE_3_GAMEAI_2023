using AI_BehaviorTree_AIGameUtility;
using BehaviorTree_ESGI;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Assertions;

public class Blackboard 
{
    public List<AIAction> actionList = new List<AIAction>();
    public List<ProjectileInformations> projectileInformations = new List<ProjectileInformations>();
    public List<BonusInformations> bonusInformations = new List<BonusInformations>();

    public PlayerInformations player;
    public PlayerInformations target;

    public BonusInformations bonusTarget;
}

// *********************************************************************************
// ACTIONS
// *********************************************************************************

public class PatrolToBonusAction : Node // Se déplacer vers un bonus
{
    private Blackboard blackboard;

    private bool moveToA;

    public PatrolToBonusAction(Blackboard blackboard)
    {
        this.blackboard = blackboard;
    }

    public override void Execute()
    {
        this.state = NodeState.Success;
        AIActionMoveToDestination actionMove = new AIActionMoveToDestination(blackboard.bonusTarget.Position);
        blackboard.actionList.Add(actionMove);
    }
}

public class DodgeAction : Node // Se déplacer autour du target
{
    private Blackboard blackboard;
    private float thresholdDistance = 10.0f;
    private bool moveToA;
    private float dodgeDistance = 10.0f;

    public DodgeAction(Blackboard blackboard)
    {
        this.blackboard = blackboard;
        this.moveToA = true;
    }

    public override void Execute()
    {
        Vector3 currentPosition = blackboard.player.Transform.Position;
        Vector3 targetPosition = blackboard.target.Transform.Position;

        if (Vector3.Distance(currentPosition, targetPosition) <= thresholdDistance)
        {
            moveToA = !moveToA;
            this.state = NodeState.Success;
        }
        else
        {
            // Calculer la direction de la balle la plus proche
            Vector3 dodgeDirection = Vector3.zero;
            float closestDistance = Mathf.Infinity;
            foreach (var projectile in blackboard.projectileInformations)
            {
                float distance = Vector3.Distance(currentPosition, projectile.Transform.Position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    dodgeDirection = (projectile.Transform.Position - currentPosition).normalized;
                }
            }

            // Calculer le vecteur perpendiculaire à la direction de la balle
            Vector3 perpendicularVector = Vector3.Cross(dodgeDirection, Vector3.up);

            // Esquiver la balle en marchant latéralement dans la direction perpendiculaire
            Vector3 dodgePosition = currentPosition + perpendicularVector * dodgeDistance;
            AIActionMoveToDestination actionMove = new AIActionMoveToDestination();
            if(blackboard.projectileInformations.Count != 0)
            {
                actionMove.Position = dodgePosition;
            } else
            {
                actionMove.Position = targetPosition;
            }
            blackboard.actionList.Add(actionMove);
            this.state = NodeState.Running;
        }
    }
}

public class DashOut : Node // Dash sur les côtés pour éviter un projectile
{
    private Blackboard blackboard;

    public DashOut(Blackboard blackboard)
    {
        this.blackboard = blackboard;
    }

    public override void Execute()
    {
        Vector3 dodgeDirection = Vector3.zero;
        float closestDistance = Mathf.Infinity;

        foreach (var projectile in blackboard.projectileInformations)
        {
            float distance = Vector3.Distance(blackboard.player.Transform.Position, projectile.Transform.Position);
            if (distance < closestDistance) {
                closestDistance = distance;
                dodgeDirection = projectile.Transform.Position;
            }
        }

        Vector3 targetPosition = blackboard.player.Transform.Position + Vector3.Distance(dodgeDirection, blackboard.player.Transform.Position) * (blackboard.player.Transform.Rotation * Vector3.forward);

        // Calcule le vecteur perpendiculaire à la direction de la balle et lance le dash vers ce vector
        Vector3 dodgePosition = blackboard.player.Transform.Position + Vector3.Cross(targetPosition, Vector3.up);
                
        AIActionDash actionDash = new AIActionDash(dodgePosition);
        blackboard.actionList.Add(actionDash);
        this.state = NodeState.Success;
    }
}

public class DashToBonus : Node //Dash vers le bonus pour aller plus vite
{
    private Blackboard blackboard;

    public DashToBonus(Blackboard blackboard)
    {
        this.blackboard = blackboard;
    }

    public override void Execute()
    {
        Vector3 targetPosition = blackboard.player.Transform.Position + Vector3.Distance(blackboard.bonusTarget.Position, blackboard.player.Transform.Position) * (blackboard.player.Transform.Rotation * Vector3.forward);

        AIActionDash actionDash = new AIActionDash(targetPosition - blackboard.player.Transform.Position);
        blackboard.actionList.Add(actionDash);
        this.state = NodeState.Success;
    }
}

public class ShootAction : Node // Tirer
{
    private Blackboard blackboard;
    public ShootAction(Blackboard blackboard)
    {
        this.blackboard = blackboard;
    }
    public override void Execute()
    {
        this.state = NodeState.Success;
        blackboard.actionList.Add(new AIActionFire());
    }
}

public class LookAtBonus : Node // Regarde le bonus choisi 
{
    private Blackboard blackboard;

    public LookAtBonus(Blackboard blackboard)
    {
        this.blackboard = blackboard;
    }
    public override void Execute()
    {
        AIActionLookAtPosition actionLookAtPosition = new AIActionLookAtPosition(blackboard.bonusTarget.Position);
        blackboard.actionList.Add(actionLookAtPosition);
        this.state = NodeState.Success;
    }
}

public class LookAtTargetAction : Node // Regarde la target 
{
    private Blackboard blackboard;

    public LookAtTargetAction(Blackboard blackboard)
    {
        this.blackboard = blackboard;
    }
    public override void Execute()
    {
        AIActionLookAtPosition actionLookAtPosition = new AIActionLookAtPosition(blackboard.target.Transform.Position);
        blackboard.actionList.Add(actionLookAtPosition);
        this.state = NodeState.Success;
    }
}

public class StopMovementAction : Node // Arrete de se déplacer
{
    private Blackboard blackboard;
    public StopMovementAction(Blackboard blackboard)
    {
        this.blackboard = blackboard;
    }
    public override void Execute()
    {
        this.state = NodeState.Success;
        blackboard.actionList.Add(new AIActionStopMovement());
    }
}

// *********************************************************************************
// CONDITIONS
// *********************************************************************************

public class IsShootedOn : Condition // Renvoie TRUE si un projective est proche de lui par rapport à un certain seuil
{
    private Blackboard blackboard;
    public IsShootedOn(Blackboard bb)  
    {
        this.blackboard = bb;
    }

    public override bool Check()
    {
        if (blackboard.projectileInformations.Count != 0 && blackboard.target != null)
        {
            foreach (var ve in blackboard.projectileInformations)
            {
                // Si un projectile est proche 
                if (Vector3.Distance(blackboard.player.Transform.Position, ve.Transform.Position) < 7.0)
                {
                    return true;
                }
                else
                {
                    continue;
                }
            }

            return false;
        }
        return false;
    }
}

public class IsDashAvailable : Condition //Renvoie TRUE si le dash est utilisable
{
    private Blackboard blackboard;
    public IsDashAvailable(Blackboard bb)
    {
        this.blackboard = bb;
    }

    public override bool Check()
    {
        return blackboard.player.IsDashAvailable;
    }
}

public class IsTargetActive : Condition //Renvoie TRUE si la target est en vie
{
    private Blackboard blackboard;
    public IsTargetActive(Blackboard bb)
    {
        this.blackboard = bb;
    }

    public override bool Check()
    {
        return blackboard.target != null;
    }
}

public class IsBonusIsAvailable : Condition //Renvoie TRUE si un bonus est disponible sur la map
{
    private Blackboard blackboard;
    public IsBonusIsAvailable(Blackboard bb)
    {
        this.blackboard = bb;
    }

    public override bool Check()
    {
        if(blackboard.bonusInformations.Count != 0)
        {
            return true;
        }
        return false;
    }
}

public class IsMoreClose : Condition //Renvoie TRUE si *
{
    private Blackboard blackboard;
    public IsMoreClose(Blackboard bb)
    {
        this.blackboard = bb;
    }

    public override bool Check()
    {
        Vector3 bonusMoreCloser = Vector3.zero;
        foreach(BonusInformations bonus in this.blackboard.bonusInformations)
        {
            if(Vector3.Distance(blackboard.player.Transform.Position, bonus.Position) < Vector3.Distance(this.blackboard.player.Transform.Position, bonusMoreCloser))
            {
                bonusMoreCloser = bonus.Position;
            }
        }
        return false;
        //return Vector3.Distance(this.blackboard.player.Transform.Position, this.blackboard.bonusInformations.) < Vector3.Distance(this.blackboard.player.Transform.Position, this.blackboard.target.Transform.Position);
    }
}

public class IsMoreCloseBonus : Condition //Renvoie TRUE si le bonus est plus proche que la target
{
    private Blackboard blackboard;
    public IsMoreCloseBonus(Blackboard bb)
    {
        this.blackboard = bb;
    }

    public override bool Check()
    {
        return Vector3.Distance(this.blackboard.player.Transform.Position, this.blackboard.bonusTarget.Position) < Vector3.Distance(this.blackboard.player.Transform.Position, this.blackboard.target.Transform.Position);
    }
}

public class IsCriticalState : Condition //Renvoie TRUE si le joueur est en état critique (-30% vie)
{
    private Blackboard blackboard;
    public IsCriticalState(Blackboard bb)
    {
        this.blackboard = bb;
    }

    public override bool Check()
    {
        return blackboard.player.CurrentHealth < (blackboard.player.MaxHealth*0.3);
    }
}

public class IsBonusLifeAvailable : Condition //Renvoie TRUE si un bonus de vie est disponible
{
    private Blackboard blackboard;
    public IsBonusLifeAvailable(Blackboard bb)
    {
        this.blackboard = bb;
    }

    public override bool Check()
    {
        return blackboard.bonusTarget.Type == EBonusType.Health;
    }
}