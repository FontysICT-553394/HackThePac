using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class GameManager : MonoBehaviour
{
    [Header("PacMan Settings")]
    [SerializeField] private GameObject pacmanPrefab;
    [SerializeField] private Transform pacmanSpawnPoint;
    
    [Header("Ghost Settings")]
    [SerializeField] private List<GameObject> ghostPrefabs;
    [SerializeField] private List<Transform> ghostSpawn;
    
    private GameObject pacmanInstance;
    private List<GameObject> ghostInstances = new List<GameObject>();

    public void Start()
    {
        InstantiatePacman();
        InstantiateGhosts();
        
        AddPlayerScriptToPlayer();
        AddAiScriptToEnemies();
    }
    
    private void InstantiatePacman()
    {
        if (pacmanInstance != null)
            Destroy(pacmanInstance);
        
        pacmanInstance = Instantiate(pacmanPrefab, pacmanSpawnPoint.position, Quaternion.identity);
    }
    
    private void InstantiateGhosts()
    {
        foreach (var ghost in ghostInstances)
            Destroy(ghost);
        ghostInstances.Clear();
        
        var j = 0;
        for (int i = 0; i < GameSettings.instance.ghostAmount; i++)
        {
            var ghostInstance = Instantiate(ghostPrefabs[j], ghostSpawn[j].position, Quaternion.identity);
            ghostInstances.Add(ghostInstance);
            
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
        
        foreach (var ghost in ghostInstances)
        {
            if (ghost.name == GameSettings.instance.selectedCharacter + "(Clone)")
                continue;
            
            //TODO: Add ghost AI
        }
    }
}
