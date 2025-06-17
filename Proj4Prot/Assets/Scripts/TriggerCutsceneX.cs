using UnityEngine;
using UnityEngine.SceneManagement; // Required for scene management functions
using System.Collections;

public class TriggerCutsceneX : MonoBehaviour
{
    [Header("The UI Element / Cutscene to show on collision")]
    public GameObject canvasElement;

    [Header("The mainMenu object, to manage pause state")]
    public GameObject mainMenu;
    private MainMenu menuManager;

    [Header("The player, to manage entering and exiting cutscene")]
    public GameObject player;
    private PlayerController pController;

    private bool showingCutscene;

    void Awake()
    {
        Collider triggerCollider = GetComponent<Collider>();
        menuManager = mainMenu.GetComponent<MainMenu>();
        pController = player.GetComponent<PlayerController>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(showCutscene(true)); // TURNS ON CUTSCENE ON COLLISION
        }
    }

    private void Update()
    {
        if (showingCutscene && Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(showCutscene(false)); // TURNS OFF CUTSCENE ON EXITING
        }
    }

    System.Collections.IEnumerator showCutscene(bool enableC)
    {
        if (enableC){

            menuManager.narrativeOn = true;
            showingCutscene = true;

            StartCoroutine(pController.triggeredCutscene(true));

            yield return new WaitForSeconds(1.5f);
            Time.timeScale = 0f;
            canvasElement.SetActive(true);

        } else if (!enableC){

            canvasElement.SetActive(false);
            Time.timeScale = 1f;
            yield return new WaitForSeconds(1.5f);

            StartCoroutine(pController.triggeredCutscene(false));

            menuManager.narrativeOn = false;
            showingCutscene = false;
        }
    }
}