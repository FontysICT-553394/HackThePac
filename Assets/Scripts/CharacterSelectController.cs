using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;


public class CharacterSelectController : MonoBehaviour
{
    [Header("ui")]
    public Button pacButton;
    public Button ghostButton;
    public Button readyButton;

    public Image selector;
    public Vector3 selectorOffset = new Vector3(-2f, 0f, 0f);

    [Header("visuals")]
    public SpriteRenderer pacRenderer;
    public SpriteRenderer ghostRenderer;
    public SpriteRenderer overlayFade;
    public TextMeshProUGUI timerText;
    public GameObject timerVisuals;

    public TMP_Text pacText;
    public TMP_Text ghostText;
    public TMP_Text readyText;

    [Header("settings")]
    public float dimFactor = 0.5f;

    [Header("panels")]
    public GameObject characterPanel;
    public GameObject speedPanel;

    [HideInInspector] public bool blockInput = false;
    [HideInInspector] public PlayerTeam selectedCharacter = PlayerTeam.Pacman;
    
    public static int totalPlayers = 0;
    public static int readyPlayers = 0;

    public static int timer = 30;
    public static bool timer_is_active = false;

    public bool has_moved = false;

    public int playerID = 0;
    
    Button[] buttons;
    int currentIndex = 0;

    Color pacOriginal;
    Color ghostOriginal;
    Color pacTextOriginal;
    Color ghostTextOriginal;
    Color readyOriginal;
    Color overlayOriginal;

    private Coroutine routine;

    void Start()
    {
        buttons = new Button[] { pacButton, ghostButton, readyButton };
        readyButton.interactable = false;

        pacOriginal = pacRenderer.color;
        ghostOriginal = ghostRenderer.color;
        pacTextOriginal = pacText.color;
        ghostTextOriginal = ghostText.color;
        readyOriginal = readyText.color;
        overlayOriginal = overlayFade.color;
        
        UpdateSelector();
        UpdateSelectionVisuals();
        this.readyText.color = readyOriginal * dimFactor;

        
    }

    void Update()
    {
        if (timer_is_active)
        {
            if (readyPlayers == 0)
            {
                timer_is_active = false;
                if (routine != null)
                {
                    StopCoroutine(routine);
                }
                timer = 30;
                timerVisuals.SetActive(false);
            }
            else
            {
                timerText.text = timer.ToString() + "s";
                if (timer <= 0)
                {
                    StartGame();
                }
            }  
        }
        else
        {
            if (routine != null)
            {
                StopCoroutine(routine);
            }
            timer = 30;
        }
        if (blockInput) return;
        if (characterPanel.activeSelf)
        {
            if (!this.has_moved)
            {
                overlayFade.color = overlayOriginal * 0.8f;
            }
            else
            {
                overlayFade.color = overlayOriginal * 0.0f;
            }
        }
        else
        {
            return;
        }
    }

    /// <summary>
    /// Routine that counts down from 30 to 0. Player must choose a character during the remaining time.
    /// </summary>
    /// <returns></returns>
    IEnumerator CountdownRoutine()
    {
        while (timer > 0)
        {
            yield return new WaitForSeconds(1f);
            timer--;
        }

        Debug.Log("Done!");
    }

    /// <summary>
    /// Sets up the chosen direction based off of if the player pressed up/left or down/right. It then calls MoveSelector() to handle the menu selection
    /// </summary>
    /// <param name="is_up_or_left"></param>
    /// <returns></returns>
    public void menuDirection(bool is_up_or_left)
    {
        if (!has_moved)
        {
            totalPlayers++;
            this.has_moved = true;
        }
        else
        {
            int dir = 1;
            if (is_up_or_left) dir = -1;
            MoveSelector(dir);
        }  
    }

    /// <summary>
    /// (Needs update: remove function and just call HandleSelection directly) calls the handleSelection() function.
    /// </summary>
    /// <returns></returns>
    public void menuSelect()
    {
        if (!has_moved)
        {
            totalPlayers++;
            this.has_moved = true;
        }
        else
        {
            HandleSelection();
        }
            
    }

    /// <summary>
    /// Moves the selected menu item based off of the chosen direction.
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    void MoveSelector(int direction)
    {
        if(PlayerManager.addedPlayers[playerID - 1].hasSelected) return;

        int newIndex = currentIndex;

        do
        {
            newIndex += direction;

            if (newIndex >= buttons.Length) newIndex = 0;
            if (newIndex < 0) newIndex = buttons.Length - 1;

        } while (!buttons[newIndex].interactable);

        currentIndex = newIndex;
        UpdateSelector();
    }

    /// <summary>
    /// Visually updates the position of the selector arrow.
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    void UpdateSelector()
    {
        if (selector == null) return;
        selector.transform.position = buttons[currentIndex].transform.position + selectorOffset;
    }

    /// <summary>
    /// Updates the menu based off of which option had been selected.
    /// </summary>
    /// <returns></returns>
    public void HandleSelection()
    {
        Button currentButton = buttons[currentIndex];
        if (currentButton == readyButton)
        {
            if (PlayerManager.addedPlayers[playerID - 1].hasSelected)
            {
                if (selectedCharacter == PlayerTeam.Pacman)
                {
                    SliderController.PacHumanAmount -= 1;
                }
                else
                {
                    SliderController.GhostHumanAmount -= 1;
                }
                ResetSelection();
            }
        }
        else
        {
            if (blockInput)
            {
                ResetSelection();
            }
            else
            {
                if (currentButton == pacButton)
                {
                    selectedCharacter = PlayerTeam.Pacman;
                    SliderController.PacHumanAmount += 1;
                    this.readyText.color = pacOriginal;
                    
                }
                else if (currentButton == ghostButton)
                {
                    selectedCharacter = PlayerTeam.Ghost;
                    SliderController.GhostHumanAmount += 1;
                    this.readyText.color = ghostOriginal;
                }
                UpdateSelectionVisuals();
                readyButton.interactable = true;
                PlayerManager.addedPlayers[playerID - 1].team = selectedCharacter;
                PlayerManager.addedPlayers[playerID - 1].hasSelected = true;
                readyPlayers++;
                blockInput = true;
                CheckAllReady();
            }
        }
    }

    /// <summary>
    /// Checks if all players have joined in or not. If so, the next menu starts up, if not and the countdown timer has not activated yet, it will activate
    /// </summary>
    /// <returns></returns>
    void CheckAllReady()
    {
        if (readyPlayers >= totalPlayers)
        {
            StartGame();
        }
        else
        {
            if (!timer_is_active)
            {
                timerVisuals.SetActive(true);
                timer_is_active = true;
                routine = StartCoroutine(CountdownRoutine());
            }
        }
    }

    /// <summary>
    /// Visually updates which option should be highlighted after selecting an option or moving through the menu manually with the joystick.
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    void UpdateSelectionVisuals()
    {
        pacRenderer.color = pacOriginal * dimFactor;
        ghostRenderer.color = ghostOriginal * dimFactor;
        pacText.color = pacTextOriginal * dimFactor;
        ghostText.color = ghostTextOriginal * dimFactor;

        if (selectedCharacter == PlayerTeam.Pacman)
        {
            pacRenderer.color = pacOriginal;
            pacText.color = pacTextOriginal;
        }
        if (selectedCharacter == PlayerTeam.Ghost)
        {
            ghostRenderer.color = ghostOriginal;
            ghostText.color = ghostTextOriginal;
        }
    }

    /// <summary>
    /// Sets the player information and moves onto the next menu.
    /// </summary>
    /// <returns></returns>
    void StartGame()
    {
        foreach (PlayerControl player in PlayerManager.addedPlayers)
        {
            if (player != null)
            {
                if (player.hasSelected)
                {
                    PlayerManager.first_active_player = player.playerId;
                    break;
                }
            }
        }
        if (speedPanel != null) speedPanel.SetActive(true);
        if (characterPanel != null) characterPanel.SetActive(false);
    }
    
    public void ResetSelection()
    {
        if (blockInput)
            readyPlayers--;

        PlayerManager.addedPlayers[playerID - 1].hasSelected = false;
        blockInput = false;
        readyButton.interactable = false;
        this.readyText.color = readyOriginal * dimFactor;

        UpdateSelector();
        UpdateSelectionVisuals();
    }
}