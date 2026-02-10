using UnityEngine;

public class Ghost : MonoBehaviour
{
    public PlayerControl controlledBy;
    public Movement movement { get; private set; }
    public GhostHome home { get; private set; }
    public GhostScatter scatter { get; set; }
    public GhostChase chase { get; set; }
    public GhostFrightened frightened { get; set; }
    public Transform target { get; private set; }

    public SpriteRenderer bodyRenderer;

    public Node lastNode;
    public GhostBehavior initialBehavior;

    public int points = 200;
    public int id;
    public bool isOutside;


    private void Update()
    {
        this.target = GetClosestPacman(this.transform.position);
    }

    private void Awake()
    {
        this.movement = GetComponent<Movement>();
        this.home = GetComponent<GhostHome>();
        this.scatter = GetComponent<GhostScatter>();
        this.chase = GetComponent<GhostChase>();
        this.frightened = GetComponent<GhostFrightened>();
    }

    /// <summary>
    /// Resets the Ghost's actions, states, puts them in the Home (center of the maze) and sets the Ghost active
    /// </summary>
    /// <returns></returns>
    public void ResetState()
    {
        this.isOutside = false;
        this.gameObject.SetActive(true);
        this.movement.ResetState();

        this.frightened.Disable();
        this.chase.Disable();
        this.scatter.Enable();
        if (this.home != this.initialBehavior)
        {
            this.home.Disable();
        }

        if (this.initialBehavior != null)
        {
            this.initialBehavior.Enable();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Pacman"))
        {
            Pacman pacman = collision.gameObject.GetComponent<Pacman>();
            
            if (pacman.isDead) return;
            
            if (this.frightened.enabled)
            {
                FindObjectOfType<GameManager>().GhostEaten(this);
            }
            else
            {
                FindObjectOfType<GameManager>().PacmanEaten(pacman);
            }
        }
    }

    /// <summary>
    /// Looks for the nearest Pac-Man in the game, used by the AI
    /// </summary>
    /// <param name="fromPosition"></param>
    /// <returns></returns>
    Transform GetClosestPacman(Vector3 fromPosition)
    {
        int pacmanLayer = LayerMask.NameToLayer("Pacman");

        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        Transform closest = null;
        float minDistance = float.MaxValue;

        foreach (GameObject obj in allObjects)
        {
            if (obj.layer != pacmanLayer) continue;

            float dist = (obj.transform.position - fromPosition).sqrMagnitude;

            if (dist < minDistance)
            {
                minDistance = dist;
                closest = obj.transform;
            }
        }

        return closest;
    }

    /// <summary>
    /// Looks for a new nearest Pac-Man. Used by the AI
    /// </summary>
    /// <returns></returns>
    public void FindNewTarget()
    {
        // Find all Pacmen in the scene
        Pacman[] allPacmen = Object.FindObjectsByType<Pacman>(FindObjectsSortMode.None);
        float minDistance = float.MaxValue;
        Transform closest = null;

        foreach (Pacman p in allPacmen)
        {
            // Only target Pacmen that are active and alive
            if (p.gameObject.activeSelf)
            {
                float dist = (p.transform.position - transform.position).sqrMagnitude;
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closest = p.transform;
                }
            }
        }

        this.target = closest;
    }
}
