using UnityEngine;
using UnityEngine.SceneManagement;

public class SpeedController : MonoBehaviour
{
    public Transform[] options;
    public Pacman pacman;
    public string option = "Normal";
    public static float gameSpeed = 1f;

    public GameObject speedMenuPanel;
    public GameObject sliderAmountPanel;
    public SliderMenu sliderMenu;
    public SpriteRenderer indicator;
    public SpriteRenderer P1Icon;

    void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        Vector3 currentpos = this.pacman.transform.position;
        this.pacman.transform.position = new Vector3(currentpos.x, 4f, currentpos.z);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = gameSpeed;
    }

    private void Update()
    {
        if (this.pacman != null)
        {
            if (this.pacman.enabled)
            {
                if (this.pacman.movement != null)
                {
                    this.pacman.movement.enabled = false;
                }
                this.pacman.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                this.P1Icon.gameObject.SetActive(true);
                this.indicator.color = this.pacman.bodyRenderer.color;
                this.P1Icon.color = this.pacman.bodyRenderer.color;
            }
        }
    }

    /// <summary>
    /// Sets the currently selected speed for the game and starts the next menu.
    /// </summary>
    /// <returns></returns>
    private void SelectSpeedAndStart()
    {
        Time.timeScale = gameSpeed;
        Debug.Log("speed set to " + gameSpeed + "x");
        // show slider if it exists
        if (sliderAmountPanel != null)
        {
            sliderAmountPanel.SetActive(true);
            for (int i = 0; i < PlayerManager.addedPlayers.Length; i++)
            {
                if(PlayerManager.addedPlayers[i] != null)
                {
                    PlayerManager.addedPlayers[i].sliderMenu = sliderMenu;
                }
            }
            // give slider focus
            Debug.Log("Slider is now active!");
        }
        // hide speed menu
        if (speedMenuPanel != null) speedMenuPanel.SetActive(false);
        // don't load the scene here, slider handles it now
    }

    /// <summary>
    /// Sets speedMenu.
    /// </summary>
    /// <param name="player_id"></param>
    /// <returns></returns>
    public void menuSelect(int player_id)
    {
        if (player_id == PlayerManager.first_active_player)
        {
            switch (option)
            {
                case "Normal":
                    gameSpeed = 1f;
                    break;
                case "Fast":
                    gameSpeed = 1.5f;
                    break;
                case "Turbo":
                    gameSpeed = 2f;
                    break;
            }
            Time.timeScale = gameSpeed;
            this.pacman.movement.enabled = true;
            SelectSpeedAndStart();
        }
    }

    public void menuDirection(string dir, int player_id)
    {
        Vector3 currentpos = this.pacman.transform.position;
        Vector3 buttonpos = this.options[0].transform.position;
        if (player_id == PlayerManager.first_active_player)
        {
            PlayerManager.Instance.PlayMenuSound();
            switch (dir)
            {
                case "up":
                    switch (option)
                    {
                        case "Normal":
                            option = "Turbo";
                            buttonpos = this.options[2].transform.position;
                            break;
                        case "Fast":
                            option = "Normal";
                            buttonpos = this.options[0].transform.position;
                            break;
                        case "Turbo":
                            buttonpos = this.options[1].transform.position;
                            option = "Fast";
                            break;
                    }
                    break;
                case "down":
                    switch (option)
                    {
                        case "Normal":
                            option = "Fast";
                            buttonpos = this.options[1].transform.position;
                            break;
                        case "Fast":
                            option = "Turbo";
                            buttonpos = this.options[2].transform.position;
                            break;
                        case "Turbo":
                            option = "Normal";
                            buttonpos = this.options[0].transform.position;
                            break;
                    }
                    break;
            }
            this.pacman.transform.position = new Vector3(currentpos.x, buttonpos.y, currentpos.z);
        }
    }

    public void SetNormal()
    {
        gameSpeed = 1f;
        SelectSpeedAndStart();
    }
    public void SetFast()
    {
        gameSpeed = 1.5f;
        SelectSpeedAndStart();
    }
    public void SetTurbo()
    {
        gameSpeed = 2f;
        SelectSpeedAndStart();
    }
}