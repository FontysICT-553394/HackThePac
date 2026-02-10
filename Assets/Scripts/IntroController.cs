using System.Collections;
using UnityEngine;

public class IntroController : MonoBehaviour
{
    public AudioSource introAudio;
    public GameManager gameManager;
    private bool isFirstRound = true;
    
    public void TriggerIntro()
    {
        StartCoroutine(PlayIntro());
    }

    /// <summary>
    /// Routine that plays a little intro jingle and pauses the game until the song has finished playing
    /// </summary>
    /// <returns></returns>
    private IEnumerator PlayIntro()
    {
        gameManager.SetGameStarted(false);
        
        Time.timeScale = 0f;
        
        if (gameManager.uiManager != null) {
            gameManager.uiManager.SetReadyVisible(true);
        }

        float waitDuration = 2f; 

        if (isFirstRound && introAudio != null && introAudio.clip != null)
        {
            introAudio.Play();
            waitDuration = introAudio.clip.length;
            isFirstRound = false;
        }

        yield return new WaitForSecondsRealtime(waitDuration);
        
        if (gameManager.uiManager != null) {
            gameManager.uiManager.SetReadyVisible(false);
        }

        Time.timeScale = SpeedController.gameSpeed;
        gameManager.enabled = true; 
        gameManager.SetGameStarted(true);
        gameManager.StartSpawningPacmen();
        
        foreach (var pacman in FindObjectsOfType<Pacman>())
        {
            pacman.enabled = true;
            pacman.canChomp = true;
        }
    }
}