using System;
using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public static GameSettings instance { get; private set; }

    public float pacmanSpeed = 3f;
    public float ghostSpeed = 2f;
    public string selectedCharacter;
    
    //hacks PacMan
    public bool freezeEnabled = false;
    public bool cloneEnabled = false;
    public bool speedOverflowEnabled = false;
    
    //Hacks Ghost
    public bool fearOverrideEnabled = false;
    public bool visionHackEnabled = false;
    public bool wallPhaseEnabled = false;
    
    //Hacks General
    public bool debugModeEnabled = false;
    public bool noClipEnabled = false;

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
