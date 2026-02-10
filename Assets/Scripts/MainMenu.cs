using System;
using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Transform[] options;
    public float pacmanOffset = 0.6f;
    
    public GameObject mainMenuPanel;       
    public GameObject characterSelectPanel;
    
    public SpeedController speedMenu;
    public Pacman pacman;

    public string option = "Play";

    public CharacterSelectController[] selectionPanels;
    bool is_speedMenu = false;

    //private void Awake() //Sets the game to take up the full screen manually in case the game does not do this on its own
    //{
    //    Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
    //    Screen.SetResolution(Screen.currentResolution.width,
    //                         Screen.currentResolution.height,
    //                         FullScreenMode.ExclusiveFullScreen);
    //}

    private void Start() //Reset everything
    {
        //PlayerInput[] instances = FindObjectsOfType<PlayerInput>();
        //foreach (PlayerInput instance in instances)
        //{
        //    Destroy(instance.gameObject);
        //}

        PlayerManager.ResetItems();
        Vector3 currentpos = this.pacman.transform.position;
        this.pacman.transform.position = new Vector3(-3.5f, 2.38f, currentpos.z);
        this.pacman.movement.enabled = false;
        
        //StartCoroutine(BlockInputOneFrame());
        if (PlayerManager.Session_Started)
        {
            PlayGame();
        }
    }

    private void Update()
    {
        if (speedMenu.speedMenuPanel.activeSelf && !is_speedMenu) //Checks if the speed menu is active, so that each player can have its speedmenu assigned
        {
            foreach (PlayerControl control in PlayerManager.addedPlayers)
            {
                if (control != null)
                {
                    control.speedMenu = this.speedMenu;
                }
            }
            is_speedMenu = true;
        }
        if (this.pacman != null) //Resets Pac-Man rotation
        {
            if (this.pacman.enabled)
            {
                if (this.pacman.movement != null)
                {
                    this.pacman.movement.enabled = false;
                }
                this.pacman.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            }
        }
    }

    /// <summary>
    /// Loads up the next menu.
    /// </summary>
    /// <returns></returns>
    public void PlayGame()
    {
        mainMenuPanel.SetActive(false);
        characterSelectPanel.SetActive(true);
        PlayerManager.Session_Started = true;
        foreach (PlayerControl control in PlayerManager.addedPlayers)
        {
            if (control != null)
            {
                if (control.playerId <= 0)
                {
                    addPlayerSelect(control);
                }
                else
                {
                    control.setChooseMenu(selectionPanels[control.playerId - 1]);
                } 
            }
        }
    }

    //private IEnumerator BlockInputOneFrame()
    //{
    //    CharacterSelectController controller = characterSelectPanel.GetComponent<CharacterSelectController>();
    //    if (controller != null)
    //        controller.blockInput = true;

    //    yield return null;

    //    if (controller != null)
    //        controller.blockInput = false;
    //}

    /// <summary>
    /// Quits the game by shutting down the application.
    /// </summary>
    /// <returns></returns>
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("quitting game");
    }

    /// <summary>
    /// Moves the Pac-Man icon to the next or previous menu item based off of the direction of the joystick on the controller, along with setting this.option to said menu item.
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public void menuDirection(string dir)
    {
        Vector3 currentpos = this.pacman.transform.position;
        Vector3 buttonpos = options[0].transform.position;
        switch (dir)
        {
            case "up":
                switch (option)
                {
                    case "Play":
                        option = "Quit";
                        buttonpos = options[2].transform.position;
                        break;
                    case "Settings":
                        option = "Play";
                        buttonpos = options[0].transform.position;
                        break;
                    case "Quit":
                        buttonpos = options[1].transform.position;
                        option = "Settings";
                        break;
                }
                break;
            case "down":
                switch (option)
                {
                    case "Play":
                        option = "Settings";
                        buttonpos = options[1].transform.position;
                        break;
                    case "Settings":
                        option = "Quit";
                        buttonpos = options[2].transform.position;
                        break;
                    case "Quit":
                        option = "Play";
                        buttonpos = options[0].transform.position;
                        break;
                }
                break;
        }
        this.pacman.transform.position = new Vector3(currentpos.x, buttonpos.y, currentpos.z);
    }

    /// <summary>
    /// Selects the currently chosen menu option.
    /// </summary>
    /// <returns></returns>
    public void EatOption()
    {
        if (option == "Play")
        {
            PlayGame();
        }
        else if (option == "Settings")
        {
            Debug.Log("settings selected (do your thing here)");
        }
        else if (option == "Quit")
        {
            QuitGame();
        }
        this.pacman.movement.enabled = true;
    }

    /// <summary>
    /// Adds player to the game (PlayerManager.addedPlayers) if the player joined in the main menu.
    /// </summary>
    /// <param name="control"></param>
    /// <returns></returns>
    public MainMenu addPlayerMainMenu(PlayerControl control)
    {        
        if (mainMenuPanel.activeSelf)
        {
            control.playerId = PlayerManager.joinedCount + 1;
            control.joined = true;
            PlayerManager.addedPlayers[PlayerManager.joinedCount] = control;
            PlayerManager.joinedCount++;
            return this;
        }
        return null;
    }

    /// <summary>
    /// Adds player to the game (PlayerManager.addedPlayers) along with the corresponding menu panel if the player joined in the character select menu.
    /// </summary>
    /// <param name="control"></param>
    /// <returns></returns>
    public CharacterSelectController addPlayerSelect(PlayerControl control)
    {
        if (characterSelectPanel.activeSelf)
        {
            control.setChooseMenu(selectionPanels[PlayerManager.joinedCount]);
            control.playerId = PlayerManager.joinedCount + 1;
            control.joined = true;
            PlayerManager.addedPlayers[PlayerManager.joinedCount] = control;
            PlayerManager.joinedCount++;
            return this.selectionPanels[control.playerId - 1];
        }
        return null;
    }

    /// <summary>
    /// Enables the main menu panels.
    /// </summary>
    /// <returns></returns>
    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        characterSelectPanel.SetActive(false);
    }
}
