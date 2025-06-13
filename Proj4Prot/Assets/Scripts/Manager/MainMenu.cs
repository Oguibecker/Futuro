using UnityEngine;
using UnityEngine.SceneManagement;


public class MainMenu : MonoBehaviour
{
    [Header("Menus")]
    public GameObject panelMainMenu;
    public GameObject panelOptions;

    [Header("Configurações")]
    public bool isStartMenu = false; 

    private bool isPaused = false;

    private void Start()
    {
        if (isStartMenu)
        {
            
            Time.timeScale = 1f;
            ShowMainMenu();
        }
        else
        {
            panelMainMenu.SetActive(false);
            panelOptions.SetActive(false);
        }
    }

    private void Update()
    {
        if (!isStartMenu && Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
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
        if (isStartMenu)
        {
            SceneManager.LoadScene("Main"); 
        }
        else
        {
            ResumeGame();
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void PauseGame()
    {
        panelMainMenu.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    private void ResumeGame()
    {
        panelMainMenu.SetActive(false);
        panelOptions.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }
}
