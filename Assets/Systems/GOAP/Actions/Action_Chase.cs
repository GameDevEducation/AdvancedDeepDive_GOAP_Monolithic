using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_Chase : Action_Base
{
    List<System.Type> SupportedGoals = new List<System.Type>(new System.Type[] { typeof(Goal_Chase) });

    Goal_Chase ChaseGoal;

    public override List<System.Type> GetSupportedGoals()
    {
        return SupportedGoals;
    }

    public override float GetCost()
    {
        return 0f;
    }

    public override void OnActivated(Goal_Base _linkedGoal)
    {
        base.OnActivated(_linkedGoal);
        
        // cache the chase goal
        ChaseGoal = (Goal_Chase)LinkedGoal;

        Agent.MoveTo(ChaseGoal.MoveTarget);
    }

    public override void OnDeactivated()
    {
        base.OnDeactivated();
        
        ChaseGoal = null;
    }

    public override void OnTick()
    {
        Agent.MoveTo(ChaseGoal.MoveTarget);
    }    
}
