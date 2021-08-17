using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPausable
{
    bool OnPauseRequested();

    bool OnResumeRequested();

    void OnPause();

    void OnResume();
}