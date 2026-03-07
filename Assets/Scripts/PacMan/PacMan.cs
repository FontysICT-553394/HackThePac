using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PacMan : MonoBehaviour
{
    public bool isPoweredUp = false;

    [Header("Power Pellet")]
    [SerializeField] private float powerDuration = 6f;
    private float powerTimer;
    private GhostModeController _ghostModeController;

    private readonly int[] ghostEatScores = { 200, 400, 800, 1600 };
    private int ghostsEatenThisPower = 0;

    private GameManager _gameManager;
    private GameObject _pelletTilemap;
    private GameObject _powerPelletTilemap;

    private BoxCollider2D _pacmanCollider2D;
    private TilemapCollider2D _powerPelletTilemapCollider2D;
    private TilemapCollider2D _pelletTilemapCollider2D;

    private Tilemap _pelletTilemapComponent;
    private Tilemap _powerPelletTilemapComponent;
    private readonly List<BoxCollider2D> ghostColliders = new List<BoxCollider2D>();

    private bool _isDead = false;
    private readonly float minMovementForRotation = 0.01f;
    private Vector3 _previousPosition;

    public Vector2Int MoveDir { get; private set; } = Vector2Int.left;

    private void Start()
    {
        _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        _pelletTilemap = GameObject.Find("pelletsTilemap");
        _powerPelletTilemap = GameObject.Find("powerpelletsTilemap");

        _pelletTilemapComponent = _pelletTilemap.GetComponent<Tilemap>();
        _powerPelletTilemapComponent = _powerPelletTilemap.GetComponent<Tilemap>();

        _pacmanCollider2D = gameObject.GetComponent<BoxCollider2D>();
        _pelletTilemapCollider2D = _pelletTilemap.GetComponent<TilemapCollider2D>();
        _powerPelletTilemapCollider2D = _powerPelletTilemap.GetComponent<TilemapCollider2D>();

        _ghostModeController = FindObjectOfType<GhostModeController>();

        var blinkyObj = GameObject.Find("blinky(Clone)") ?? GameObject.Find("blinky");
        var pinkyObj = GameObject.Find("pinky(Clone)") ?? GameObject.Find("pinky");
        var inkyObj = GameObject.Find("inky(Clone)") ?? GameObject.Find("inky");
        var clydeObj = GameObject.Find("clyde(Clone)") ?? GameObject.Find("clyde");

        if (blinkyObj && blinkyObj.TryGetComponent<BoxCollider2D>(out var blinkyCollider)) ghostColliders.Add(blinkyCollider);
        if (pinkyObj && pinkyObj.TryGetComponent<BoxCollider2D>(out var pinkyCollider)) ghostColliders.Add(pinkyCollider);
        if (inkyObj && inkyObj.TryGetComponent<BoxCollider2D>(out var inkyCollider)) ghostColliders.Add(inkyCollider);
        if (clydeObj && clydeObj.TryGetComponent<BoxCollider2D>(out var clydeCollider)) ghostColliders.Add(clydeCollider);
    }

    private void Update()
    {
        if (_pelletTilemapCollider2D.IsTouching(_pacmanCollider2D))
            EatPellet();

        if (_powerPelletTilemapCollider2D.IsTouching(_pacmanCollider2D))
            EatPowerPellet();

        if (isPoweredUp)
        {
            powerTimer -= Time.deltaTime;
            if (powerTimer <= 0f)
            {
                isPoweredUp = false;
                ghostsEatenThisPower = 0;
            }
        }

        foreach (var ghostCollider in ghostColliders)
        {
            if (ghostCollider == null) continue;
            if (!_pacmanCollider2D.IsTouching(ghostCollider)) continue;
            if (_isDead) break;

            if (isPoweredUp)
                TryEatGhost(ghostCollider);
            else
                OnCollisionWithGhost();
        }

        RotateToMovementDirection();
        _previousPosition = transform.position;
    }

    private void RotateToMovementDirection()
    {
        Vector3 delta = transform.position - _previousPosition;
        if (delta.magnitude < minMovementForRotation) return;

        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            MoveDir = delta.x > 0 ? Vector2Int.right : Vector2Int.left;
        else
            MoveDir = delta.y > 0 ? Vector2Int.up : Vector2Int.down;

        float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void EatPellet()
    {
        var cellPos = _pelletTilemapComponent.WorldToCell(transform.position);

        if (_pelletTilemapComponent.HasTile(cellPos))
        {
            _gameManager.PelletEaten(_pelletTilemapComponent, cellPos);
            _gameManager.AddScore(10f);
        }
    }

    private void EatPowerPellet()
    {
        var cellPos = _powerPelletTilemapComponent.WorldToCell(transform.position);

        if (_powerPelletTilemapComponent.HasTile(cellPos))
        {
            _gameManager.PelletEaten(_powerPelletTilemapComponent, cellPos, true);
            _gameManager.AddScore(50f);

            isPoweredUp = true;
            powerTimer = powerDuration;
            ghostsEatenThisPower = 0;

            if (_ghostModeController != null)
                _ghostModeController.TriggerFrightened(powerDuration);
        }
    }

    private void TryEatGhost(BoxCollider2D ghostCollider)
    {
        GhostMovement ghostMovement = ghostCollider.GetComponent<GhostMovement>();
        if (ghostMovement == null) return;
        if (!ghostMovement.CanBeEaten) return;

        int scoreIndex = Mathf.Min(ghostsEatenThisPower, ghostEatScores.Length - 1);
        _gameManager.AddScore(ghostEatScores[scoreIndex]);
        ghostsEatenThisPower++;

        ghostMovement.OnEatenByPacman();
    }

    private void OnCollisionWithGhost()
    {
        _isDead = true;
        _gameManager.PacManDied();
    }
}