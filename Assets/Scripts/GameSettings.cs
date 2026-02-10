using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance { get; private set; }

    public float GhostSpeed = 1f;
    public float PacmanSpeed = 1f;
    public int GhostCount = 4;
    public int PowerPelletCount = 4;
    public bool PacmanNoClip = false;
    public string playerCharacter = "Pac";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
