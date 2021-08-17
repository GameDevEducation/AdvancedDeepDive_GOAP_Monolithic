using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public enum PauseMode
    {
        Invalid,

        ComponentsMustRegister,
        FindActiveComponents,
        FindAllComponents
    }

    #pragma warning disable 0649
    [Header("Mode")]
    [SerializeField] PauseMode Mode = PauseMode.FindActiveComponents;

    [Header("Options")]
    [SerializeField] bool Pause2DPhysics = false;
    [SerializeField] bool Pause3DPhysics = false;
    [SerializeField] bool ChangeTimescale = false;
    private float OldTimescale = 1f;

    [SerializeField] UnityEvent OnPause;
    [SerializeField] UnityEvent OnResume;
    #pragma warning restore 0649

    private bool IsPaused_Internal = false;
    private List<IPausable> PausableComponents = null;

    private static PauseManager _Instance = null;

    public static PauseManager Instance
    {
        get
        {
            return _Instance;
        }
    }

    public static bool IsPaused
    {
        get
        {
            return PauseManager.Instance != null && PauseManager.Instance.IsPaused_Internal;
        }
    }

    void Awake()
    {
        if (_Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        _Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool RequestPause()
    {
        // already paused
        if (IsPaused_Internal)
            return true;

        // generate list of pausable components at this point
        if (Mode == PauseMode.FindActiveComponents || Mode == PauseMode.FindAllComponents)
        {
            bool includeInactive = Mode == PauseMode.FindAllComponents;

            PausableComponents = new List<IPausable>();

            // traverse all scenes
            for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; ++sceneIndex)
            {
                Scene activeScene = SceneManager.GetSceneAt(sceneIndex);

                // get the root objects
                GameObject[] rootObjects = activeScene.GetRootGameObjects();

                // get the list of interfaces
                foreach(GameObject rootObject in rootObjects)
                {
                    PausableComponents.AddRange(rootObject.GetComponentsInChildren<IPausable>(includeInactive));
                }
            }
        }

        bool allowPause = true;

        // check if there are pausable components
        if (PausableComponents != null)
        {
            // give each component the option to inhibit pausing
            foreach(IPausable pausable in PausableComponents)
            {
                if (!pausable.OnPauseRequested())
                {
                    allowPause = false;
                    break;
                }
            }
        }

        // is pausing allowed?
        if (allowPause)
        {
            Internal_Pause();
            return true;
        }

        return false;
    }

    private void Internal_Pause()
    {
        // are there pausable components
        if (PausableComponents != null)
        {
            // notify of pausing
            foreach(IPausable pausable in PausableComponents)
            {
                pausable.OnPause();
            }
        }

        // emit the event
        OnPause?.Invoke();

        // process the advanced options
        if (Pause2DPhysics)
            Physics2D.simulationMode = SimulationMode2D.Script;
        if (Pause3DPhysics)
            Physics.autoSimulation = false;
        if (ChangeTimescale)
        {
            OldTimescale = Time.timeScale;
            Time.timeScale = 0f;
        }

        // state only changes once everything is updated
        IsPaused_Internal = true;
    }

    public bool RequestResume()
    {
        // already resumed
        if (!IsPaused_Internal)
            return true;

        bool allowResume = true;

        // check if there are pausable components
        if (PausableComponents != null)
        {
            // give each component the option to inhibit resuming
            foreach(IPausable pausable in PausableComponents)
            {
                if (!pausable.OnPauseRequested())
                {
                    allowResume = false;
                    break;
                }
            }
        }

        // is resuming allowed?
        if (allowResume)
        {
            Internal_Resume();
            return true;
        }

        return false;
    }

    private void Internal_Resume()
    {
        // are there pausable components
        if (PausableComponents != null)
        {
            // notify of resuming
            foreach(IPausable pausable in PausableComponents)
            {
                pausable.OnResume();
            }
        }

        // emit the event
        OnResume?.Invoke();

        // process the advanced options
        if (Pause2DPhysics)
            Physics2D.simulationMode = SimulationMode2D.FixedUpdate;
        if (Pause3DPhysics)
            Physics.autoSimulation = true;
        if (ChangeTimescale)
            Time.timeScale = OldTimescale;

        // state only changes once everything is updated
        IsPaused_Internal = false;
    }

    public void RegisterPausable(IPausable pausable)
    {
        if (PausableComponents == null)
            PausableComponents = new List<IPausable>();

        PausableComponents.Add(pausable);
    }

    public void DeregisterPausable(IPausable pausable)
    {
        PausableComponents.Remove(pausable);
    }
}
