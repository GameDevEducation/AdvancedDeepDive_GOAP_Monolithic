using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GOAPUI : MonoBehaviour
{
    [SerializeField] GameObject GoalPrefab;
    [SerializeField] RectTransform GoalRoot;

    Dictionary<MonoBehaviour, GoalUI> DisplayedGoals = new Dictionary<MonoBehaviour, GoalUI>();

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateGoal(MonoBehaviour goal, string _name, string _status, float _priority)
    {
        // add if not present
        if (!DisplayedGoals.ContainsKey(goal))
            DisplayedGoals[goal] = Instantiate(GoalPrefab, Vector3.zero, Quaternion.identity, GoalRoot).GetComponent<GoalUI>();

        DisplayedGoals[goal].UpdateGoalInfo(_name, _status, _priority);
    }
}
