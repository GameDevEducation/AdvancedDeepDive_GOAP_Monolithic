using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

static public class SettingsBootstrap
{
    const string SceneName = "SettingsBootstrap";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void LoadSettingsScene()
    {
        for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; ++sceneIndex)
        {
            // retrieve the scene
            Scene candidate = SceneManager.GetSceneAt(sceneIndex);

            // is this the settings bootstrap?
            if (candidate.name == SceneName)
                return;
        }

        SceneManager.LoadScene(SceneName, LoadSceneMode.Additive);
    }
}
