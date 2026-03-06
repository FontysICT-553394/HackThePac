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
        // Don't cache here — ghosts aren't spawned yet
    }

    private void Update()
    {
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
        ghosts.RemoveAll(g => g == null);

        // Only search when list is incomplete (avoids per-frame allocation)
        if (ghosts.Count < 4)
        {
            var found = FindObjectsOfType<GhostMovement>();
            foreach (var g in found)
            {
                if (!ghosts.Contains(g))
                    ghosts.Add(g);
            }
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
        phases.Add(new Phase(GhostMovement.Mode.Scatter, 7f));
        phases.Add(new Phase(GhostMovement.Mode.Chase, 20f));
        phases.Add(new Phase(GhostMovement.Mode.Scatter, 7f));
        phases.Add(new Phase(GhostMovement.Mode.Chase, 20f));
        phases.Add(new Phase(GhostMovement.Mode.Scatter, 5f));
        phases.Add(new Phase(GhostMovement.Mode.Chase, -1f));
    }

    public void TriggerFrightened(float duration)
    {
        FindGhosts(); // Ensure list is up to date before applying
        foreach (var g in ghosts)
        {
            if (g != null)
                g.SetMode(GhostMovement.Mode.Frightened, true);
        }

        CancelInvoke(nameof(EndFrightened)); // Cancel any existing timer
        Invoke(nameof(EndFrightened), duration);
    }

    void EndFrightened()
    {
        if (phase < phases.Count)
            ApplyMode(phases[phase].mode);
    }
}
