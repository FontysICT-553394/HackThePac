using System;
using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public static GameSettings instance { get; private set; }

    public float pacmanSpeed = 3f;
    public float ghostSpeed = 3f;
    public string selectedCharacter;

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
