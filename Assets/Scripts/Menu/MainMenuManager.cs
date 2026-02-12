using System;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField]
    private GameObject mainMenu;
    
    [Header("Hacks")]
    [SerializeField] private GameObject hacksMenu;
    [SerializeField] private Slider pacmanSpeedSlider;
    [SerializeField] private Slider ghostSpeedSlider;
    [SerializeField] private Slider ghostAmountSlider;
    [SerializeField] private Slider powerPelletAmountSlider;
    [SerializeField] private Toggle noClipToggle;
    
    public void PlayGame()
    {
        return;
    }
    
    public void ShowMainMenu()
    {
        mainMenu.SetActive(true);
        hacksMenu.SetActive(false);
    }

    public void ShowHacksMenu()
    {
        hacksMenu.SetActive(true);
        mainMenu.SetActive(false);
        SetSliders();
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void ResetHacks()
    {
        GameSettings.instance.ghostAmount = 4;
        GameSettings.instance.powerPelletAmount = 4;
        GameSettings.instance.pacmanSpeed = 3f;
        GameSettings.instance.ghostSpeed = 3f;
        GameSettings.instance.noClipEnabled = false;
        SetSliders();
    }

    private void SetSliders()
    {
        pacmanSpeedSlider.value = GameSettings.instance.pacmanSpeed;
        ghostSpeedSlider.value = GameSettings.instance.ghostSpeed;
        ghostAmountSlider.value = GameSettings.instance.ghostAmount;
        powerPelletAmountSlider.value = GameSettings.instance.powerPelletAmount;
        noClipToggle.isOn = GameSettings.instance.noClipEnabled;
    }

    #region Hack UI Callbacks
    public void OnUpdatePacmanSpeed(float value) => GameSettings.instance.pacmanSpeed = value;
    public void OnUpdateGhostSpeed(float value) => GameSettings.instance.ghostSpeed = value;
    public void OnUpdateGhostAmount(float value) => GameSettings.instance.ghostAmount = Mathf.RoundToInt(value);
    public void OnUpdatePowerPelletAmount(float value) => GameSettings.instance.powerPelletAmount = Mathf.RoundToInt(value);
    public void OnUpdateNoClip(bool value) => GameSettings.instance.noClipEnabled = value;

    #endregion
}
