using UnityEngine;
using UnityEngine.UI;
using System.Collections;
 
public class GlitchEffect : MonoBehaviour
{
    public Image flashImage;
    public float flashDuration = 0.1f;

    public GameObject tilemap1;
    public GameObject tilemap2;
    public GameObject tilemap3;
    public GameObject tilemap4;

    public float idleTime = 2f;
    public float stepDownDelay = 1f; // tijd tussen 3->2->1->uit

    private int currentLevel = 0;
    private float timer = 0f;
    private Coroutine stepDownCoroutine;    
    
    private bool _isPacmanInGlitchRoom;
    
    
    void Start()
    {
        SetLevel(0);
        UpdateFreezeFromLevel();
    }

    void Update()
    {
        if (currentLevel > 0)
        {
            timer -= Time.deltaTime;

            if (timer <= 0f && stepDownCoroutine == null && !IsLockedAtMaxLevel())
            {
                stepDownCoroutine = StartCoroutine(StepDownRoutine());
            }
        }
    }

    public void SetPacmanInGlitchRoom(bool isInRoom)
    {
        bool wasLockedAtMax = _isPacmanInGlitchRoom && currentLevel >= 4;
        _isPacmanInGlitchRoom = isInRoom;

        // If PacMan enters the room while already on level 4, keep it locked.
        if (isInRoom && IsLockedAtMaxLevel() && stepDownCoroutine != null)
        {
            StopCoroutine(stepDownCoroutine);
            stepDownCoroutine = null;
        }

        // When PacMan leaves the room, allow the max-level lock to step down again.
        if (!isInRoom && wasLockedAtMax)
        {
            timer = 0f;
            if (stepDownCoroutine == null)
                stepDownCoroutine = StartCoroutine(StepDownRoutine());
        }
    }
    
    public void Glitch(){
        TriggerStep();
        Flash();
    }
    public void Flash()
    {
        StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        flashImage.color = new Color(0, 0, 1, 0.2f);
        yield return new WaitForSeconds(flashDuration);
        flashImage.color = new Color(1, 1, 1, 0);
    }
    
    public void TriggerStep()
    {
        if (stepDownCoroutine != null)
        {
            StopCoroutine(stepDownCoroutine);
            stepDownCoroutine = null;
        }

        currentLevel++;
        if (currentLevel > 4)
            currentLevel = 4;

        SetLevel(currentLevel);
        UpdateFreezeFromLevel();

        timer = idleTime;

        // If we've reached max level inside the glitch room, keep it there.
        if (IsLockedAtMaxLevel())
        {
            // Ensure no step-down is running.
            if (stepDownCoroutine != null)
            {
                StopCoroutine(stepDownCoroutine);
                stepDownCoroutine = null;
            }
            timer = Mathf.Infinity;
        }
    }

    private IEnumerator StepDownRoutine()
    {
        while (currentLevel > 0)
        {
            if (IsLockedAtMaxLevel())
                break;

            yield return new WaitForSeconds(stepDownDelay);

            if (IsLockedAtMaxLevel())
                break;

            currentLevel--;
            SetLevel(currentLevel);
            UpdateFreezeFromLevel();
        }

        stepDownCoroutine = null;
    }

    private bool IsLockedAtMaxLevel()
    {
        return _isPacmanInGlitchRoom && currentLevel >= 4;
    }

    private void UpdateFreezeFromLevel()
    {
        if (GameSettings.instance == null)
            return;

        GameSettings.instance.FreezeEnabled = currentLevel == 4;
    }

    private void SetLevel(int level)
    {
        tilemap1.SetActive(level == 1);
        tilemap2.SetActive(level == 2);
        tilemap3.SetActive(level == 3);
        tilemap4.SetActive(level == 4); 
    }
}