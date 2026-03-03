using UnityEngine;

public class GhostHouseController : MonoBehaviour
{
    public static GhostHouseController Instance { get; private set; }

    [Header("Ghost refs (alleen de 3 die starten in house)")]
    [SerializeField] private GhostMovement pinky;
    [SerializeField] private GhostMovement inky;
    [SerializeField] private GhostMovement clyde;

    [Header("Level")]
    [SerializeField] private int level = 1; // 1-based

    // Personal dot counters (alleen 1 actief tegelijk)
    private int pinkyDots, inkyDots, clydeDots;

    // Global dot counter (na life lost)
    private bool useGlobalCounter;
    private int globalDots;

    // Inactivity timer
    private float noDotTimer;
    private float noDotLimit => (level >= 5) ? 3f : 4f;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        BeginLevel(level);
    }

    public void BeginLevel(int newLevel)
    {
        level = newLevel;

        // reset personal counters
        pinkyDots = inkyDots = clydeDots = 0;

        // normal mode: personal counters
        useGlobalCounter = false;
        globalDots = 0;

        noDotTimer = 0f;

        // Arcade: Pinky dot limit = 0 => leaves immediately at start
        TryReleaseByRules(forceImmediateChecks: true);
    }

    public void OnLifeLost()
    {
        // Arcade: switch to global counter, reset it
        useGlobalCounter = true;
        globalDots = 0;

        // personal counters disabled but not reset in arcade
        // (we keep them as-is; when global deactivates, personal resumes)
        noDotTimer = 0f;
    }

    public void OnDotEaten()
    {
        noDotTimer = 0f;

        if (useGlobalCounter)
        {
            globalDots++;
            TryReleaseByRules(forceImmediateChecks: true);
            return;
        }

        // personal: increment only the highest-priority ghost still inside
        var g = HighestPriorityGhostStillInHouse();
        if (g == pinky) pinkyDots++;
        else if (g == inky) inkyDots++;
        else if (g == clyde) clydeDots++;

        TryReleaseByRules(forceImmediateChecks: true);
    }

    private void Update()
    {
        noDotTimer += Time.deltaTime;

        if (noDotTimer >= noDotLimit)
        {
            // force the highest-priority ghost out (if any still inside)
            var g = HighestPriorityGhostStillInHouse();
            if (g != null)
                g.ReleaseFromHouse();

            noDotTimer = 0f;
        }
    }


    private void TryReleaseByRules(bool forceImmediateChecks)
    {
        // release decisions can chain (pinky leaves -> inky counter active, etc.)
        bool releasedSomeone;
        int safety = 0;

        do
        {
            releasedSomeone = false;
            safety++;

            if (useGlobalCounter)
            {
                if (pinky != null && pinky.IsInHouse && globalDots >= 7) { pinky.ReleaseFromHouse(); releasedSomeone = true; }
                else if (inky != null && inky.IsInHouse && globalDots >= 17) { inky.ReleaseFromHouse(); releasedSomeone = true; }
                else if (clyde != null && clyde.IsInHouse && globalDots >= 32)
                {
                    clyde.ReleaseFromHouse();
                    releasedSomeone = true;

                    // Arcade: only deactivates if Clyde is inside when it hits 32
                    // (we just released him now, so yes)
                    useGlobalCounter = false;
                    globalDots = 0;
                }
            }
            else
            {
                // personal dot limits per level
                int inkyLimit = GetInkyLimit(level);
                int clydeLimit = GetClydeLimit(level);

                // Pinky limit always 0 (immediate)
                if (pinky != null && pinky.IsInHouse && 0 <= 0) { pinky.ReleaseFromHouse(); releasedSomeone = true; }
                else if (inky != null && inky.IsInHouse && inkyDots >= inkyLimit) { inky.ReleaseFromHouse(); releasedSomeone = true; }
                else if (clyde != null && clyde.IsInHouse && clydeDots >= clydeLimit) { clyde.ReleaseFromHouse(); releasedSomeone = true; }
            }

        } while (releasedSomeone && safety < 10);
    }

    private GhostMovement HighestPriorityGhostStillInHouse()
    {
        if (pinky != null && pinky.IsInHouse) return pinky;
        if (inky != null && inky.IsInHouse) return inky;
        if (clyde != null && clyde.IsInHouse) return clyde;
        return null;
    }

    private static int GetInkyLimit(int lvl)
    {
        if (lvl == 1) return 30;
        if (lvl == 2) return 0;
        return 0; // lvl 3+
    }

    private static int GetClydeLimit(int lvl)
    {
        if (lvl == 1) return 60;
        if (lvl == 2) return 50;
        return 0; // lvl 3+
    }
}