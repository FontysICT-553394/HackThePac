using System;
using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public static GameSettings instance { get; private set; }

    public float pacmanSpeed = 3f;
    public float ghostSpeed = 2f;
    public string selectedCharacter;
    
    // Hacks - PacMan
    [SerializeField] private bool freezeEnabled = false;
    public event Action<bool> FreezeChanged;
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
    public event Action<bool> CloneChanged;
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
    public event Action<bool> SpeedOverflowChanged;
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
    public event Action<bool> FearOverrideChanged;
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
    public event Action<bool> VisionHackChanged;
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
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    
}
