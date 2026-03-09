using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PacMan : MonoBehaviour
{
    public bool isPoweredUp = false;
    
    private GameManager _gameManager;
    private GameObject _pelletTilemap;
    private GameObject _powerPelletTilemap;
    
    private BoxCollider2D _pacmanCollider2D;
    private TilemapCollider2D _powerPelletTilemapCollider2D;
    private TilemapCollider2D _pelletTilemapCollider2D;

    private Tilemap _pelletTilemapComponent;
    private Tilemap _powerPelletTilemapComponent;
    private List<CircleCollider2D> ghostColliders = new();

    private bool _isDead = false;
    private readonly float minMovementForRotation = 0.01f;
    private Vector3 _previousPosition;
    
    private void Start()
    {
        _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        _pelletTilemap = GameObject.Find("pelletsTilemap");
        _powerPelletTilemap = GameObject.Find("powerpelletsTilemap");
        _pelletTilemapComponent = _pelletTilemap.GetComponent<Tilemap>();
        _powerPelletTilemapComponent = _powerPelletTilemap.GetComponent<Tilemap>();
        
        _gameManager = FindObjectOfType<GameManager>();
        _pacmanCollider2D = gameObject.GetComponent<BoxCollider2D>();
        _pelletTilemapCollider2D = _pelletTilemap.GetComponent<TilemapCollider2D>();
        _powerPelletTilemapCollider2D = _powerPelletTilemap.GetComponent<TilemapCollider2D>();
        
        ghostColliders.Add(GameObject.Find("Blinky(Clone)").TryGetComponent<CircleCollider2D>(out var blinkyCollider) ? blinkyCollider : null);
        ghostColliders.Add(GameObject.Find("Pinky(Clone)").TryGetComponent<CircleCollider2D>(out var pinkyCollider) ? pinkyCollider : null);
        ghostColliders.Add(GameObject.Find("Inky(Clone)").TryGetComponent<CircleCollider2D>(out var inkyCollider) ? inkyCollider : null);
        ghostColliders.Add(GameObject.Find("Clyde(Clone)").TryGetComponent<CircleCollider2D>(out var clydeCollider) ? clydeCollider : null);
    }

    private void Update()
    {
        if (_pelletTilemapCollider2D.IsTouching(_pacmanCollider2D))
            EatPellet();

        if (_powerPelletTilemapCollider2D.IsTouching(_pacmanCollider2D))
            EatPowerPellet();

        if (!isPoweredUp)
        {
            foreach (var ghostCollider in ghostColliders)
            {
                if (ghostCollider == null) continue;

                if (_pacmanCollider2D.IsTouching(ghostCollider) && !_isDead)
                {
                    GhostFrightened frightened = ghostCollider.GetComponent<GhostFrightened>();
                    if (frightened != null && frightened.enabled)
                        continue;

                    OnCollisionWithGhost();
                }
            }
        }
        else
        {
            foreach (var ghostCollider in ghostColliders)
            {
                if (ghostCollider == null) continue;

                if (_pacmanCollider2D.IsTouching(ghostCollider))
                {
                    Ghost ghost = ghostCollider.GetComponent<Ghost>();
                    if (ghost != null && ghost.frightened.enabled && !ghost.frightened.eaten)
                    {
                        _gameManager.GhostEaten(ghost);
                        ghost.frightened.Eaten();
                    }
                }
            }
        }

        RotateToMovementDirection();
        _previousPosition = transform.position;
    }
    
    private void RotateToMovementDirection()
    {
        Vector3 delta = transform.position - _previousPosition;
        if (delta.magnitude < minMovementForRotation) return;

        float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
    
    private void EatPellet()
    {
        var cellPos = _pelletTilemapComponent.WorldToCell(gameObject.transform.position);

        if (_pelletTilemapComponent.HasTile(cellPos))
        {
            _gameManager.PelletEaten(_pelletTilemapComponent, cellPos);
            _gameManager.AddScore(10f);
        }
    }
    
    private void EatPowerPellet()
    {
        var cellPos = _powerPelletTilemapComponent.WorldToCell(gameObject.transform.position);
        
        if (_powerPelletTilemapComponent.HasTile(cellPos))
        {
            _gameManager.PelletEaten(_powerPelletTilemapComponent, cellPos, true);
            _gameManager.AddScore(50f);
        }
        
    }

    private void OnCollisionWithGhost()
    {
        // GhostHouseController.Instance.OnLifeLost();
        _isDead = true;
        _gameManager.PacManDied();
    }
    
}