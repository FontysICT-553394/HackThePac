using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{
    public GameManager gameManager;
    
    [Header("Lives Display")]
    public GameObject lifePrefab;
    public Transform livesContainer;
    private List<GameObject> activeIcons = new List<GameObject>();

    [Header("Score Display")]
    public TextMeshProUGUI scoreText;
    public string scorePrefix = "SCORE: ";
    
    [Header("High Score Display")]
    public TextMeshProUGUI highScoreText;
    
    [Header("Status Display")]
    public TextMeshProUGUI readyText;

    /// <summary>
    /// Sets the visibility of the "READY?" text for when a game starts
    /// </summary>
    /// <param name="visible"></param>
    /// <returns></returns>
    public void SetReadyVisible(bool visible)
    {
        if (readyText != null)
        {
            readyText.gameObject.SetActive(visible);
            readyText.color = Color.yellow; 
            readyText.text = "READY?";
        }
    }

    /// <summary>
    /// Updates the current amount of little Pac-Men icons based off of the current amount of lives of the Pac-Men.
    /// </summary>
    /// <param name="currentLives"></param>
    /// <returns></returns>
    public void UpdateLivesDisplay(int currentLives)
    {
        foreach (GameObject icon in activeIcons) {
            Destroy(icon);
        }
        activeIcons.Clear();

        for (int i = 0; i < currentLives; i++)
        {
            GameObject newIcon = Instantiate(lifePrefab, livesContainer);
            activeIcons.Add(newIcon);
        }
    }

    /// <summary>
    /// Updates the text of the score whenever the score is increased by the Pac-Men.
    /// </summary>
    /// <param name="currentScore"></param>
    /// <returns></returns>
    public void UpdateScoreDisplay(int currentScore)
    {
        if (scoreText != null)
        {
            scoreText.text = scorePrefix + currentScore.ToString();
        }
    }

    /// <summary>
    /// Updates the text of the high score during the event, based off of the last high score and if a new high score is currently being set.
    /// </summary>
    /// <param name="highScore"></param>
    /// <returns></returns>
    public void UpdateHighScoreDisplay(int highScore)
    {
        if (highScoreText != null)
        {
            highScoreText.text = "HIGH SCORE: " + highScore.ToString("D5");
        }
    }
}