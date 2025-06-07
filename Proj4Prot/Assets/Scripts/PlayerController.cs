using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float forwardSpeed;
    public float laneDistance = 2f;
    public float jumpForce = 8f;
    public float gravity = 20f;
    public CharacterController controller;
    public int keyCooldown;
    public int passedByCheckpoint = 0;

    public int maxLives = 3;
    private int currentLives;
    public Vector3 lastCheckpoint;
    public float currentCollectableNumber;

    public int currentLane = 1; // 0 = esquerda, 1 = centro, 2 = direita
    private Vector3 moveDirection;
    private float verticalVelocity;
    public int laneMovementEnabled = 0; //0 = no movement / 1 = movement

    private bool isRespawning = false;  

    public Camera mainCamera;
    public GameObject skybox;
    private SkyboxRotator skyboxRotator;
    public Text GasolineText;
    public Text BoosterText;
    public Text DeathText;

    // SOUND AND MUSIC
    public float volumeControl;
    public AudioSource musicSource;
    public AudioSource hurtSource;
    public AudioSource speedSource;
    public AudioSource collectSource;


    void Start()
    {
        
        if (controller == null)
            controller = GetComponent<CharacterController>();

        currentLives = maxLives;
        musicSource.volume = volumeControl;
        lastCheckpoint = transform.position;
        skyboxRotator = skybox.GetComponent<SkyboxRotator>();
    }

    void Update()
    {
        if (isRespawning) return; 

        moveDirection = Vector3.forward * forwardSpeed;

        Vector3 targetPosition = transform.position.z * Vector3.forward;
        if (currentLane == 0)
            targetPosition += Vector3.left * laneDistance;
        else if (currentLane == 2)
            targetPosition += Vector3.right * laneDistance;

        Vector3 difference = targetPosition - transform.position;
        Vector3 moveVector = new Vector3(difference.x * 10f, verticalVelocity, forwardSpeed);
        controller.Move(moveVector * Time.deltaTime);

        // lane movement
        currentLane = Mathf.Clamp(currentLane, 0, 2);
        if (laneMovementEnabled == 1){
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))  currentLane -= 1;
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) currentLane += 1;
            if (Input.GetKeyDown(KeyCode.Space) && keyCooldown == 0)
            {
                StartCoroutine(KeySpeedBoost());
                StartCoroutine(SpaceKeyCooldown());
            }
        }

        // pan audio to current lane
        if (currentLane == 0)
            musicSource.panStereo = -0.5f;
        if (currentLane == 1)
            musicSource.panStereo = 0;
        if (currentLane == 2)
            musicSource.panStereo = 0.5f;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Obstacle"))
        {
            TakeDamage();

            Collider playerCollider = hit.collider;
            if (playerCollider != null)
                StartCoroutine(TemporarilyDisableCollider(playerCollider));
        }
    }

    public void SpeedUp()
    {
        forwardSpeed = forwardSpeed * 1.2f;
        StartCoroutine(playSFX("speed"));
    }

    void TakeDamage()
    {
        currentLives--;
        Debug.Log("Vidas restantes: " + currentLives);
        if (currentLives <= 0)
        {
            StartCoroutine(Respawn());  
        } else {
            StartCoroutine(playSFX("hurt"));
        }
    }

    public void collectFuel()
    {
        string fullTextToType = "Gasoline: " + currentCollectableNumber;
        currentCollectableNumber += 1;

        StartCoroutine(playSFX("collect"));
        StartCoroutine(WriteText(fullTextToType,GasolineText));
    }

    public IEnumerator WriteText(string textToType, Text textBox)
    {
        textBox.text = "";

        for (int i = 0; i < textToType.Length; i++)
        {
            textBox.text += textToType[i];
            yield return new WaitForSeconds(0.02f);
        }
    }

    System.Collections.IEnumerator KeySpeedBoost()
    {
        float previousSpeed = forwardSpeed;
        float previousSkyboxRotation = skyboxRotator.rotationSpeedY;
        float fovTimer = 0f;
        float fovFXduration = 0.5f;

        forwardSpeed = forwardSpeed * 1.5f;
        skyboxRotator.rotationSpeedY = skyboxRotator.rotationSpeedY * 30f;
        Debug.Log("Boost Engaged = " + forwardSpeed);

        while (fovTimer < fovFXduration)
        {
            fovTimer += Time.deltaTime;
            mainCamera.fieldOfView = Mathf.Lerp(120, 140, fovTimer / fovFXduration);
            yield return null;
        }


        StartCoroutine(playSFX("speed"));

        yield return new WaitForSeconds(1f);


        fovTimer = 0f;
        while (fovTimer < fovFXduration)
        {
            fovTimer += Time.deltaTime;
            mainCamera.fieldOfView = Mathf.Lerp(140, 120, fovTimer / fovFXduration);
            yield return null;
        }
        forwardSpeed = previousSpeed;
        skyboxRotator.rotationSpeedY = previousSkyboxRotation;
    }

    System.Collections.IEnumerator SpaceKeyCooldown()
    {
        string DashBack = "Boosters Engaged";
        BoosterText.color = Color.blue;
        StartCoroutine(WriteText(DashBack,BoosterText));

        keyCooldown = 1;
        yield return new WaitForSeconds(3f);

        if (passedByCheckpoint == 1){keyCooldown = 0; yield break;}

        StartCoroutine(playSFX("collect"));
        keyCooldown = 0;

        DashBack = "Boosters Ready";
        BoosterText.color = Color.white;
        StartCoroutine(WriteText(DashBack,BoosterText));
    }

    public System.Collections.IEnumerator passedCheckpoint(int beforeLevel)
    {
        if (beforeLevel == 1 && isRespawning == false){
            passedByCheckpoint = 0;
            string DashBack = "Boosters Ready";
            BoosterText.color = Color.white;
            StartCoroutine(WriteText(DashBack,BoosterText));
        } else{
            passedByCheckpoint = 1;
            string DashBack = "Boosters Offline";
            BoosterText.color = Color.gray;
            StartCoroutine(WriteText(DashBack,BoosterText));
        }
        yield break;
    }

    System.Collections.IEnumerator playSFX(string type)
    {
        musicSource.volume = volumeControl / 5f;
        if (type == "speed"){
            speedSource.Play();
        } else if (type == "hurt"){
            hurtSource.Play();
        } else if (type == "collect"){
            collectSource.Play();
            collectSource.pitch = (1 + (currentCollectableNumber/10));
            musicSource.volume = volumeControl;
        }
        yield return new WaitForSeconds(0.25f);
        musicSource.volume = volumeControl;
    }

    System.Collections.IEnumerator TemporarilyDisableCollider(Collider col)
    {
        col.enabled = false;
        yield return new WaitForSeconds(3f);
        col.enabled = true;
    }

    System.Collections.IEnumerator Respawn()
    {
        isRespawning = true;  
        musicSource.pitch = -1f;

        string deathString = "Rebooting";
        StartCoroutine(WriteText(deathString,DeathText));
        
        yield return new WaitForSeconds(0.5f);

        transform.position = lastCheckpoint;
        currentLane = 1;
        currentLives = maxLives;

        yield return new WaitForSeconds(0.5f); 
        musicSource.pitch = 1f;
        isRespawning = false;

        deathString = "";
        StartCoroutine(WriteText(deathString,DeathText));
    }

    public int GetLives()
    {
        return currentLives;
    }
}
