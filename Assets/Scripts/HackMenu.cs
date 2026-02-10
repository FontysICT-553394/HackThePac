// csharp
using UnityEngine;
using UnityEngine.UI;

public class HackMenu : MonoBehaviour
{
    public Slider ghostSpeedSlider;
    public Slider pacmanSpeedSlider;
    public Slider ghostCountSlider;
    public Slider powerPelletSlider;
    public Toggle noClipToggle;

    public GameObject MainMenuPanel;
    public GameObject HackPanel;
    
    void Start()
    {
        var s = GameSettings.Instance;
        ghostSpeedSlider.value = s.GhostSpeed;
        pacmanSpeedSlider.value = s.PacmanSpeed;
        ghostCountSlider.value = s.GhostCount;
        powerPelletSlider.value = s.PowerPelletCount;
        noClipToggle.isOn = s.PacmanNoClip;
    }
    
    public void ResetSettings()
    {
        GameSettings.Instance.GhostSpeed = 1f;
        GameSettings.Instance.PacmanSpeed = 1f;
        GameSettings.Instance.GhostCount = 4;
        GameSettings.Instance.PowerPelletCount = 4;
        GameSettings.Instance.PacmanNoClip = false;
        Start();
    }

    public void Back()
    {
        HackPanel.SetActive(false);
        MainMenuPanel.SetActive(true);
    }

    public void OnGhostSpeedChanged(float v)
    {
        GameSettings.Instance.GhostSpeed = v;
    }

    public void OnPacmanSpeedChanged(float v)
    {
        GameSettings.Instance.PacmanSpeed = v;
    }

    public void OnGhostCountChanged(float v)
    {
        GameSettings.Instance.GhostCount = Mathf.RoundToInt(v);
    }

    public void OnPowerPelletChanged(float v)
    {
        GameSettings.Instance.PowerPelletCount = Mathf.RoundToInt(v);
    }

    public void OnNoClipToggled(bool on)
    {
        GameSettings.Instance.PacmanNoClip = on;
    }
}