using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GoalUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI Name;
    [SerializeField] Slider Priority;
    [SerializeField] TextMeshProUGUI Status;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateGoalInfo(string _name, string _status, float _priority)
    {
        Name.text = _name;
        Status.text = _status;
        Priority.value = _priority;
    }
}
