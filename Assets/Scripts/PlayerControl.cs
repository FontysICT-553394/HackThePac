using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.SceneManagement;

public enum PlayerTeam { Pacman, Ghost}
public enum MenuState
{
    MainMenu,
    CharacterSelect,
    SpeedSelect,
    AmountSelect,
    InGame
}
public class PlayerControl : MonoBehaviour
{
    public GameManager GameManager;
    private Vector2 moveInput;

    public PlayerTeam team;

    public Pacman currentPacMan;
    public Ghost currentGhost;

    public PlayerInput playerInput = null;

    public SpriteRenderer[] playerNumbers;
    public GameObject indicator;
    public SpriteRenderer arrow;
    // public SpriteRenderer selectedNumber;

    private Vector3 offset = new Vector3(0, 1.5f, 1);

    public SpeedController speedMenu;
    public CharacterSelectController chooseMenu;
    public MainMenu mainMenu;
    public SliderMenu sliderMenu;

    public bool joined = false;
    public bool hasSelected = false;

    public int controllerId = 0;
    public int playerId = 0;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Turns on the indicator and sets which player number sprite the indicator should show.
    /// </summary>
    /// <returns></returns>
    public void enableSelectedNumber()
    {
        // this.selectedNumber = playerNumbers[playerId - 1];
        // this.selectedNumber.gameObject.SetActive(true);
    }

    private void Update()
    {
        if(this.GameManager != null) //Running a per-frame check to see where the joystick on the controller is pointing to
        {
            Vector2 keyboardInput = Vector2.zero;
            
            // Check for keyboard input (WASD and Arrow keys)
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                keyboardInput.y = 1f;
            else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                keyboardInput.y = -1f;
            
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                keyboardInput.x = 1f;
            else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                keyboardInput.x = -1f;
            
            // Use keyboard input if available, otherwise use joystick input
            Vector2 input = keyboardInput.sqrMagnitude > 0.01f ? keyboardInput : this.moveInput;
            
            if (input != Vector2.zero)
            {
                if (input.sqrMagnitude < 0.01f) return;
                // --- IN-GAME MOVEMENT (No menu sound here) ---
    
                Vector2 direction = Vector2.zero;
    
                if (input.y > 0.5) // Moving up
                {
                    direction = Vector2.up;
                }
                else if (input.y < -0.5) // Moving down
                {
                    direction = Vector2.down;
                }
                else if (input.x > 0.5) // Moving right
                {
                    direction = Vector2.right;
                }
                else if (input.x < -0.5) // Moving left
                {
                    direction = Vector2.left;
                }
    
                if (team == PlayerTeam.Pacman && currentPacMan != null)
                {
                    currentPacMan.movement.SetDirection(direction);
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    currentPacMan.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                }
                else if (team == PlayerTeam.Ghost && currentGhost != null)
                {
                    currentGhost.movement.SetDirection(direction);
                }
            }
        }
    }

    private void LateUpdate()
    {
        if(chooseMenu != null)
        {
            chooseMenu.playerID = this.playerId;
        }
        this.indicator.gameObject.SetActive(false);
        if (mainMenu == null && chooseMenu == null)
        {
            if (currentPacMan != null)
            {
                this.indicator.gameObject.SetActive(true);
                Vector2 newPosition = this.currentPacMan.transform.position + offset;
                this.indicator.transform.position = newPosition;
                // this.arrow.color = this.selectedNumber.color;
            }
            if (currentGhost != null)
            {
                this.indicator.gameObject.SetActive(true);
                Vector2 newPosition = this.currentGhost.transform.position + offset;
                this.indicator.transform.position = newPosition;
                // this.arrow.color = this.selectedNumber.color;
            }
        }
    }

    /// <summary>
    /// If the player moves a joystick in the menus, this code will call the function from the current menuscreen that's active. It also sets moveInput to track the joystick of the controller for use during gameplay
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public void OnMove(InputAction.CallbackContext context)
    {
        this.controllerId = context.control.device.deviceId;
        this.moveInput = context.ReadValue<Vector2>();

        if (context.canceled)
        {
            this.moveInput = Vector2.zero;
        }

        
        if (!context.performed) return;

        Vector2 direction = context.ReadValue<Vector2>().normalized;
        if (direction.sqrMagnitude < 0.01f) return;

        if (this.GameManager == null) 
        {
            if ((sliderMenu != null && sliderMenu.sliderPanel.activeSelf))
            {
                if(this.playerId == PlayerManager.first_active_player)
                {
                    PlayerManager.Instance.PlayMenuSound();
                    if (direction.x < -0.1f) sliderMenu.moveSlider(-1);
                    else if (direction.x > 0.1f) sliderMenu.moveSlider(1);
                }
            }
            else if (speedMenu != null && speedMenu.speedMenuPanel.activeSelf)
            {
                string dir = direction.y > 0.5f ? "up" : direction.y < -0.5f ? "down" : "";
                speedMenu.menuDirection(dir, playerId);
            }
            else if (chooseMenu != null && chooseMenu.characterPanel.activeSelf)
            {
                PlayerManager.Instance.PlayMenuSound();
                bool moveUp = direction.y > 0.5f || direction.x < -0.5f;
                chooseMenu.menuDirection(moveUp);
            }
            else
            {
                if (this.mainMenu == null) mainMenu = FindObjectOfType<MainMenu>();

                if (mainMenu != null && joined)
                {
                    if (this.mainMenu.characterSelectPanel.activeSelf)
                    {
                        if(this.chooseMenu == null) this.chooseMenu = this.mainMenu.selectionPanels[playerId - 1];
                    }
                    else
                    {
                        string dir = direction.y > 0.5f ? "up" : direction.y < -0.5f ? "down" : "";
                        if (!string.IsNullOrEmpty(dir)) mainMenu.menuDirection(dir);
                    }
                }
            }
        }
    }

    /// <summary>
    /// If the fire button on the joystick is being pressed, the appropriate function of the current menu or gameplay action will be called.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if(this.GameManager != null)
        {
            if (team == PlayerTeam.Pacman)
            {
                GameManager.SwitchToNextPacmanControl(this);
            }
            else
            {
                GameManager.SwitchToNextGhostControl(this);
            }
        }
        else if ((sliderMenu != null && sliderMenu.sliderPanel.activeSelf))
        {
            if(this.playerId == PlayerManager.first_active_player)
            {
                PlayerManager.Instance.PlayMenuSound();
                this.sliderMenu.interact();
            }
        }
        else if (speedMenu != null && speedMenu.speedMenuPanel.activeSelf)
        {
            this.speedMenu.menuSelect(playerId);
        }
        else if (this.chooseMenu != null && this.chooseMenu.characterPanel.activeSelf)
        {
            this.chooseMenu.menuSelect();
        }
        else
        {
            if (this.mainMenu == null)
            {
                this.mainMenu = FindObjectOfType<MainMenu>();
            }

            if (mainMenu != null)
            {
                if (mainMenu.characterSelectPanel.activeSelf)
                {
                    if (joined)
                    {
                        this.chooseMenu = this.mainMenu.selectionPanels[this.playerId - 1];
                        this.chooseMenu.menuSelect();
                    }
                    else
                    {
                        this.chooseMenu = this.mainMenu.addPlayerSelect(this);
                    } 
                }
                else
                {
                    if (joined)
                    {
                        this.mainMenu.EatOption();
                    }
                    else
                    {
                        this.mainMenu = this.mainMenu.addPlayerMainMenu(this);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Sets mainMenu and clears the other menus.
    /// </summary>
    /// <param name="mainMenu"></param>
    /// <returns></returns>
    public void setMainMenu(MainMenu mainMenu)
    {
        this.mainMenu = mainMenu;
        this.chooseMenu = null;
        this.speedMenu = null;
    }

    /// <summary>
    /// Sets chooseMenu and clears speedmenu.
    /// </summary>
    /// <param name="setController"></param>
    /// <returns></returns>
    public void setChooseMenu(CharacterSelectController setController)
    {
        this.chooseMenu = setController;
        this.speedMenu = null;
    }

    /// <summary>
    /// Sets speedMenu.
    /// </summary>
    /// <param name="setController"></param>
    /// <returns></returns>
    public void setSpeedMenu(SpeedController setController)
    {
        this.speedMenu = setController;
    }
}
