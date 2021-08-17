using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyAI))]
public class HearingSensor : MonoBehaviour
{
    EnemyAI LinkedAI;
    
    // Start is called before the first frame update
    void Start()
    {
        LinkedAI = GetComponent<EnemyAI>();
        HearingManager.Instance.Register(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDestroy()
    {
        if (HearingManager.Instance != null)
            HearingManager.Instance.Deregister(this);
    }

    public void OnHeardSound(GameObject source, Vector3 location, EHeardSoundCategory category, float intensity)
    {
        // outside of hearing range
        if (Vector3.Distance(location, LinkedAI.EyeLocation) > LinkedAI.HearingRange)
            return;

        LinkedAI.ReportCanHear(source, location, category, intensity);
    }
}
