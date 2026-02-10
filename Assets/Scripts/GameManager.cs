using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public Ghost[] ghosts;
    public Pacman[] pacmen;
    public GameObject PlayerPrefab;
    public Pacman pacman;
    public Transform pellets;
    public UIController uiManager;

    [Header("Win Text Visuals")]
    public SpriteRenderer overlayFade;
    public float flashInterval = 0.5f;
    public GameObject winTextGameObject;
    public TextMeshProUGUI winnerText;
    public TextMeshProUGUI returnText;

    [Header("Speed Text Visuals")]
    public GameObject speedTextGameObject;
    public TextMeshProUGUI selectedSpeedText;

    Color ghostColor;
    Color pacColor;

    public int ghostMultiplier { get; private set; } = 1;
    public int score { get; private set; }

    public static int highScore { get; private set; } = 0;

    private float spawnInterval = 5f;
    private int nextIndex = 1;
    private Coroutine spawnCoroutine;
    private int humanPacmen = 0;
    private int humanGhosts = 0;
    private int aiPacmenCount = 0;
    private int aiGhostCount = 0;
    
    public AudioSource sirenAudio;
    private int frightenedGhostCount = 0;
    
    public AudioSource pacmanDeathAudio;
    public float pacmanDeathFreezeTime = 2f;

    public AudioSource ghostEatAudio;
    public float ghostEatFreezeTime = 1f;
    
    public IntroController introController;
    
    public AudioSource globalChompAudio;
    private Coroutine globalChompRoutine;
    
    // Booleans
    private bool gameStarted = false;
    private bool isDeathSequenceActive = false;

    public int gameOverTimer = 2;
    bool canRestart = false;

    Coroutine returnTextRoutine;


    private void Start()
    {
        this.spawnInterval = 5f / SpeedController.gameSpeed;
        
        Time.timeScale = SpeedController.gameSpeed;
        if (uiManager != null) {
            uiManager.UpdateHighScoreDisplay(highScore);
        }
        
        pacColor = this.pacmen[0].bodyRenderer.color;
        ghostColor = this.ghosts[0].bodyRenderer.color;
        humanPacmen = 0;
        humanGhosts = 0;
        if (PlayerManager.addedPlayers != null) //Goes through each player and there selected team to add to the game
        {
            for (int i = 0; i < PlayerManager.addedPlayers.Length; i++)
            {
                if (PlayerManager.addedPlayers[i] != null)
                {
                    if (PlayerManager.addedPlayers[i].hasSelected)
                    {
                        PlayerManager.addedPlayers[i].GameManager = this;
                        SetPlayer(i);
                    }
                }
            }
        }

        switch (SpeedController.gameSpeed) //Updates the text on screen to show the currently selected speed
        {
            case 1f:
                this.selectedSpeedText.text = "NORMAL";
                break;
            case 1.5f:
                this.selectedSpeedText.text = "FAST";
                break;
            case 2f:
                this.selectedSpeedText.text = "TURBO";
                break;
        }

        //Spawns or sets entities (Ghost/Pac-Man) based off of the selected amount of extra entities. It also sets up routines for spawning Pac-Men and Ghosts if necessary
        aiPacmenCount = SliderController.PacmanAIAmount;
        aiGhostCount = SliderController.GhostAIAmount;

        for (int i = humanPacmen; i < humanPacmen + aiPacmenCount && i < pacmen.Length; i++)
            pacmen[i].gameObject.SetActive(false);

        if (aiPacmenCount > 0)
            StartSpawningPacmen();

        for (int i = 4; i < humanGhosts + aiGhostCount && i < ghosts.Length; i++)
            ghosts[i].gameObject.SetActive(false);

        // start spawning AI ghosts
        if (aiGhostCount > 0)
            StartCoroutine(SpawnGhostsRoutine());

        if (aiPacmenCount == 0 && humanPacmen == 0) aiPacmenCount++;
        if (aiGhostCount + humanGhosts < 4) aiGhostCount = 4 - humanGhosts;

        Debug.Log($"humanPacmen={humanPacmen}, aiPacmenCount={aiPacmenCount}");
        Debug.Log($"humanGhosts={humanGhosts}, aiGhostCount={aiGhostCount}");
        
        NewGame();
    }

    /// <summary>
    /// Changes a boolean that checks if the game has started or not.
    /// </summary>
    /// <param name="started"></param>
    /// <returns></returns>
    public void SetGameStarted(bool started)
    {
        gameStarted = started;
    }
    
    private void Update()
    {
        CheckGlobalChomp();
        
        int active = 0;
        for (int i = 0; i < this.pacmen.Length; i++)
        {
            if (this.pacmen[i].gameObject.activeSelf)
            {
                active++;
            }
        }

        if (active <= 0)
        {
            if (gameOverTimer <= 0)
            {
                CancelInvoke(nameof(ToggleReturnToMenuText));
                InvokeRepeating(nameof(ToggleReturnToMenuText), 0f, flashInterval);
                if(returnTextRoutine != null)
                {
                    StopCoroutine(returnTextRoutine);
                }
                this.canRestart = true;
                gameOverTimer = 3;
            }

            if (this.canRestart)
            {
                foreach (var joystick in Joystick.all)
                {
                    foreach (var button in joystick.allControls)
                    {
                        if (button is ButtonControl btn && btn.wasPressedThisFrame)
                        {
                            //PlayerManager[] managers = FindObjectsOfType<PlayerManager>();
                            //foreach (PlayerManager manager in managers)
                            //{
                            //    Destroy(manager.gameObject);
                            //}
                            Time.timeScale = 1f;
                            SpeedController.gameSpeed = 1f;
                            PlayerManager.Session_Started = true;
                            SceneManager.LoadScene("MainMenu");
                            return;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Sets player to a specifc entity based off of which team the player is part of. playerNumber is based on which controller the player is using (P1, P2, etc.). arrayIndex is used to determine which index from either the pacmen or ghosts array should be used
    /// <param name="playerNumber"></param>
    /// <param name="arrayIndex"></param>
    /// <param name="type"></param>
    /// </summary>
    /// <returns></returns>
    //public void SetPlayer(int playerNumber, int arrayIndex, PlayerTeam type, int controller_id)
    //{
    //    GameObject player = Instantiate(PlayerPrefab);
    //    PlayerControl control = player.GetComponent<PlayerControl>();
    //    control.Initialize(playerNumber, controller_id, type, this);

    //    if (type == PlayerTeam.Pacman)
    //    {
    //        control.currentPacMan = this.pacmen[arrayIndex];
    //        this.pacmen[arrayIndex].controlledBy = control;
    //        Debug.Log(this.pacmen[arrayIndex].controlledBy);
    //    }
    //    else
    //    {
    //        control.currentGhost = this.ghosts[arrayIndex];
    //        this.ghosts[arrayIndex].controlledBy = control;
    //        this.ghosts[arrayIndex].initialBehavior.duration = 0;
    //        Debug.Log(this.ghosts[arrayIndex].controlledBy);
    //    }
    //}

    /// <summary>
    /// Sets the player to an instance of a character part of the team that player has chosen. It uses playerIndex to find the player's information in PlayerManager.AddedPlayers
    /// </summary>
    /// <param name="playerIndex"></param>
    /// <returns></returns>
    public void SetPlayer(int playerIndex)
    {
        if (PlayerManager.addedPlayers[playerIndex].team == PlayerTeam.Pacman)
        {
            PlayerManager.addedPlayers[playerIndex].currentGhost = null;
            PlayerManager.addedPlayers[playerIndex].currentPacMan = pacmen[humanPacmen];
            this.pacmen[humanPacmen].controlledBy = PlayerManager.addedPlayers[playerIndex];
            this.humanPacmen++;
        }
        else
        {
            PlayerManager.addedPlayers[playerIndex].currentPacMan = null;
            PlayerManager.addedPlayers[playerIndex].currentGhost = ghosts[playerIndex];
            this.ghosts[playerIndex].controlledBy = PlayerManager.addedPlayers[playerIndex];
            this.ghosts[playerIndex].home.duration = 0;
            this.ghosts[playerIndex].isOutside = true;
            if(humanGhosts == 0)
            {
                ghostColor = this.ghosts[playerIndex].bodyRenderer.color;
            }
            this.humanGhosts++;
        }
        PlayerManager.addedPlayers[playerIndex].enableSelectedNumber();
    }

    /// <summary>
    /// Starts routines to activate inactive Pac-Men from the pacmen array
    /// </summary>
    /// <returns></returns>
    public void StartSpawningPacmen()
    {
        if (aiPacmenCount <= 0)
            return;

        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);

        spawnCoroutine = StartCoroutine(SpawnPacmenRoutine());
    }

    /// <summary>
    /// Activates more Pac-Men in the maze at an interval based on the amount of Pac-Men in the maze.
    /// </summary>
    /// <returns></returns>
    private IEnumerator SpawnPacmenRoutine()
    {
        int maxIndex = Mathf.Min(humanPacmen + aiPacmenCount, pacmen.Length);

        int currentIndex = (humanPacmen == 0) ? 1 : humanPacmen;
        
        while (currentIndex < maxIndex)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (currentIndex < maxIndex && pacmen[currentIndex] != null)
            {
                pacmen[currentIndex].gameObject.SetActive(true);
                pacmen[currentIndex].ResetState();
                currentIndex++;
            }
        }

        spawnCoroutine = null;
    }

    /// <summary>
    /// Activates routine where the selected amount of ghosts spawn in
    /// </summary>
    /// <returns></returns>
    private IEnumerator SpawnGhostsRoutine()
    {
        int nextGhostIndex = 4;
        int maxIndex = Mathf.Min(humanGhosts + aiGhostCount, ghosts.Length);

        while (nextGhostIndex < maxIndex)
        {
            yield return new WaitForSeconds(1f);

            if (ghosts[nextGhostIndex] != null)
            {
                ghosts[nextGhostIndex].movement.enabled = true;
            }

            nextGhostIndex++;
        }
    }

    /// <summary>
    /// Resets all Pac-Men and stops the spawning routine. The only Pac-Men set to stay active are the amount of players wanting to control a Pac-Man.
    /// </summary>
    /// <returns></returns>
    public void ResetPacmen()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        for (int i = 0; i < this.pacmen.Length; i++)
        {
            if (this.pacmen[i] != null)
            {
                this.pacmen[i].gameObject.SetActive(false);
            }
        }

        for (int i = 0; i < humanPacmen; i++)
        {
            if (this.pacmen[i] != null)
            {
                this.pacmen[i].gameObject.SetActive(true);
                this.pacmen[i].ResetState();
            }
        }
        
        if (aiPacmenCount > 0)
        {
            if (humanPacmen == 0)
            {
                pacmen[0].gameObject.SetActive(true);
                pacmen[0].ResetState();
                nextIndex = 1;
            }
            else
            {
                nextIndex = humanPacmen; 
            }
            
            StartSpawningPacmen();
        }
    }

    /// <summary>
    /// Switches which ghost is being controlled by going from the last controlled ghost to the next one in the ghosts array. It loops around the whole array until it's back at the ghost the player was previously controlling
    /// <param name="player"></param>
    /// </summary>
    /// <returns></returns>
    public void SwitchToNextGhostControl(PlayerControl player)
    {
        int currentIndex = -1;

        // Step 1: Find the currently controlled ghost
        if (player.currentGhost != null)
        {
            for (int i = 0; i < ghosts.Length; i++)
            {
                if (this.ghosts[i].id == player.currentGhost.id)
                {
                    currentIndex = i;
                    break;
                }
            }
        }

        if (currentIndex == -1)
        {
            for (int i = 0; i < this.ghosts.Length; i++)
            {
                if (this.ghosts[i].controlledBy == null)
                {
                    player.currentGhost = this.ghosts[i];
                    this.ghosts[i].controlledBy = player;
                }
            }
            player.currentGhost = null; //No more playable ghosts
        }

        int index = currentIndex;

        while (true & player.currentGhost != null)
        {
            index = (index + 1) % ghosts.Length;
            if (index == currentIndex)
                break;
            if (this.ghosts[index].isOutside && this.ghosts[index].controlledBy == null)
            {
                this.ghosts[currentIndex].controlledBy = null;
                if (this.ghosts[currentIndex].lastNode != null)
                {
                    if (this.ghosts[currentIndex].frightened.enabled)
                    {
                        this.ghosts[currentIndex].frightened.GhostAI(this.ghosts[currentIndex].lastNode);
                    }
                    else if (ghosts[currentIndex].scatter.enabled)
                    {
                        this.ghosts[currentIndex].scatter.GhostAI(this.ghosts[currentIndex].lastNode);
                    }
                    else
                    {
                        this.ghosts[currentIndex].chase.GhostAI(this.ghosts[currentIndex].lastNode);
                    }
                }
                this.ghosts[index].controlledBy = player;
                Debug.Log(index);
                player.currentGhost = this.ghosts[index];
                PlayerManager.Instance.PlayMenuSound();
                break;
            }
        }
    }

    /// <summary>
    /// (Deprecated: Was used when more than 2 buttons were available on controllers) Switches which ghost is being controlled by going from the last controlled ghost to the previous one in the ghosts array. It loops around the whole array until it's back at the ghost the player was previously controlling
    /// </summary>
    /// <returns></returns>
    private void SwitchToPreviousGhostControl()
    {
        int currentIndex = -1;

        for (int i = 0; i < ghosts.Length; i++)
        {
            if (this.ghosts[i].controlledBy != null)
            {
                currentIndex = i;
                break;
            }
        }

        if (currentIndex == -1)
        {
            return; // no ghost is currently controlled
        }

        int index = currentIndex;

        while (true)
        {
            index = (index - 1 + ghosts.Length) % ghosts.Length;
            if (index == currentIndex)
            {
                break;
            }
            if (this.ghosts[index].isOutside)
            {
                if (this.ghosts[currentIndex].lastNode != null)
                {
                    if (this.ghosts[currentIndex].frightened.enabled)
                    {
                        this.ghosts[currentIndex].frightened.GhostAI(this.ghosts[currentIndex].lastNode);
                    }
                    else if (this.ghosts[currentIndex].scatter.enabled)
                    {
                        this.ghosts[currentIndex].scatter.GhostAI(this.ghosts[currentIndex].lastNode);
                    }
                    else
                    {
                        this.ghosts[currentIndex].chase.GhostAI(this.ghosts[currentIndex].lastNode);
                    }
                }
                Debug.Log(index);
                break;
            }
        }
    }

    /// <summary>
    /// Switches which Pac-Man is being controlled by going from the last controlled Pac-Man to the next one in the pacmen array. It loops around the whole array until it's back at the Pac-Man the player was previously controlling
    /// <param name="player"></param>
    /// </summary>
    /// <returns></returns>
    public void SwitchToNextPacmanControl(PlayerControl player)
    {
        int currentIndex = -1; //Temporarily set current index to -1. This only changes if a pac man to switch to is found

        // Step 1: Find the currently controlled pacman
        if (player.currentPacMan != null)
        {
            for (int i = 0; i < this.pacmen.Length; i++)
            {
                if (this.pacmen[i].id == player.currentPacMan.id) //If the index of the pac-man currently being assigned to player is found, set currentindex to the index of the pac-man in the pacmen array
                {
                    currentIndex = i;
                    break;
                }
            }
        }

        if (currentIndex == -1) //If no Pac-Man could be found in the previous for-loop, it means the player is currently not controlling a pac-man. Loop through the array if an available pac-man is present
        {
            for (int i = 0; i < this.pacmen.Length; i++)
            {
                if (this.pacmen[i].controlledBy == null)
                {
                    this.pacmen[i].controlledBy = player;
                    player.currentPacMan = this.pacmen[i];
                }
            }
            player.currentPacMan = null;
        }

        int index = currentIndex;

        while (true & player.currentPacMan != null)
        {
            index = (index + 1) % this.pacmen.Length;
            if (index == currentIndex)
                break;
            if (this.pacmen[index].gameObject.activeSelf && this.pacmen[index].controlledBy == null)
            {
                this.pacmen[currentIndex].controlledBy = null;

                this.pacmen[index].controlledBy = player;
                player.currentPacMan = this.pacmen[index];
                PlayerManager.Instance.PlayMenuSound();
                break;
            }
        }
    }

    /// <summary>
    /// (Deprecated: Was used when more than 2 buttons were available on controllers) Switches which Pac-Man is being controlled by going from the last controlled Pac-Man to the previous one in the pacmen array. It loops around the whole array until it's back at the pac-man the player was previously controlling
    /// </summary>
    /// <returns></returns>
    private void SwitchToPreviousPacmanControl()
    {
        int currentIndex = -1;

        for (int i = 0; i < this.pacmen.Length; i++)
        {
            if (this.pacmen[i].controlledBy != null)
            {
                currentIndex = i;
                break;
            }
        }

        if (currentIndex == -1)
        {
            return;
        }

        int index = currentIndex;

        while (true)
        {
            index = (index - 1 + this.pacmen.Length) % this.pacmen.Length;
            if (index == currentIndex)
            {
                break;
            }
            if (this.pacmen[index].gameObject.activeSelf)
            {
                break;
            }
        }
    }

    /// <summary>
    /// Starts up a fresh new game, by resetting almost everything accordingly to the choices made by the players in the previous menus.
    /// </summary>
    /// <returns></returns>
    private void NewGame()
    {
        SetScore(0);
        NewRound();
    }

    /// <summary>
    /// Sets up the state and positions of all entities and collectibles in the maze
    /// </summary>
    /// <returns></returns>
    private void NewRound()
    {
        foreach (Transform pellet in this.pellets)
        {
            pellet.gameObject.SetActive(true);
        }
        ResetState();
        ResetPacmen();
        //ResetPlayers();

    }

    /// <summary>
    /// Resets the ghosts, Pac-Men and collectibles in the maze
    /// </summary>
    /// <returns></returns>
    private void ResetState()
    {
        ResetGhostMultiplier();
        StopAllCoroutines();
        
        
        int totalAllowedGhosts = humanGhosts + aiGhostCount;


        for (int i = 0; i < this.ghosts.Length; i++)
        {
            if (i < totalAllowedGhosts)
            {
                this.ghosts[i].gameObject.SetActive(true);
                this.ghosts[i].ResetState();
                
                if (i >= humanGhosts)
                {
                    this.ghosts[i].movement.enabled = true;
                }
            }
            else
            {
                // Explicitly disable any extra ghosts in the array
                this.ghosts[i].gameObject.SetActive(false);
            }
        }

        
        if (aiGhostCount > 0) {
            StartCoroutine(SpawnGhostsRoutine());
        }

        if (this.pacmen.Length > 0) {
            ResetPacmen();
        } else {
            this.pacman.ResetState();
        }

        if (introController != null)
        {
            introController.TriggerIntro();
        }

        for (int i = 0; i < humanGhosts; i++)
        {
            this.ghosts[i].isOutside = true;
        }
        this.ghosts[0].isOutside = true;
    }

    /// <summary>
    /// Removes all the ghosts and Pac-Men from the maze
    /// </summary>
    /// <returns></returns>
    private void GameOver()
    {
        // 1. Safety check for PlayerManager and the static array
        if (PlayerManager.addedPlayers != null)
        {
            for (int i = 0; i < PlayerManager.addedPlayers.Length; i++)
            {
                var player = PlayerManager.addedPlayers[i];
                if (player != null)
                {
                    player.currentGhost = null;
                    player.currentPacMan = null;
                
                    // Only disable indicator if it's actually assigned in Inspector
                    if (player.indicator != null)
                    {
                        player.indicator.SetActive(false);
                    }
                }
            }
        }

        // 2. Disable ghosts safely
        if (ghosts != null)
        {
            for (int i = 0; i < this.ghosts.Length; i++)
            {
                if (this.ghosts[i] != null)
                    this.ghosts[i].gameObject.SetActive(false);
            }
        }

        // 3. Disable Pac-Men safely
        if (this.pacmen != null && this.pacmen.Length > 0)
        {
            for (int i = 0; i < this.pacmen.Length; i++)
            {
                if (this.pacmen[i] != null)
                    this.pacmen[i].gameObject.SetActive(false);
            }
        }
        else if (this.pacman != null) // Fallback for single pacman variable
        {
            this.pacman.gameObject.SetActive(false);
        }

        // 4. UI Visuals safety checks
        if (winTextGameObject != null) winTextGameObject.SetActive(true);
        if (overlayFade != null) overlayFade.gameObject.SetActive(true);
        if (speedTextGameObject != null) speedTextGameObject.SetActive(false);
        if (sirenAudio != null) sirenAudio.Stop();
        StopGlobalChomp();
    
        frightenedGhostCount = 0;
        returnText.enabled = false;
        returnTextRoutine = StartCoroutine(showReturnTextCountdown());
    }

    IEnumerator showReturnTextCountdown()
    {
        while (gameOverTimer >= 0)
        {
            Debug.Log(gameOverTimer);
            yield return new WaitForSeconds(1f);
            gameOverTimer--;
        }
    }

    /// <summary>
    /// (Needs updating: Save score of each individual player, along with setting up a ranking system for the winning team to be shown during the game-over screen) Sets the score for the Pac-Man Players
    /// </summary>
    /// <returns></returns>
    private void SetScore(int score)
    {
        this.score = score;
        
        if (uiManager != null) {
            uiManager.UpdateScoreDisplay(this.score);
        }
        
        if (this.score > highScore)
        {
            highScore = this.score;
        
            if (uiManager != null) {
                uiManager.UpdateHighScoreDisplay(highScore);
            }
        }
    }

    /// <summary>
    /// Code that runs when a ghost gets eaten. The ghost goes back to the fenced off area waiting to regenerate. If the ghost was being controlled and there are other ghosts on the map, the player will switch to one of the other ghosts
    /// </summary>
    /// <param name="ghost"></param>
    /// <returns></returns>
    public void GhostEaten(Ghost ghost)
    {
        StartCoroutine(GhostEatRoutine(ghost));
    }

    private IEnumerator GhostEatRoutine(Ghost ghost)
    {
        if (ghostEatAudio != null)
            ghostEatAudio.Play();

        yield return new WaitForSecondsRealtime(ghostEatFreezeTime);
        
        // mark ghost as eaten
        ghost.isOutside = false;
        if (ghost.controlledBy != null)
        {
            SwitchToNextGhostControl(ghost.controlledBy);
        }

        int points = ghost.points * ghostMultiplier;
        SetScore(this.score + points);
        this.ghostMultiplier++;
    }

    /// <summary>
    /// Code that runs when a Pac-Man gets eaten. It checks if any other Pac-Man is present. If not, the game ends. Needs the eaten Pac-Man in the parameter.
    /// </summary>
    /// <param name="eatenPacman"></param>
    /// <returns></returns>
    public void PacmanEaten(Pacman eatenPacman)
    {
        if (eatenPacman != null && !eatenPacman.isDead)
        {
            StartCoroutine(PacmanDeathRoutine(eatenPacman));
        }
    }

    /// <summary>
    /// The animation and audio of the last Pac-Man being defeated
    /// </summary>
    /// /// <param name="eatenPacman"></param>
    /// <returns></returns>
    private IEnumerator PacmanDeathRoutine(Pacman eatenPacman)
    {
        if (eatenPacman == null) yield break;

        int remainingCount = pacmen.Count(p => p.gameObject.activeSelf && p != eatenPacman);

        isDeathSequenceActive = true;
    
        if (remainingCount <= 0)
        {
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
            }
            Time.timeScale = 1f;
            
            StopGlobalChomp();
            if (sirenAudio != null) sirenAudio.Stop();
        
            foreach (Ghost ghost in ghosts) {
                if (ghost != null) {
                    ghost.gameObject.SetActive(false);
                }
            }
            
            if (pacmanDeathAudio != null) {
                pacmanDeathAudio.Play();
            }
            yield return eatenPacman.PlayDeathAnimation();
        }
        else
        {
            if (ghostEatAudio != null) {
                ghostEatAudio.Play();
            }
        }

    
        eatenPacman.gameObject.SetActive(false);
    
        PlayerControl controller = eatenPacman.controlledBy;
        eatenPacman.controlledBy = null;

        if (controller != null) {
            SwitchToNextPacmanControl(controller);
        }

        if (remainingCount <= 0)
        { 
            StopAllCoroutines();
            if (winnerText != null) {
                winnerText.text = "TEAM GHOST";
                winnerText.color = ghostColor;
            }
            GameOver();
            yield break;
        }
    
        isDeathSequenceActive = false; 
    
        foreach (Ghost ghost in ghosts) {
            if (ghost != null && ghost.gameObject.activeSelf) {
                ghost.FindNewTarget();
            }
        }
    }

    /// <summary>
    /// Code that runs when a pellet gets eaten. If a pellet gets eaten, the game ends. If all pellets are eaten, the game ends.
    /// </summary>
    /// <param name="pellet"></param>
    /// <returns></returns>
    public void PelletEaten(Pellet pellet)
    {
        pellet.gameObject.SetActive(false);
        
        SetScore(this.score + pellet.points);

        if (!HasRemainingPellets())
        {
            if (this.pacmen.Length > 0)
            {
                StopAllCoroutines();
                for (int i = 0; i < this.pacmen.Length; i++)
                {
                    this.pacmen[i].gameObject.SetActive(false);
                }
            }
            else
            {
                this.pacman.gameObject.SetActive(false);
            }
            winnerText.text = "TEAM PAC-MAN";
            winnerText.color = pacColor;
            
            StopCoroutine(SpawnPacmenRoutine());
            GameOver();
        }
    }

    /// <summary>
    /// Code that runs when a Power Pellet gets eaten. If a Power Pellet gets eaten, the Pac-Men get the ability to eat ghosts for a limited time.
    /// </summary>
    /// <param name="pellet"></param>
    /// <returns></returns>
    public void PowerPelletEaten(PowerPellet pellet)
    {
        
        
        for (int i = 0; i < this.ghosts.Length; i++)
        {
            this.ghosts[i].frightened.Enable(pellet.duration);
        }

        PelletEaten(pellet);
        CancelInvoke();
        Invoke(nameof(ResetGhostMultiplier), pellet.duration);
    }

    /// <summary>
    /// Returns if there are still pellets on the map for the player to eat.
    /// </summary>
    /// <returns></returns>
    private bool HasRemainingPellets()
    {
        foreach (Transform pellet in this.pellets)
        {
            if (pellet.gameObject.activeSelf)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Resets the ghostMultiplier value when the ability to eat ghosts given to the Pac-Men by eating a Power Pellet runs out
    /// </summary>
    /// <returns></returns>
    private void ResetGhostMultiplier()
    {
        this.ghostMultiplier = 1;
        frightenedGhostCount = 0;
        if (sirenAudio != null) sirenAudio.Stop();
    }
    
    // Call this from GhostFrightened when they turn blue
    public void SetGhostFrightened(bool isFrightened)
    {
        if (isFrightened) {
            frightenedGhostCount++;
        } else {
            frightenedGhostCount = Mathf.Max(0, frightenedGhostCount - 1);
        }

        // Play siren if at least one ghost is blue, otherwise stop
        if (frightenedGhostCount > 0) {
            if (!sirenAudio.isPlaying) sirenAudio.Play();
        } else {
            sirenAudio.Stop();
        }
    }

    /// <summary>
    /// (Deprecated: Was used when game would reset instead of returning to MainMenu scene) Resets player information and position.
    /// </summary>
    /// <returns></returns>
    private void ResetPlayers()
    {
        PlayerControl[] resetPlayers = FindObjectsOfType<PlayerControl>();

        foreach (PlayerControl selectedPlayer in resetPlayers)
        {
            Destroy(selectedPlayer.gameObject);
        }

        humanPacmen = 0;
        humanGhosts = 0;
        for (int i = 0; i < PlayerManager.addedPlayers.Length; i++)
        {
            if (PlayerManager.addedPlayers[i] != null)
            {
                PlayerManager.addedPlayers[i].GameManager = this;
                SetPlayer(i);
            }
        }

        aiPacmenCount = SliderController.PacmanAIAmount;
        aiGhostCount = SliderController.GhostAIAmount;

        Debug.Log($"humanPacmen={humanPacmen}, aiPacmenCount={aiPacmenCount}");
        Debug.Log($"humanGhosts={humanGhosts}, aiPacmenCount={aiGhostCount}");

        if (aiPacmenCount == 0 && humanPacmen == 0) aiPacmenCount++;
        if (aiGhostCount + humanGhosts < 4) aiGhostCount = 4 - (aiGhostCount - humanGhosts);

        for (int i = humanPacmen; i < humanPacmen + aiPacmenCount && i < pacmen.Length; i++)
            pacmen[i].gameObject.SetActive(false);
        if (aiPacmenCount > 0)
            StartSpawningPacmen();

        for (int i = humanGhosts; i < humanGhosts + aiGhostCount && i < ghosts.Length; i++)
            ghosts[i].gameObject.SetActive(false);

        // start spawning AI ghosts
        if (aiGhostCount > 0)
            StartCoroutine(SpawnGhostsRoutine());
    }

    /// <summary>
    /// Sets the text that says "Press Fire Button To Start New Game"'s visibility to the opposite state, used for flashing effect.
    /// </summary>
    /// <returns></returns>
    public void ToggleReturnToMenuText()
    {
        returnText.enabled = !returnText.enabled;
    }

    /// <summary>
    /// Code that checks if the audio for eating Pac-Dots is playing, used to make sure the audio doesn't overlap
    /// </summary>
    /// <returns></returns>
    private void CheckGlobalChomp()
    {
        if (globalChompAudio == null) return;

        bool anyPacmanAlive = pacmen.Any(p => p != null && p.gameObject.activeInHierarchy);
        
        if (gameStarted && anyPacmanAlive && !isDeathSequenceActive)
        {
            if (globalChompRoutine == null)
            {
                globalChompAudio.pitch = 1f; 
                globalChompRoutine = StartCoroutine(GlobalChompLoop());
            }
        }
        else if (globalChompRoutine != null)
        {
            StopGlobalChomp();
        }
    }

    /// <summary>
    /// Stops the eating Pac-Dots noises from the game.
    /// </summary>
    /// <returns></returns>
    private void StopGlobalChomp()
    {
        if (globalChompRoutine != null) {
            StopCoroutine(globalChompRoutine);
            globalChompRoutine = null;
        }
        if (globalChompAudio != null) globalChompAudio.Stop();
    }

    /// <summary>
    /// Routine that loops the eating Pac-Dots audio.
    /// </summary>
    /// <returns></returns>
    IEnumerator GlobalChompLoop()
    {
        while (true)
        {
            if (globalChompAudio != null && globalChompAudio.clip != null)
            {
                globalChompAudio.Play();
            
                // Use WaitForSecondsRealtime so Turbo mode doesn't speed up the rhythm
                // We wait for the exact length of the audio clip
                yield return new WaitForSecondsRealtime(globalChompAudio.clip.length); 
            }
            else
            {
                yield return null;
            }
        }
    }
}
