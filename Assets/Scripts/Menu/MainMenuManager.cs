using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject characterSelectionMenu;
    
    [Header("Hacks")]
    [SerializeField] private GameObject hacksMenu;
    [SerializeField] private Slider pacmanSpeedSlider;
    [SerializeField] private Slider ghostSpeedSlider;
    [SerializeField] private Slider ghostAmountSlider;
    [SerializeField] private Slider powerPelletAmountSlider;
    [SerializeField] private Toggle noClipToggle;
    
    public void PlayGame(string character)
    {
        GameSettings.instance.selectedCharacter = character;
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        // SceneManager.LoadScene("Game - Ryan");
        SceneManager.LoadScene("Game - Ryan2");
    }
    
    public void ShowCharacterSelection() 
    {
        characterSelectionMenu.SetActive(true);
        hacksMenu.SetActive(false);
        mainMenu.SetActive(false);
    }
    
    public void ShowMainMenu()
    {
        mainMenu.SetActive(true);
        hacksMenu.SetActive(false);
        characterSelectionMenu.SetActive(false);
    }

    public void ShowHacksMenu()
    {
        hacksMenu.SetActive(true);
        mainMenu.SetActive(false);
        characterSelectionMenu.SetActive(false);
        SetSliders();
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void ResetHacks()
    {
        GameSettings.instance.pacmanSpeed = 3f;
        GameSettings.instance.ghostSpeed = 3f;
        SetSliders();
    }

    private void SetSliders()
    {
        pacmanSpeedSlider.value = GameSettings.instance.pacmanSpeed;
        ghostSpeedSlider.value = GameSettings.instance.ghostSpeed;
    }

    #region Hack UI Callbacks
    public void OnUpdatePacmanSpeed(float value) => GameSettings.instance.pacmanSpeed = value;
    public void OnUpdateGhostSpeed(float value) => GameSettings.instance.ghostSpeed = value;
    #endregion
}
