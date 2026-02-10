using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class PlayerManager : MonoBehaviour
{
    public static PlayerControl[] presentControllers;
    public static PlayerControl[] addedPlayers;
    public GameObject PlayerPrefab;

    public static int controllerNumberIndex = 0;
    public static int joinedCount = 0;

    public static bool is_menu = true; //Check for if the game is currently in a menu
    public static bool Session_Started = false; //Check for if -or when the character select menu has been entered at least once, so that the game can loop back to the character select menu instead of the main menu

    public static int first_active_player = 1; //The player_id of the first controller in addedPlayers that have actually pressed ready in the character select menu.

    public static string[] playerChoices = new string[4] { "Pac", "Pac", "Pac", "Pac" };
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public static PlayerManager Instance;
    
    public AudioSource menuAudioSource;
    public AudioClip menuSound;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
    }
    void Start()
    {
        presentControllers = new PlayerControl[4]; //The controllers present during gameplay
        addedPlayers = new PlayerControl[4]; //The controllers that are active and have a player number assigned
        controllerNumberIndex = 0;
        joinedCount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var joystick in Joystick.all)
        {
            // Skip if already assigned
            if (presentControllers != null && presentControllers.Any(c => c != null && c.playerInput.devices.Contains(joystick)))
                continue;

            if (joystick.allControls[0] is ButtonControl button) // A / Cross button
            {
                if (button.wasPressedThisFrame)
                {
                    GameObject player = Instantiate(PlayerPrefab);
                    PlayerControl control = player.GetComponent<PlayerControl>();
                    control.playerInput = PlayerInput.Instantiate(
                        player,
                        controlScheme: "Joystick",
                        pairWithDevice: joystick
                    );

                    // Add to array / list
                    int index = Array.IndexOf(presentControllers, null);
                    if (index != -1)
                        presentControllers[index] = control;
                }
            }
        }
    }

    /// <summary>
    /// Resets the arrays.
    /// </summary>
    /// <returns></returns>
    public static void ResetItems()
    {
        CharacterSelectController.totalPlayers = 0;
        CharacterSelectController.readyPlayers = 0;
        CharacterSelectController.timer = 30;
        CharacterSelectController.timer_is_active = false;
        SliderController.PacHumanAmount = 0;
        SliderController.PacmanAIAmount = 0;
        SliderController.GhostHumanAmount = 0;
        SliderController.GhostAIAmount = 0;

        //presentControllers = new PlayerControl[4];
        //addedPlayers = new PlayerControl[4];
        playerChoices = new string[4] { "Pac", "Pac", "Pac", "Pac" };
        //controllerNumberIndex = 0;
        joinedCount = 0;

        foreach(PlayerControl control in addedPlayers)
        {
            if(control != null)
            {
                control.hasSelected = false;
            }
        }
    }

    /// <summary>
    /// Plays menu sound, usually called if the player moves around in the menu.
    /// </summary>
    /// <returns></returns>
    public void PlayMenuSound()
    {
        if (menuAudioSource != null && menuSound != null)
        {
            menuAudioSource.PlayOneShot(menuSound);
        }
    }
}
