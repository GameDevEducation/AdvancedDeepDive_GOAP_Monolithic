using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_Base : MonoBehaviour
{
    protected CharacterAgent Agent;
    protected AwarenessSystem Sensors;
    protected Goal_Base LinkedGoal;

    void Awake()
    {
        Agent = GetComponent<CharacterAgent>();
        Sensors = GetComponent<AwarenessSystem>();
    }

    public virtual List<System.Type> GetSupportedGoals()
    {
        return null;
    }

    public virtual float GetCost()
    {
        return 0f;
    }

    public virtual void OnActivated(Goal_Base _linkedGoal)
    {
        LinkedGoal = _linkedGoal;
    }

    public virtual void OnDeactivated()
    {
        LinkedGoal = null;
    }

    public virtual void OnTick()
    {

    }
}
