using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField]
    private GameObject mainMenu;
    
    [SerializeField]
    private GameObject hacksMenu;

    public void ShowMainMenu()
    {
        mainMenu.SetActive(true);
        hacksMenu.SetActive(false);
    }

    public void ShowHacksMenu()
    {
        hacksMenu.SetActive(true);
        mainMenu.SetActive(false);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
