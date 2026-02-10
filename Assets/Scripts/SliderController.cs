using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class SliderController : MonoBehaviour
{
    [Header("Slider Settings")]
    public Slider slider;
    public float step = 1f;
    [SerializeField] private TextMeshProUGUI sliderText;
    [SerializeField] private bool isPacmanSlider = true;

    [Header("Visuals")]
    [SerializeField] private Image background;
    [SerializeField] private float dimFactor = 0.5f;
    [SerializeField] private RectTransform selector;
    [SerializeField] private Vector3 selectorOffset = new Vector3(-60f, 0f, 0f);
    public SpriteRenderer Overlay;

    public bool hasSelected = false;
    public bool isSelecting = false;

    public static int PacmanAIAmount;
    public static int GhostAIAmount;
    public static int PacHumanAmount = 0;
    public static int GhostHumanAmount = 0;
    public Color overlayOriginal;

    private void Awake()
    {
        
    }

    private void Start()
    {
        slider.maxValue = 10;
        if (isPacmanSlider)
        {
            if(PacHumanAmount > 0)
            {
                slider.minValue = PacHumanAmount;
                slider.value = PacHumanAmount;
            }
            else
            {
                slider.minValue = 1;
                slider.value = 1;
            }
        }
        else
        {
            slider.minValue = 4;
            slider.value = 4;
        }

        overlayOriginal = Overlay.color;
        if (selector != null)
        {
            selector.position = slider.transform.position + selectorOffset;
        }

        if (isSelecting) //Change black overlay to change how bright the panel is. Is used to show which menu is currently being controlled
        {
            Overlay.color = overlayOriginal * 0.0f;
        }
        else
        {
            Overlay.color = overlayOriginal * 0.8f;
        }
    }

    private void Update()
    {
        if (this.gameObject.activeSelf)
        {
            if (isSelecting)
            {
                Overlay.color = overlayOriginal * 0.0f;
            }
            else
            {
                Overlay.color = overlayOriginal * 0.8f;
            }
            sliderText.text = (slider.value).ToString("0");
        }
    }

    /// <summary>
    /// Code that moves the selected slider left or right, based off of which direction is given.
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public void moveSlider(int dir)
    {
        int amount;

        if (isPacmanSlider)
        {
            amount = (int)slider.value - PacHumanAmount + dir;
            PacmanAIAmount = amount;
            slider.value = slider.value + dir;
        }
        else
        {
            GhostAIAmount = (int)slider.value - GhostHumanAmount + dir;
            slider.value = slider.value + dir;
        }
    }
}