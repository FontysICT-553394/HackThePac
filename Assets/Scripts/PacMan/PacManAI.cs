
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PacManAI : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] public LayerMask wallLayer;
    [SerializeField] private float raycastDistance = 0.225f;
    [SerializeField] private Vector2 boxCastSize = Vector2.one * 0.75f;

    [Header("AI Tuning")] 
    // How close a ghost must be (in units) before PacMan reacts to it.
    // Increase to make PacMan react to ghosts from further away; decrease to make it braver.
    [SerializeField] private float ghostDangerRadius = 3f; 
    
    // How strongly PacMan avoids (or chases when powered up) ghosts within danger radius.
    // Higher values = more aggressive avoidance/chasing; lower values = ghost proximity matters less.
    [SerializeField] private float ghostPenaltyWeight = 150f; 
    
    // How strongly PacMan is attracted toward nearby pellets.
    // Higher values = PacMan prioritizes pellets more aggressively.
    [SerializeField] private float pelletRewardWeight = 100f; 
    
    // Bonus score added when continuing in the same direction (smoother movement).
    // Increase to prefer straight paths; set to 0 to disable the bonus entirely.
    [SerializeField] private float directionChangeBonus = 0f;
    
    // Score penalty applied when PacMan considers reversing its current direction.
    // Increase to strongly discourage U-turns; decrease to allow more free reversal.
    [SerializeField] private float reverseDirectionPenalty = 40f;
    
    // How often (in seconds) PacMan recalculates its best direction.
    // Lower values = more reactive but more CPU usage; higher values = smoother but slower reaction.
    [SerializeField] private float decisionInterval = 0.05f;

    private GameObject _blinky, _pinky, _inky, _clyde;
    private List<GameObject> _ghosts = new List<GameObject>();

    private Tilemap _pelletTilemap;
    private Tilemap _powerPelletTilemap;

    private Vector2 _currentDirection = Vector2.zero;
    private Vector2 _queuedDirection = Vector2.zero;
    private Rigidbody2D _rb;
    private PacMan _pacMan;
    private float _decisionTimer;

    private static readonly Vector2[] Directions =
    {
        Vector2.up, 
        Vector2.down, 
        Vector2.left, 
        Vector2.right
    };

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _pacMan = GetComponent<PacMan>();

        _blinky = GameObject.Find("blinky(Clone)");
        _pinky = GameObject.Find("pinky(Clone)");
        _inky = GameObject.Find("inky(Clone)");
        _clyde = GameObject.Find("clyde(Clone)");

        if (_blinky != null) _ghosts.Add(_blinky);
        if (_pinky != null) _ghosts.Add(_pinky);
        if (_inky != null) _ghosts.Add(_inky);
        if (_clyde != null) _ghosts.Add(_clyde);

        _pelletTilemap = GameObject.Find("pelletsTilemap").GetComponent<Tilemap>();
        _powerPelletTilemap = GameObject.Find("powerpelletsTilemap").GetComponent<Tilemap>();
        wallLayer = LayerMask.GetMask("walls");
    }

    private void Update()
    {
        _decisionTimer -= Time.deltaTime;
        if (_decisionTimer <= 0f)
        {
            _decisionTimer = decisionInterval;
            ChooseBestDirection();
        }

        TryQueuedDirection();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void ChooseBestDirection()
    {
        float bestScore = float.NegativeInfinity;
        Vector2 bestDir = Vector2.zero;

        foreach (var dir in Directions)
        {
            if (!CanMove(dir))
                continue;

            float score = EvaluateDirection(dir);
            if (score > bestScore)
            {
                bestScore = score;
                bestDir = dir;
            }
        }

        if (bestDir != Vector2.zero)
            _queuedDirection = bestDir;
    }

    // Scores a given direction based on pellet proximity, ghost proximity, and movement continuity.
    // Higher score = more desirable direction for PacMan to move in.
    private float EvaluateDirection(Vector2 direction)
    {
        float score = 0f;
        // Project PacMan slightly forward in the candidate direction to evaluate what is ahead.
        Vector2 futurePos = _rb.position + direction * 0.5f;

        // Pellet attraction: reward directions that lead closer to a pellet.
        // Score increases as pellet distance decreases (inverse relationship).
        float nearestPelletDist = FindNearestPelletDistance(futurePos);
        if (nearestPelletDist > 0f)
            score += pelletRewardWeight / nearestPelletDist;

        // Ghost avoidance/chasing: apply a penalty or bonus based on ghost proximity.
        bool isPoweredUp = _pacMan != null && _pacMan.isPoweredUp;
        foreach (var ghost in _ghosts)
        {
            if (ghost == null) continue;
            float dist = Vector2.Distance(futurePos, ghost.transform.position);

            if (dist < ghostDangerRadius)
            {
                if (isPoweredUp)
                {
                    // When powered up, chase ghosts: higher score for directions closer to a ghost.
                    score += ghostPenaltyWeight / Mathf.Max(dist, 0.1f);
                }
                else
                {
                    // When not powered up, flee: penalize directions that lead closer to a ghost.
                    // Mathf.Max prevents division by zero if PacMan overlaps with a ghost.
                    score -= ghostPenaltyWeight / Mathf.Max(dist, 0.1f);
                }
            }
        }

        // Discourage reversing direction to prevent PacMan from oscillating back and forth.
        if (_currentDirection != Vector2.zero && direction == -_currentDirection)
            score -= reverseDirectionPenalty;

        // Optionally reward continuing in the same direction for smoother, more natural movement.
        if (direction == _currentDirection)
            score += directionChangeBonus;

        return score;
    }
    
    // Returns the distance to the nearest pellet (regular or power) from the given position.
    // Returns float.MaxValue if no pellets are found (both tilemaps are empty).
    private float FindNearestPelletDistance(Vector2 position)
    {
        float nearest = float.MaxValue;

        SearchTilemap(_pelletTilemap, position, ref nearest);
        SearchTilemap(_powerPelletTilemap, position, ref nearest);

        return nearest;
    }

    // Iterates over every cell in the tilemap and updates 'nearest' if a closer tile is found.
    // Uses 'ref' so the same nearest value can be updated across multiple tilemap searches.
    private void SearchTilemap(Tilemap tilemap, Vector2 position, ref float nearest)
    {
        BoundsInt bounds = tilemap.cellBounds;
        foreach (var cellPos in bounds.allPositionsWithin)
        {
            if (!tilemap.HasTile(cellPos))
                continue;

            Vector3 worldPos = tilemap.GetCellCenterWorld(cellPos);
            float dist = Vector2.Distance(position, worldPos);
            if (dist < nearest)
                nearest = dist;
        }
    }

    private void TryQueuedDirection()
    {
        if (_queuedDirection != Vector2.zero && CanMove(_queuedDirection))
        {
            _currentDirection = _queuedDirection;
            _queuedDirection = Vector2.zero;
        }
    }

    private void Move()
    {
        if (_currentDirection != Vector2.zero && CanMove(_currentDirection))
        {
            float finalSpeed = moveSpeed + GameSettings.instance.pacmanSpeed;
            Vector2 translation = finalSpeed * Time.fixedDeltaTime * _currentDirection;
            _rb.MovePosition(_rb.position + translation);
        }
    }

    private bool CanMove(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, boxCastSize, 0f, direction, raycastDistance, wallLayer);
        return hit.collider == null;
    }
}
