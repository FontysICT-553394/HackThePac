using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PacMan : MonoBehaviour
{
    private GameManager _gameManager;
    private GameObject _pelletTilemap;
    private GameObject _powerPelletTilemap;
    
    private BoxCollider2D _pacmanCollider2D;
    private TilemapCollider2D _powerPelletTilemapCollider2D;
    private TilemapCollider2D _pelletTilemapCollider2D;
    
    private List<BoxCollider2D> ghostColliders = new List<BoxCollider2D>();
    
    private void Start()
    {
        _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        _pelletTilemap = GameObject.Find("pelletsTilemap");
        _powerPelletTilemap = GameObject.Find("powerpelletsTilemap");
        
        _gameManager = FindObjectOfType<GameManager>();
        _pacmanCollider2D = gameObject.GetComponent<BoxCollider2D>();
        _pelletTilemapCollider2D = _pelletTilemap.GetComponent<TilemapCollider2D>();
        _powerPelletTilemapCollider2D = _powerPelletTilemap.GetComponent<TilemapCollider2D>();
        
        ghostColliders.Add(GameObject.Find("blinky(Clone)").TryGetComponent<BoxCollider2D>(out var blinkyCollider) ? blinkyCollider : null);
        //TODO: Enable when ghost prefabs are fixed to have colliders
        // ghostColliders.Add(GameObject.Find("pinky(Clone)").TryGetComponent<BoxCollider2D>(out var pinkyCollider) ? pinkyCollider : null);
        // ghostColliders.Add(GameObject.Find("inky(Clone)").TryGetComponent<BoxCollider2D>(out var inkyCollider) ? inkyCollider : null);
        // ghostColliders.Add(GameObject.Find("clyde(Clone)").TryGetComponent<BoxCollider2D>(out var clydeCollider) ? clydeCollider : null);
    }

    private void Update()
    {
        if (_pelletTilemapCollider2D.IsTouching(_pacmanCollider2D))
            EatPellet();
        
        if (_powerPelletTilemapCollider2D.IsTouching(_pacmanCollider2D))
            EatPowerPellet();

        foreach (var ghostCollider in ghostColliders)
        {
            if (_pacmanCollider2D.IsTouching(ghostCollider))
                OnCollisionWithGhost();
        }
    }
    
    private void EatPellet()
    {
        var pelletTilemapComponent = _pelletTilemap.GetComponent<Tilemap>();
        var cellPos = pelletTilemapComponent.WorldToCell(gameObject.transform.position);

        if (pelletTilemapComponent.HasTile(cellPos))
        {
            _gameManager.PelletEaten(pelletTilemapComponent, cellPos);
            _gameManager.AddScore(10f);
        }
    }
    
    private void EatPowerPellet()
    {
        var powerTilemapComponent = _powerPelletTilemap.GetComponent<Tilemap>();
        var cellPos = powerTilemapComponent.WorldToCell(gameObject.transform.position);
        
        if (powerTilemapComponent.HasTile(cellPos))
        {
            _gameManager.PelletEaten(powerTilemapComponent, cellPos, true);
            _gameManager.AddScore(50f);
        }
        
    }

    private void OnCollisionWithGhost()
    {
        Debug.Log("Ghost collision");
        _gameManager.PacManDied();
    }
}
