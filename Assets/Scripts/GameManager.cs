using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    [Header("PacMan Settings")]
    [SerializeField] private GameObject pacmanPrefab;
    [SerializeField] private Transform pacmanSpawnPoint;
    
    [Header("Ghost Settings")]
    [SerializeField] private List<GameObject> ghostPrefabs;
    [SerializeField] private List<Transform> ghostSpawn;
    
    [Header("Game Settings")]
    [SerializeField] private GameObject pelletTilemap;
    [SerializeField] private GameObject powerPelletTilemap;
    [SerializeField] private TMP_Text scoreText;
    
    private float _score = 0f;
    
    private GameObject _pacmanInstance;
    private List<GameObject> _ghostInstances = new List<GameObject>();
    private TilemapCollider2D _powerPelletTilemapCollider2D;
    private TilemapCollider2D _pelletTilemapCollider2D;
    
    // Colliders
    private BoxCollider2D _pacmanCollider2D;

    public void Start()
    {
        _pelletTilemapCollider2D = pelletTilemap.GetComponent<TilemapCollider2D>();
        _powerPelletTilemapCollider2D = pelletTilemap.GetComponent<TilemapCollider2D>();
        
        InstantiatePacman();
        InstantiateGhosts();
        
        AddPlayerScriptToPlayer();
        AddAiScriptToEnemies();
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
        
        var j = 0;
        for (int i = 0; i < GameSettings.instance.ghostAmount; i++)
        {
            var ghostInstance = Instantiate(ghostPrefabs[j], ghostSpawn[j].position, Quaternion.identity);
            _ghostInstances.Add(ghostInstance);
            
            j++;
            if (j == 4)
                j = 0;
        }
    }

    private void AddPlayerScriptToPlayer()
    {
        var playerCharacterName = GameSettings.instance.selectedCharacter;
        var character = GameObject.Find(playerCharacterName + "(Clone)");
        character.AddComponent<PlayerMovement>().wallLayer = LayerMask.GetMask("walls");
    }
    
    private void AddAiScriptToEnemies()
    {
        if (GameSettings.instance.selectedCharacter != "pacman")
        {
            //TODO: Add pacman ai script
        }
        
        foreach (var ghost in _ghostInstances)
        {
            if (ghost.name == GameSettings.instance.selectedCharacter + "(Clone)")
                continue;
            
            //TODO: Add ghost AI
        }
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

        if (isPowerPellet)
        {
            //TODO: Add power-up effect (eating ghosts)
        }
    }

    public void PacManDied()
    {
        //TODO: Show game-over screen (if you won or not), reset to main menu, etc.
        if (GameSettings.instance.selectedCharacter == "pacman")
        {
            //You Lost
        }
        else
        {
            //You won
        }
    }

    private void AllPelletsEaten()
    {
        //TODO: Show game-over screen (if you won or not), reset to main menu, etc.
        if (GameSettings.instance.selectedCharacter == "pacman")
        {
            //You Won
        }
        else
        {
            //You Lost
        }
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
        scoreText.text = "Score: " + _score;
    }
    
}
