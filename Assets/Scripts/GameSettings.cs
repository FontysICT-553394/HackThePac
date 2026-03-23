using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSettings : MonoBehaviour
{
    public static GameSettings instance { get; private set; }

    public float pacmanSpeed = 3f;
    public float ghostSpeed = 2f;
    public string selectedCharacter;
    
    // Hacks - PacMan
    [SerializeField] private bool freezeEnabled = false;
    [SerializeField] public List<GameObject> freezeHackUIElements = new();
    public event Action<bool> FreezeChanged;
    public void SubscribeFreezeChanged(Action<bool> handler) => FreezeChanged += handler;
    public void UnsubscribeFreezeChanged(Action<bool> handler) => FreezeChanged -= handler;
    public bool FreezeEnabled
    {
        get => freezeEnabled;
        set
        {
            if (freezeEnabled == value) return;
            freezeEnabled = value;
            FreezeChanged?.Invoke(value);
        }
    }

    [SerializeField] private bool cloneEnabled = false;
    [SerializeField] public List<GameObject> cloneHackUIElements = new();
    public event Action<bool> CloneChanged;
    public void SubscribeCloneChanged(Action<bool> handler) => CloneChanged += handler;
    public void UnsubscribeCloneChanged(Action<bool> handler) => CloneChanged -= handler;
    public bool CloneEnabled
    {
        get => cloneEnabled;
        set
        {
            if (cloneEnabled == value) return;
            cloneEnabled = value;
            CloneChanged?.Invoke(value);
        }
    }

    [SerializeField] private bool speedOverflowEnabled = false;
    [SerializeField] public List<GameObject> speedOverflowUIElements = new();
    public event Action<bool> SpeedOverflowChanged;
    public void SubscribeSpeedOverflowChanged(Action<bool> handler) => SpeedOverflowChanged += handler;
    public void UnsubscribeSpeedOverflowChanged(Action<bool> handler) => SpeedOverflowChanged -= handler;
    public bool SpeedOverflowEnabled
    {
        get => speedOverflowEnabled;
        set
        {
            if (speedOverflowEnabled == value) return;
            speedOverflowEnabled = value;
            SpeedOverflowChanged?.Invoke(value);
        }
    }

    // Hacks \- Ghost
    [SerializeField] private bool fearOverrideEnabled = false;
    [SerializeField] private List<GameObject> fearHackUIElements = new();
    public event Action<bool> FearOverrideChanged;
    public void SubscribeFearOverrideChanged(Action<bool> handler) => FearOverrideChanged += handler;
    public void UnsubscribeFearOverrideChanged(Action<bool> handler) => FearOverrideChanged -= handler;
    public bool FearOverrideEnabled
    {
        get => fearOverrideEnabled;
        set
        {
            if (fearOverrideEnabled == value) return;
            fearOverrideEnabled = value;
            FearOverrideChanged?.Invoke(value);
        }
    }

    [SerializeField] private bool visionHackEnabled = false;
    [SerializeField] private List<GameObject> visionHackUIElements = new();
    public event Action<bool> VisionHackChanged;
    public void SubscribeVisionHackChanged(Action<bool> handler) => VisionHackChanged += handler;
    public void UnsubscribeVisionHackChanged(Action<bool> handler) => VisionHackChanged -= handler;
    public bool VisionHackEnabled
    {
        get => visionHackEnabled;
        set
        {
            if (visionHackEnabled == value) return;
            visionHackEnabled = value;
            VisionHackChanged?.Invoke(value);
        }
    }

    [SerializeField] private bool wallPhaseEnabled = false;
    public event Action<bool> WallPhaseChanged;
    public void SubscribeWallPhaseChanged(Action<bool> handler) => WallPhaseChanged += handler;
    public void UnsubscribeWallPhaseChanged(Action<bool> handler) => WallPhaseChanged -= handler;
    public bool WallPhaseEnabled
    {
        get => wallPhaseEnabled;
        set
        {
            if (wallPhaseEnabled == value) return;
            wallPhaseEnabled = value;
            WallPhaseChanged?.Invoke(value);
        }
    }

    // Hacks \- General
    [SerializeField] private bool debugModeEnabled = false;
    public event Action<bool> DebugModeChanged;
    public void SubscribeDebugModeChanged(Action<bool> handler) => DebugModeChanged += handler;
    public void UnsubscribeDebugModeChanged(Action<bool> handler) => DebugModeChanged -= handler;
    public bool DebugModeEnabled
    {
        get => debugModeEnabled;
        set
        {
            if (debugModeEnabled == value) return;
            debugModeEnabled = value;
            DebugModeChanged?.Invoke(value);
        }
    }

    [SerializeField] private bool noClipEnabled = false;
    public event Action<bool> NoClipChanged;
    public void SubscribeNoClipChanged(Action<bool> handler) => NoClipChanged += handler;
    public void UnsubscribeNoClipChanged(Action<bool> handler) => NoClipChanged -= handler;
    public bool NoClipEnabled
    {
        get => noClipEnabled;
        set
        {
            if (noClipEnabled == value) return;
            noClipEnabled = value;
            NoClipChanged?.Invoke(value);
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Bind to events on the singleton instance
        instance.SubscribeFreezeChanged(OnFreezeChanged);
        instance.SubscribeCloneChanged(OnCloneChanged);
        instance.SubscribeSpeedOverflowChanged(OnSpeedOverflowChanged);
        instance.SubscribeFearOverrideChanged(OnFearOverrideChanged);
        instance.SubscribeVisionHackChanged(OnVisionHackChanged);
        instance.SubscribeWallPhaseChanged(OnWallPhaseChanged);
        instance.SubscribeDebugModeChanged(OnDebugModeChanged);
        instance.SubscribeNoClipChanged(OnNoClipChanged);

        // Initialize UI references for the currently active scene
        RebindSceneUI();
    }

    private void OnDestroy()
    {
        if (instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RebindSceneUI(scene);
        
        if(freezeEnabled) OnFreezeChanged(true);
        if(cloneEnabled) OnCloneChanged(true);
        if(speedOverflowEnabled) OnSpeedOverflowChanged(true);
        if(fearOverrideEnabled) OnFearOverrideChanged(true);
        if(visionHackEnabled) OnVisionHackChanged(true);
        if(wallPhaseEnabled) OnWallPhaseChanged(true);
        if(debugModeEnabled) OnDebugModeChanged(true);
        if(noClipEnabled) OnDebugModeChanged(true);
    }

    private void RebindSceneUI(Scene scene = default)
    {
        Scene activeScene = scene.Equals(default(Scene)) ? SceneManager.GetActiveScene() : scene;

        // First attempt: only finds active objects
        freezeHackUIElements = new List<GameObject>(GameObject.FindGameObjectsWithTag("FreezeUI"));
        cloneHackUIElements = new List<GameObject>(GameObject.FindGameObjectsWithTag("CloneUI"));
        speedOverflowUIElements = new List<GameObject>(GameObject.FindGameObjectsWithTag("SpeedOverflowUI"));
        fearHackUIElements = new List<GameObject>(GameObject.FindGameObjectsWithTag("FearOverrideUI"));
        visionHackUIElements = new List<GameObject>(GameObject.FindGameObjectsWithTag("VisionUI"));

        // Fallback: include inactive objects by searching all loaded GameObjects
        if (freezeHackUIElements.Count == 0 ||
            cloneHackUIElements.Count == 0 ||
            speedOverflowUIElements.Count == 0 ||
            fearHackUIElements.Count == 0 ||
            visionHackUIElements.Count == 0)
        {
            var all = Resources.FindObjectsOfTypeAll<GameObject>();

            // Clear and repopulate each list with matching objects in the active scene (includes inactive)
            freezeHackUIElements = new List<GameObject>();
            cloneHackUIElements = new List<GameObject>();
            speedOverflowUIElements = new List<GameObject>();
            fearHackUIElements = new List<GameObject>();
            visionHackUIElements = new List<GameObject>();

            foreach (var go in all)
            {
                if (go.scene != activeScene) continue; // only current scene
                if (go.CompareTag("FreezeUI")) freezeHackUIElements.Add(go);
                if (go.CompareTag("CloneUI")) cloneHackUIElements.Add(go);
                if (go.CompareTag("SpeedOverflowUI")) speedOverflowUIElements.Add(go);
                if (go.CompareTag("FearOverrideUI")) fearHackUIElements.Add(go);
                if (go.CompareTag("VisionUI")) visionHackUIElements.Add(go);
            }
        }
    }

    private void OnFreezeChanged(bool val)
    {
        foreach (var element in freezeHackUIElements)
            element.SetActive(val);
    }

    private void OnCloneChanged(bool val)
    {
        foreach (var element in cloneHackUIElements)
            element.SetActive(val);
    }

    private void OnSpeedOverflowChanged(bool val)
    {
        foreach (var element in speedOverflowUIElements)
            element.SetActive(val);
    }

    private void OnFearOverrideChanged(bool val)
    {
        foreach (var element in fearHackUIElements)
            element.SetActive(val);
    }

    private void OnVisionHackChanged(bool val)
    {
        foreach (var element in visionHackUIElements)
            element.SetActive(val);
    }

    private void OnWallPhaseChanged(bool val)
    {
        
    }

    private void OnDebugModeChanged(bool val)
    {
        
    }

    private void OnNoClipChanged(bool val)
    {
        
    }
}
