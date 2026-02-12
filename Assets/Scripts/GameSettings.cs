using System;
using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public static GameSettings instance { get; private set; }

    public float pacmanSpeed = 3f;
    public float ghostSpeed = 3f;
    public int ghostAmount = 4;
    public int powerPelletAmount = 4;
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
