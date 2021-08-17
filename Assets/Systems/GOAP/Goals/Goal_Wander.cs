using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal_Wander : Goal_Base
{
    [SerializeField] int WanderPriority = 30;

    [SerializeField] float PriorityBuildRate = 1f;
    [SerializeField] float PriorityDecayRate = 0.1f;
    float CurrentPriority = 0f;

    public override void OnTickGoal()
    {
        if (Agent.IsMoving)
            CurrentPriority -= PriorityDecayRate * Time.deltaTime;
        else
            CurrentPriority += PriorityBuildRate * Time.deltaTime;
    }

    public override void OnGoalActivated(Action_Base _linkedAction)
    {
        base.OnGoalActivated(_linkedAction);
        
        CurrentPriority = WanderPriority;
    }

    public override int CalculatePriority()
    {
        return Mathf.FloorToInt(CurrentPriority);
    }

    public override bool CanRun()
    {
        return true;
    }
}
