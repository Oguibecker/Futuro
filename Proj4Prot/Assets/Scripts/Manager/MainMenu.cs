using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject panelMainMenu;
    public GameObject panelOptions;

    private void Start()
    {
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        panelMainMenu.SetActive(true);
        panelOptions.SetActive(false);
    }

    public void ShowOptions()
    {
        panelMainMenu.SetActive(false);
        panelOptions.SetActive(true);
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("IntroNarrative");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}