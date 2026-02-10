using UnityEngine;
using UnityEngine.SceneManagement;

public class SliderMenu : MonoBehaviour
{
    public Pacman pacman;
    public GameObject pacmanObject;
    public SliderController[] sliders;
    public GameObject sliderPanel;

    public SpriteRenderer P1Icon;
    public SpriteRenderer indicator;
    public SpriteRenderer readyOverlay;

    public Color overlayOriginal;
    bool choices_made_check = false;

    void Start()
    {
        Vector3 currentpos = this.pacman.transform.position;
        this.pacman.transform.position = new Vector3(-16f, 2f, currentpos.z);

        overlayOriginal = readyOverlay.color;
    }

    void Update()
    {
        if (gameObject.activeSelf)
        {
            if (this.pacman.movement != null) //Handling Pac-Man orientation, due to it using the same object (and thus movement script) from gameplay Pac-Man
            {
                this.pacman.movement.enabled = false;
            }
            this.pacman.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            this.indicator.color = this.pacman.bodyRenderer.color;
            this.P1Icon.gameObject.SetActive(true);
            this.P1Icon.color = this.pacman.bodyRenderer.color;

            if (sliders[0].isSelecting)
            {
                Vector3 currentpos = this.pacman.transform.position;
                this.pacman.transform.position = new Vector3(0f, 2f, currentpos.z);
                this.pacman.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                foreach (Transform child in this.pacman.transform)
                {
                    child.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                }
            }
            else
            {
                this.pacman.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                foreach (Transform child in this.pacman.transform)
                {
                    child.transform.localRotation = Quaternion.Euler(0f, 00f, 0f);
                }
            }

            if (choices_made_check) //Sets the brightness of the ready button panel based on if all sliders have been set or not
            {
                readyOverlay.color = overlayOriginal * 0.0f;
            }
            else
            {
                readyOverlay.color = overlayOriginal * 0.8f;
            }
        }
    }

    /// <summary>
    /// Moves the slider if the slider has been selected based off of the direction the joystick was pushed to
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public void moveSlider(int dir)
    {
        for (int i = 0; i < sliders.Length; i++)
        {
            if (!sliders[i].hasSelected && sliders[i].isSelecting)
            {
                sliders[i].moveSlider(dir);
                break;
            }
        }
    }

    /// <summary>
    /// Sets the entity amount to current value of the slider. If there is another slider, it will move there first. Otherwise it will move to the ready button and/or start the game
    /// </summary>
    /// <returns></returns>
    public void interact()
    {
        int totalSelected = 0;
        for(int i = 0; i < sliders.Length; i++)
        {
            if (sliders[i].hasSelected)
            {
                totalSelected++;
            }
            else
            {
                sliders[i].hasSelected = true;
                sliders[i].isSelecting = false;
                if (i + i < sliders.Length)
                {
                    Vector3 currentpos = this.pacman.transform.position;
                    this.pacman.transform.position = new Vector3(0f, 2f, currentpos.z);
                    sliders[i + 1].isSelecting = true;
                }
                else
                {
                    Vector3 currentpos = this.pacman.transform.position;
                    this.pacman.transform.position = new Vector3(-5f, -5f, currentpos.z);
                    this.choices_made_check = true;
                }
                    break;
            }
        }
        if(totalSelected == sliders.Length)
        {
            OnReadyPressed();
        }
    }

    /// <summary>
    /// Starts the game
    /// </summary>
    /// <returns></returns>
    private void OnReadyPressed()
    {
        if (SceneManager.GetActiveScene().name != "Classic")
        {
            SceneManager.LoadScene("Classic");
        }
    }
}
