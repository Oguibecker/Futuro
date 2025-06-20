using UnityEngine;
using UnityEngine.SceneManagement; // Required for scene management functions
using System.Collections;

public class TriggerCutsceneNEW : MonoBehaviour
{
    [Header("The UI Element / Cutscene to show on collision")]
    public GameObject canvasElement1;
    public GameObject canvasElement2;
    public GameObject canvasElement3;
    public GameObject canvasElement4;

    [Header("UI Camera object, to manage cutscene filters")]
    public GameObject cameraUI;
    private CutsceneManager cutsceneManager;

    [Header("CameraPivot gameobject, to toggle rotation")]
    public GameObject cameraPivot;
    private SimpleRotateObject cameraRotator;

    [Header("The player, to manage entering and exiting cutscene")]
    public GameObject player;
    private PlayerController pController;

    public bool rotateCamera = true;
    public bool enableEndgameControls = false;
    public bool goFirstPerson = false;
    public bool turnOffMusic = false;
    private Coroutine showingCutscene;

    void Awake()
    {
        cutsceneManager = cameraUI.GetComponent<CutsceneManager>();
        pController = player.GetComponent<PlayerController>();
        cameraRotator = cameraPivot.GetComponent<SimpleRotateObject>();

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (showingCutscene != null) // Check if a coroutine is already running
            {
                StopCoroutine(showingCutscene);
            }
            showingCutscene = StartCoroutine(showCutscene());
        }
    }

    private IEnumerator showCutscene()
    {
        //turn all FX on
        cutsceneManager.glitchState = true;
        if (enableEndgameControls) {pController.laneMovementEnabled = 1; pController.forceMiddleLane = true;}
        if (goFirstPerson) {pController.firstPerson();}
        if (rotateCamera) {cameraRotator.enabled = true;}
        if (turnOffMusic) {pController.musicSource.volume = 0;}

        canvasElement1.SetActive(true);
        yield return new WaitForSeconds(2.5f);

        canvasElement1.SetActive(false);
        canvasElement2.SetActive(true);
        yield return new WaitForSeconds(2.5f);

        canvasElement2.SetActive(false);
        canvasElement3.SetActive(true);
        yield return new WaitForSeconds(2.5f);

        canvasElement3.SetActive(false);
        canvasElement4.SetActive(true);
        yield return new WaitForSeconds(2.5f);
        canvasElement4.SetActive(false);

        //turn all FX off, and set cam position back to 0.
        cutsceneManager.glitchState = false;
        if (rotateCamera) {cameraRotator.enabled = false;}
        cameraPivot.transform.rotation = new Quaternion(0,0,0,0);
    }
}