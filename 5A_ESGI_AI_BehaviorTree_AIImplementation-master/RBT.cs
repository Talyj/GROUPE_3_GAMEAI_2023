using AI_BehaviorTree_AIGameUtility;
using BehaviorTree_ESGI;

public class ShootAction : Node
{
    public ShootAction() { }

    override
    public void Execute()
    {
        new AIActionFire();
    }
}
