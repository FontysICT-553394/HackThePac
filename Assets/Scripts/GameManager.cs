using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("PacMan Settings")]
    [SerializeField] private GameObject pacmanPrefab;
    [SerializeField] private Transform pacmanSpawnPoint;

    [Header("Ghost Settings")]
    [SerializeField] private List<GameObject> ghostPrefabs;
    [SerializeField] private List<Transform> ghostSpawn;
    [SerializeField] private Transform inside;
    [SerializeField] private Transform outside;

    [Header("Game Settings")]
    [SerializeField] private GameObject pelletTilemap;
    [SerializeField] private GameObject powerPelletTilemap;

    [Header("Game UI")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text highScoreTextLose;
    [SerializeField] private TMP_Text highScoreTextWin;
    [SerializeField] private GameObject gameWinUI;
    [SerializeField] private GameObject gameLoseUI;

    private Ghost[] ghosts;
    private PacMan pacman;
    private Transform pellets;
    private Text gameOverText;
    private Text livesText;

    private float _score = 0f;
    private int lives = 3;
    private int ghostMultiplier = 1;
    private HashSet<string> _unlockedAchievements = new HashSet<string>();

    private GameObject pacmanInstance;
    private List<GameObject> ghostInstances = new List<GameObject>();
    private Tilemap pelletMap;
    private Tilemap powerPelletMap;
    private TilemapCollider2D powerPelletTilemapCollider2D;
    private TilemapCollider2D pelletTilemapCollider2D;

    // Colliders
    private BoxCollider2D pacmanCollider2D;
    
    private Coroutine freezeAchievementCoroutine = null;
    private Coroutine killerGhostAchievementCoroutine = null;
    private Coroutine speedrunAchievementCoroutine = null;
    private int speedrunCountdown = 90;
    private int killerGhostCountdown = 10;

    private void Awake()
    {
        if (Instance != null)
        {
            DestroyImmediate(gameObject);
        }
        else
        {
            Instance = this;
        }

        int ghostLayer = LayerMask.NameToLayer("Ghost");
        if (ghostLayer == -1)
            Debug.LogError("Layer `Ghost` not found. Please create a layer named `Ghost` in Project Settings > Tags and Layers.");
        else
            Physics2D.IgnoreLayerCollision(ghostLayer, ghostLayer, true);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void Start()
    {
        pelletTilemapCollider2D = pelletTilemap.GetComponent<TilemapCollider2D>();
        powerPelletTilemapCollider2D = powerPelletTilemap.GetComponent<TilemapCollider2D>();
        pelletMap = pelletTilemap.GetComponent<Tilemap>();
        powerPelletMap = powerPelletTilemap.GetComponent<Tilemap>();

        InstantiatePacman();
        InstantiateGhosts();

        AddPlayerScriptToPlayer();
        AddAiScriptToEnemies();

        NewGame();
        
        AchievementsUnlocked();
        StartTimedAchievements();
    }

    private void Update()
    {
        int pelletsLeft = CountTiles(pelletMap) + CountTiles(powerPelletMap);
        if (pelletsLeft <= 0)
            AllPelletsEaten();

        if (lives <= 0 && Input.anyKeyDown)
        {
            NewGame();
        }

        if (!Mathf.Approximately(pacmanInstance.transform.position.x, pacmanSpawnPoint.transform.position.x) || !Mathf.Approximately(pacmanInstance.transform.position.y, pacmanSpawnPoint.transform.position.y))
            StopCoroutine(freezeAchievementCoroutine);
    }

    private void NewGame()
    {
        SetScore(0);
        SetLives(3);
        NewRound();
    }

    private void NewRound()
    {
        if (gameOverText != null)
            gameOverText.enabled = false;

        if (pellets != null)
        {
            foreach (Transform pellet in pellets)
            {
                pellet.gameObject.SetActive(true);
            }
        }

        ResetState();
    }

    private void ResetState()
    {
        if (ghosts != null)
        {
            for (int i = 0; i < ghosts.Length; i++)
            {
                ghosts[i].ResetState();
            }
        }
    }

    private void GameOver()
    {
        if (gameOverText != null)
            gameOverText.enabled = true;

        if (ghosts != null)
        {
            for (int i = 0; i < ghosts.Length; i++)
            {
                ghosts[i].gameObject.SetActive(false);
            }
        }

        if (pacman != null)
            pacman.gameObject.SetActive(false);
    }

    private void SetLives(int lives)
    {
        this.lives = lives;
        if (livesText != null)
            livesText.text = "x" + lives.ToString();
    }

    private void SetScore(float score)
    {
        this._score = score;
        UpdateScoreUI();
    }

    public void AddScore(float amount)
    {
        _score += amount;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (GameSettings.instance.selectedCharacter == "pacman")
            scoreText.text = "Score: " + _score;
        else
            scoreText.text = "Score: " + (2620f - _score);
    }

    private void InstantiatePacman()
    {
        if (pacmanInstance != null)
            Destroy(pacmanInstance);

        pacmanInstance = Instantiate(pacmanPrefab, pacmanSpawnPoint.position, Quaternion.identity);
        pacmanCollider2D = pacmanInstance.GetComponent<BoxCollider2D>();
    }

    private void InstantiateGhosts()
    {
        foreach (var ghost in ghostInstances)
            Destroy(ghost);
        ghostInstances.Clear();

        for (int i = 0; i < ghostPrefabs.Count; i++)
        {
            if (i >= ghostSpawn.Count)
            {
                Debug.LogError($"Not enough spawn points for ghost index {i}");
                break;
            }

            var ghostInstance = Instantiate(ghostPrefabs[i], ghostSpawn[i].position, Quaternion.identity);
            ghostInstances.Add(ghostInstance);
            
            Ghost ghostScript = ghostInstance.GetComponent<Ghost>();
            ghostScript.target = pacmanInstance.transform;
            
            GhostHome ghostHome = ghostInstance.GetComponent<GhostHome>();
            ghostHome.inside = inside;
            ghostHome.outside = outside;
        }
    }

    private void AddAiScriptToEnemies()
    {
        if (GameSettings.instance.selectedCharacter != "pacman")
        {
            if (pacmanInstance.GetComponent<PacManAI>() == null)
                pacmanInstance.AddComponent<PacManAI>();
        }

        foreach (var ghost in ghostInstances)
        {
            if (ghost.name.Equals(GameSettings.instance.selectedCharacter + "(Clone)",
                    StringComparison.OrdinalIgnoreCase))
            {
                // Stop coroutines on GhostHome before destroying to avoid assertion errors
                GhostHome ghostHome = ghost.GetComponent<GhostHome>();
                if (ghostHome != null)
                {
                    ghostHome.StopAllCoroutines();
                    Destroy(ghostHome);
                }

                // Disable before destroying to prevent OnDisable from calling destroyed references
                GhostScatter ghostScatter = ghost.GetComponent<GhostScatter>();
                if (ghostScatter != null)
                {
                    ghostScatter.enabled = false;
                    Destroy(ghostScatter);
                }

                GhostChase ghostChase = ghost.GetComponent<GhostChase>();
                if (ghostChase != null)
                {
                    ghostChase.enabled = false;
                    Destroy(ghostChase);
                }
            }
        }
    }

    private void AddPlayerScriptToPlayer()
    {
        var playerCharacterName = GameSettings.instance.selectedCharacter;
        var character = GameObject.Find(playerCharacterName + "(Clone)");
        character.AddComponent<PlayerMovement>().wallLayer = LayerMask.GetMask("walls");
    }

    public void PacmanEaten()
    {
        SetLives(lives - 1);

        if (lives > 0)
        {
            Invoke(nameof(ResetState), 3f);
        }
        else
        {
            GameOver();
        }
    }

    public void GhostEaten(Ghost ghost)
    {
        int points = ghost.points * ghostMultiplier;
        AddScore(points);
        ghostMultiplier++;
    }

    /// <summary>
    /// Handles pellet consumption and score updates via Tilemap. If it's a power pellet, also triggers the power-up effect.
    /// </summary>
    public void PelletEaten(Tilemap tilemap, Vector3Int cellPos, bool isPowerPellet = false)
    {
        if (tilemap.HasTile(cellPos))
        {
            tilemap.SetTile(cellPos, null);
        }

        if (isPowerPellet)
        {
            PowerPelletEatenEffect();
        }
    }

    private void PowerPelletEatenEffect(float duration = 8f)
    {
        // Set PacMan powered up so PacMan.Update() skips ghost collision checks
        PacMan pacManScript = pacmanInstance.GetComponent<PacMan>();
        if (pacManScript != null)
            pacManScript.isPoweredUp = true;

        foreach (var ghostObj in ghostInstances)
        {
            Ghost ghost = ghostObj.GetComponent<Ghost>();
            if (ghost != null)
            {
                ghost.frightened.Enable(duration);
            }
        }

        CancelInvoke(nameof(ResetGhostMultiplier));
        Invoke(nameof(ResetGhostMultiplier), duration);

        CancelInvoke(nameof(ResetPacManPowerUp));
        Invoke(nameof(ResetPacManPowerUp), duration);
    }

    private void ResetPacManPowerUp()
    {
        PacMan pacManScript = pacmanInstance.GetComponent<PacMan>();
        if (pacManScript != null)
            pacManScript.isPoweredUp = false;
    }

    public void PacManDied()
    {
        if (GameSettings.instance.selectedCharacter == "pacman")
        {
            ShowLose();
            AchievementManager.Instance.SetProgress("pro_gamer", 0);
        }
        else
        {
            ShowWin();
            
            StopCoroutine(killerGhostAchievementCoroutine);
            if (killerGhostCountdown > 0)
                AchievementManager.Instance.SetProgress("killer_ghost", 1);
        }
    }

    private void AllPelletsEaten()
    {
        if (GameSettings.instance.selectedCharacter == "pacman")
        {
            ShowWin();
            AchievementManager.Instance.AddProgress("pro_gamer");
            
            StopCoroutine(speedrunAchievementCoroutine);
            if (speedrunCountdown > 0)
                AchievementManager.Instance.SetProgress("speedrun", 1);
        }
        else
            ShowLose();
    }

    private void ShowWin()
    {
        gameWinUI.SetActive(true);
        gameLoseUI.SetActive(false);
        scoreText.enabled = false;

        if (GameSettings.instance.selectedCharacter == "pacman")
            highScoreTextWin.text = "Score: " + _score;
        else
            highScoreTextWin.text = "Score: " + (2620f - _score);

        Time.timeScale = 0;
    }

    private void ShowLose()
    {
        gameWinUI.SetActive(false);
        gameLoseUI.SetActive(true);
        scoreText.enabled = false;

        if (GameSettings.instance.selectedCharacter == "pacman")
            highScoreTextLose.text = "Score: " + _score;
        else
            highScoreTextLose.text = "Score: " + (2620f - _score);

        Time.timeScale = 0;
    }

    private bool HasRemainingPellets()
    {
        return CountTiles(pelletMap) + CountTiles(powerPelletMap) > 0;
    }

    private int CountTiles(Tilemap tilemap)
    {
        if (tilemap == null) return 0;
        var bounds = tilemap.cellBounds;
        int count = 0;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                var pos = new Vector3Int(x, y, 0);
                if (tilemap.HasTile(pos))
                    count++;
            }
        }
        return count;
    }

    private void ResetGhostMultiplier()
    {
        ghostMultiplier = 1;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = 1f;
    }
    
    private void StartTimedAchievements()
    {
        int freezeAchievementDuration = AchievementManager.Instance.GetAchievementById("freeze").targetProgress;
        int killerGhostAchievementDuration = AchievementManager.Instance.GetAchievementById("killer_ghost").targetProgress;
        int speedrunAchievementDuration = AchievementManager.Instance.GetAchievementById("speedrun").targetProgress;
        
        if (!_unlockedAchievements.Contains("freeze"))
        {
            AchievementManager.Instance.SetProgress("freeze", 0);
            freezeAchievementCoroutine = StartCoroutine(StartTimer(freezeAchievementDuration, () =>
            {
                AchievementManager.Instance.AddProgress("freeze");
            }));
        }
        
        if (!_unlockedAchievements.Contains("killer_ghost"))
        {
            AchievementManager.Instance.SetProgress("killer_ghost", 0);
            killerGhostAchievementCoroutine = StartCoroutine(StartTimer(killerGhostAchievementDuration, () =>
            {
                killerGhostCountdown--;
            }));
        }
        
        if (!_unlockedAchievements.Contains("speedrun"))
        {
            AchievementManager.Instance.SetProgress("speedrun", 0);
            speedrunAchievementCoroutine = StartCoroutine(StartTimer(speedrunAchievementDuration, () =>
            {
                speedrunCountdown--;
            }));
        }
    }

    private void AchievementsUnlocked()
    {
        AchievementManager achievementManager = AchievementManager.Instance;
        if (achievementManager == null)
        {
            Debug.LogError("AchievementManager instance not found!");
            return;
        }

        foreach (var (definition, entry) in achievementManager.GetAllStatus())
        {
            if (entry.isCompleted && !_unlockedAchievements.Contains(definition.id))
                _unlockedAchievements.Add(definition.id);
        }
        
    }
    
    private IEnumerator StartTimer(int duration, Action onComplete)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            yield return new WaitForSeconds(1f);
            onComplete?.Invoke();
            elapsed += 1f;
        }
    }
    
}
