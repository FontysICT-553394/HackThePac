using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Movement))]
public class Pacman : MonoBehaviour
{
    public int id;
    public PlayerControl controlledBy;
    public Movement movement { get; private set; }
    public SpriteRenderer bodyRenderer;

    private Transform nearestPellet;
    private Transform nearestPowerPellet;
    private Transform nearestGhost;
    
    public bool isDead { get; private set; }
    
    public bool canChomp = false;
    public bool isChomping { get; private set; }
    
    public AnimatedSprite deathAnim;
    public float ghostFearDistance = 12f;
    
    public InputAction Move;
    public InputActionAsset actions;
    
    private void OnEnable() => Move.Enable();
    private void OnDisable() => Move.Disable();

    private void Awake()
    {
        movement = GetComponent<Movement>();
    }

    private void Update()
    {
        if (movement != null && movement.enabled)
        {
            // AI Logic if not controlled by a human
            if (controlledBy == null)
            {
                Think();
            }

            // Rotation Logic
            float angle = Mathf.Atan2(movement.direction.y, movement.direction.x);
            transform.rotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, Vector3.forward);
            
            bool isMoving = movement.direction != Vector2.zero;
            isChomping = canChomp && isMoving;
        }
        else
        {
            isChomping = false;
        }
    }

    /// <summary>
    /// An AI for Pac-Man if he is not being controlled by the player.
    /// </summary>
    /// <returns></returns>
    private void Think()
    {
        FindNearestPellet();
        FindNearestPowerPellet();
        FindNearestGhost();

        Vector2[] dirs = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        Vector2 chosen = movement.direction;
        float bestScore = float.MinValue;

        foreach (var dir in dirs)
        {
            if (dir == -movement.direction) continue;
            if (movement.Occupied(dir)) continue;

            Vector3 newPos = transform.position + new Vector3(dir.x, dir.y, 0);
            float score = 0f;

            if (nearestPowerPellet != null)
            {
                float dist = Vector3.Distance(nearestPowerPellet.position, newPos);
                score += 12f / (dist + 0.1f);
            }

            if (nearestPellet != null)
            {
                float distPellet = Vector3.Distance(nearestPellet.position, newPos);
                score += 100f / (distPellet + 0.1f);
            }

            if (nearestGhost != null)
            {
                float distGhost = Vector3.Distance(nearestGhost.position, newPos);
                Ghost ghostScript = nearestGhost.GetComponent<Ghost>();
                
                if (ghostScript.frightened.enabled)
                {
                    score += 1000f * (1f - distGhost / ghostFearDistance);
                }
                else
                {
                    if (distGhost < ghostFearDistance)
                        score -= 1000f * (1f - distGhost / ghostFearDistance);
                    else
                        score -= 50f / (distGhost + 0.1f);
                }
            }

            if (dir == movement.direction) score += 0.3f;
            score += Random.Range(0f, 0.05f);

            if (score > bestScore)
            {
                bestScore = score;
                chosen = dir;
            }
        }

        movement.SetDirection(chosen);
    }

    /// <summary>
    /// Finds the Pac-Dot closest to Pac-Man. Used by the AI in the Think() function
    /// </summary>
    /// <returns></returns>
    private void FindNearestPellet()
    {
        Pellet[] allPellets = Object.FindObjectsByType<Pellet>(FindObjectsSortMode.None);
        nearestPellet = null;
        float minDist = float.MaxValue;

        foreach (Pellet p in allPellets)
        {
            if (!p.gameObject.activeSelf) continue;
            float dist = (p.transform.position - transform.position).sqrMagnitude;
            if (dist < minDist)
            {
                minDist = dist;
                nearestPellet = p.transform;
            }
        }
    }

    /// <summary>
    /// Finds the Power-Pellet closest to Pac-Man. Used by the AI in the Think() function
    /// </summary>
    /// <returns></returns>
    private void FindNearestPowerPellet()
    {
        PowerPellet[] allPower = Object.FindObjectsByType<PowerPellet>(FindObjectsSortMode.None);
        nearestPowerPellet = null;
        float minDist = float.MaxValue;

        foreach (PowerPellet p in allPower)
        {
            if (!p.gameObject.activeSelf) continue;
            float dist = (p.transform.position - transform.position).sqrMagnitude;
            if (dist < minDist)
            {
                minDist = dist;
                nearestPowerPellet = p.transform;
            }
        }
    }

    /// <summary>
    /// Finds the Ghost closest to Pac-Man. Used by the AI in the Think() function
    /// </summary>
    /// <returns></returns>
    private void FindNearestGhost()
    {
        Ghost[] allGhosts = Object.FindObjectsByType<Ghost>(FindObjectsSortMode.None);
        nearestGhost = null;
        float minDist = float.MaxValue;
        bool chasingFrightened = false;

        foreach (Ghost g in allGhosts)
        {
            if (!g.gameObject.activeSelf || !g.isOutside) continue;

            float dist = (g.transform.position - transform.position).sqrMagnitude;

            if (g.frightened.enabled)
            {
                if (dist < minDist)
                {
                    minDist = dist;
                    nearestGhost = g.transform;
                    chasingFrightened = true;
                }
            }
            else if (!chasingFrightened)
            {
                if (dist < minDist)
                {
                    minDist = dist;
                    nearestGhost = g.transform;
                }
            }
        }
    }

    /// <summary>
    /// Resets the Pac-Man's actions and changes him to be active
    /// </summary>
    /// <returns></returns>
    public void ResetState()
    {
        isDead = false;
        movement.ResetState();

        if (deathAnim != null)
        {
            deathAnim.Restart();
            deathAnim.gameObject.SetActive(false);
        }
    
        if (bodyRenderer != null) bodyRenderer.enabled = true;

        isChomping = false; 
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Plays death animation and turns off collision so no ghost can chase him or touch him.
    /// </summary>
    /// <returns></returns>
    public IEnumerator PlayDeathAnimation()
    {
        isDead = true; 
        isChomping = false;
        movement.enabled = false;
        
        GetComponent<Collider2D>().enabled = false;
        
        
        if (bodyRenderer != null) bodyRenderer.enabled = false;

        if (deathAnim != null)
        {
            deathAnim.gameObject.SetActive(true);
            deathAnim.Restart();
            while (!deathAnim.IsFinished) yield return null;
            deathAnim.gameObject.SetActive(false);
        }
        else
        {
            yield return new WaitForSeconds(2f);
        }
    }
}