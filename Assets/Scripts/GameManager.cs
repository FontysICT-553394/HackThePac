using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.TextCore.Text;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("PacMan Settings")]
    [SerializeField] private GameObject pacmanPrefab;
    [SerializeField] private Transform pacmanSpawnPoint;

    [Header("Ghost Settings")]
    [SerializeField] private List<GameObject> ghostPrefabs;
    [SerializeField] private List<Transform> ghostSpawn;

    [Header("Game Settings")]
    [SerializeField] private GameObject pelletTilemap;
    [SerializeField] private GameObject powerPelletTilemap;
    [SerializeField] private NodeGraphBuilder _nodeGraphBuilder;
    [SerializeField] private Transform _houseExitTarget;

    [Header("Game UI")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text highScoreTextLose;
    [SerializeField] private TMP_Text highScoreTextWin;
    [SerializeField] private GameObject gameWinUI;
    [SerializeField] private GameObject gameLoseUI;


    private float _score = 0f;

    private GameObject _pacmanInstance;
    private List<GameObject> _ghostInstances = new List<GameObject>();
    private Tilemap _pelletMap;
    private Tilemap _powerPelletMap;
    private TilemapCollider2D _powerPelletTilemapCollider2D;
    private TilemapCollider2D _pelletTilemapCollider2D;

    private int ghostMultiplier = 1;

    // Colliders
    private BoxCollider2D _pacmanCollider2D;

    public void Start()
    {
        _pelletTilemapCollider2D = pelletTilemap.GetComponent<TilemapCollider2D>();
        _powerPelletTilemapCollider2D = powerPelletTilemap.GetComponent<TilemapCollider2D>();
        _pelletMap = pelletTilemap.GetComponent<Tilemap>();
        _powerPelletMap = powerPelletTilemap.GetComponent<Tilemap>();

        InstantiatePacman();
        InstantiateGhosts();

        AddPlayerScriptToPlayer();
        AddAiScriptToEnemies();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        int pelletsLeft = CountTiles(_pelletMap) + CountTiles(_powerPelletMap);
        if (pelletsLeft <= 0)
            AllPelletsEaten();
    }

    private void InstantiatePacman()
    {
        if (_pacmanInstance != null)
            Destroy(_pacmanInstance);

        _pacmanInstance = Instantiate(pacmanPrefab, pacmanSpawnPoint.position, Quaternion.identity);
        _pacmanCollider2D = _pacmanInstance.GetComponent<BoxCollider2D>();
    }

    private void InstantiateGhosts()
    {
        foreach (var ghost in _ghostInstances)
            Destroy(ghost);
        _ghostInstances.Clear();

        for (int i = 0; i < ghostPrefabs.Count; i++)
        {
            if (i >= ghostSpawn.Count)
            {
                Debug.LogError($"Not enough spawn points for ghost index {i}");
                break;
            }

            var ghostInstance = Instantiate(ghostPrefabs[i], ghostSpawn[i].position, Quaternion.identity);
            _ghostInstances.Add(ghostInstance);
        }
    }

    private void AddAiScriptToEnemies()
    {
        if (GameSettings.instance.selectedCharacter != "pacman")
        {
            if (_pacmanInstance.GetComponent<PacManAI>() == null)
                _pacmanInstance.AddComponent<PacManAI>();
        }

        if (_nodeGraphBuilder == null)
            Debug.LogError("GameManager: _nodeGraphBuilder is not assigned in the Inspector!");
        if (_houseExitTarget == null)
            Debug.LogError("GameManager: _houseExitTarget is not assigned in the Inspector!");

        foreach (var ghost in _ghostInstances)
        {
            if (ghost.name.Equals(GameSettings.instance.selectedCharacter + "(Clone)",
                    StringComparison.OrdinalIgnoreCase))
                continue;

            //TODO: Add ghost movement on them later (dynamicaly)
        }
    }



    private void AddPlayerScriptToPlayer()
    {
        var playerCharacterName = GameSettings.instance.selectedCharacter;
        var character = GameObject.Find(playerCharacterName + "(Clone)");
        character.AddComponent<PlayerMovement>().wallLayer = LayerMask.GetMask("walls");
    }

    /// <summary>
    /// Handles pellet consumption and score updates. If it's a power pellet, also triggers the power-up effect.
    /// </summary>
    /// <param name="tilemap">The tilemap to remove the pellet from</param>
    /// <param name="cellPos">The pellet position in the tilemap</param>
    /// <param name="isPowerPellet">If it's a power pellet or not</param>
    public void PelletEaten(Tilemap tilemap, Vector3Int cellPos, bool isPowerPellet = false)
    {
        if (tilemap.HasTile(cellPos))
        {
            tilemap.SetTile(cellPos, null);
        }

        //if (AchievementManager.Instance == null)
        //{
        //    Debug.LogError("AchievementManager.Instance is null!");
        //    return;
        //}

        //AchievementManager.Instance.AddProgress("pellets_eaten", 1);

        //if (isPowerPellet)
        //{
        //    //TODO: Add power-up effect
        //}
    }


    public void PacManDied()
    {
        if (GameSettings.instance.selectedCharacter == "pacman")
            ShowLose();
        else
            ShowWin();
    }

    private void AllPelletsEaten()
    {
        if (GameSettings.instance.selectedCharacter == "pacman")
            ShowWin();
        else
            ShowLose();
    }

    private void ShowWin()
    {
        gameWinUI.SetActive(true);
        gameLoseUI.SetActive(false);
        scoreText.enabled = false;

        if (GameSettings.instance.selectedCharacter == "pacman")
            highScoreTextLose.text = "Score: " + _score;
        else
        {
            // Debug the exact string before assigning
            var s = GameSettings.instance.selectedCharacter == "pacman"
                ? "Score: " + _score
                : "Score: " + (2620f - _score);
            highScoreTextWin.text = s;
        }


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
        {
            // Debug the exact string before assigning
            var s = GameSettings.instance.selectedCharacter == "pacman"
                ? "Score: " + _score
                : "Score: " + (2620f - _score);
            highScoreTextWin.text = s;
        }

        Time.timeScale = 0;
    }

    /// <summary>
    /// Add score to the player's total score and updates the UI accordingly.
    /// </summary>
    /// <param name="amount">How many points you want to add</param>
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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = 1f;
    }


    public void GhostEaten(Ghost ghost)
    {
        //int points = ghost.points * ghostMultiplier;
        //SetScore(score + points);

        //ghostMultiplier++;
    }

}