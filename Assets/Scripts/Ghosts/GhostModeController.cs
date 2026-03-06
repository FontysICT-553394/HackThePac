using System.Collections.Generic;
using UnityEngine;

public class GhostModeController : MonoBehaviour
{
    private List<GhostMovement> ghosts = new List<GhostMovement>();

    [SerializeField] private int level = 1;

    private float timer;
    private int phase;

    private struct Phase
    {
        public GhostMovement.Mode mode;
        public float duration;

        public Phase(GhostMovement.Mode m, float d)
        {
            mode = m;
            duration = d;
        }
    }

    private List<Phase> phases = new List<Phase>();

    private void Start()
    {
        BuildSchedule();
    }

    private void Update()
    {
        // Zoek automatisch nieuwe GhostMovement scripts
        FindGhosts();

        if (phases.Count == 0) return;

        timer += Time.deltaTime;

        if (timer >= phases[phase].duration)
        {
            timer = 0f;
            phase++;

            if (phase >= phases.Count)
                phase = phases.Count - 1;

            ApplyMode(phases[phase].mode);
        }
    }

    void FindGhosts()
    {
        GhostMovement[] found = FindObjectsOfType<GhostMovement>();

        foreach (var g in found)
        {
            if (!ghosts.Contains(g))
                ghosts.Add(g);
        }
    }

    void ApplyMode(GhostMovement.Mode mode)
    {
        foreach (var g in ghosts)
        {
            if (g != null)
                g.SetMode(mode, true);
        }
    }

    void BuildSchedule()
    {
        phases.Clear();

        // Pac-Man level 1 timing
        phases.Add(new Phase(GhostMovement.Mode.Scatter, 7f));
        phases.Add(new Phase(GhostMovement.Mode.Chase, 20f));
        phases.Add(new Phase(GhostMovement.Mode.Scatter, 7f));
        phases.Add(new Phase(GhostMovement.Mode.Chase, 20f));
        phases.Add(new Phase(GhostMovement.Mode.Scatter, 5f));
        phases.Add(new Phase(GhostMovement.Mode.Chase, -1f));
    }

    public void TriggerFrightened(float duration)
    {
        foreach (var g in ghosts)
        {
            if (g != null)
                g.SetMode(GhostMovement.Mode.Frightened, true);
        }

        Invoke(nameof(EndFrightened), duration);
    }

    void EndFrightened()
    {
        if (phase < phases.Count)
            ApplyMode(phases[phase].mode);
    }
}