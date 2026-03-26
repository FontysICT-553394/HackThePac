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
    
    public GameObject[] ghosts;
    
    void Start()
    {
        SetLevel(0);
    }

    void Update()
    {
        if (currentLevel > 0)
        {
            timer -= Time.deltaTime;

            if (timer <= 0f && stepDownCoroutine == null)
            {
                stepDownCoroutine = StartCoroutine(StepDownRoutine());
            }
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
    
    private IEnumerator StepDownRoutine()
    {
        yield return new WaitForSeconds(idleTime);

        while (currentLevel > 0)
        {
            yield return new WaitForSeconds(stepDownDelay);
            currentLevel--;
            SetLevel(currentLevel);
        }

        stepDownCoroutine = null;
    }

    public void TriggerStep()
    {
        if (stepDownCoroutine != null)
        {
            StopCoroutine(stepDownCoroutine);
        }

        currentLevel++;
        if (currentLevel > 4)
            currentLevel = 4;

        SetLevel(currentLevel);

        stepDownCoroutine = StartCoroutine(StepDownRoutine());
    }

    private void SetLevel(int level)
    {
        tilemap1.SetActive(level == 1);
        tilemap2.SetActive(level == 2);
        tilemap3.SetActive(level == 3);
        tilemap4.SetActive(level == 4);
    
         DisableGhostMovement(level == 4);
    }

    private void DisableGhostMovement(bool disableMovement)
    {
        foreach (GameObject ghost in ghosts)
        {
            MonoBehaviour movementScript = ghost.GetComponent<MonoBehaviour>();
            if (movementScript != null)
            {
                movementScript.enabled = !disableMovement;
            }
        }
    }
}
    
    
    
    


