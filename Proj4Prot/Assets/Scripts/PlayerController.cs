using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Rendering.PostProcessing;
using TMPro;
using Kino;

public class PlayerController : MonoBehaviour
{
    [Header("Game Variables")]
    [Space]
    private float GlobalSpeed = 20f;
    public float forwardSpeed;
    public float laneDistance = 2f;
    public CharacterController controller;
    public int keyCooldown;
    public int passedByCheckpoint = 0;
    public bool forceMiddleLane;


    [Header("Current GameState")]
    [Space]
    public int maxLives = 3;
    private int currentLives;
    public Vector3 lastCheckpoint;
    public float currentCollectableNumber;
    public int currentLane = 1; // 0 = esquerda, 1 = centro, 2 = direita
    public int laneMovementEnabled = 0; //0 = no movement / 1 = movement
    public float previousCollectableNumber;
    public bool gimmickStateTD;
    public bool gimmickState180;

    public bool isBoosting = false;

    private Vector3 moveDirection;
    private float verticalVelocity;
    private bool isRespawning = false;  
    private bool isHurt = false;  
    private IEnumerator settingTimer;
    public bool reversedKeys = false;
    public float fuelAmount = 0f;
    private bool veerCooldown;

    //SKYBOX ROTATION, UI
    [Header("Camera, Skybox Rotation")]
    [Space]
    public Camera mainCamera;
    public GameObject skybox;
    private SkyboxRotator skyboxRotator;
    public Vector3 defaultCamOffset;

    public DigitalGlitch glitchEffect;
    public float transitionDuration = 1.5f;
    public float glitchDesiredIntensity;

    [Header("UI")]
    [Space]
    public Text TimerText;
    public PostProcessVolume PPVolume;
    private Vignette damageVignette;
    public GameObject fuelGaugeArrow;
    private float currentFuelGaugeAngle;
    private Coroutine currentFuelGaugeCoroutine;
    public GameObject fuelBar;
    public GameObject fuelBarDisabled;

    public GameObject DeathTextTimer;
    public GameObject DeathTextObstacle;

    public GameObject cameraUI;
    private CutsceneManager cutsceneManager;


    // SOUND AND MUSIC
    [Header("Sound Control")]
    [Space]
    public float volumeControl;
    public AudioSource musicSource;
    public AudioSource hurtSource;
    public AudioSource speedSource;
    public AudioSource collectSource;
    public AudioSource heartbeat;
    public AudioSource corruptSource;
    public AudioSource mysteryMusicSource;


    void Start()
    {
        // GET COMPONENTS
        controller = GetComponent<CharacterController>();
        glitchEffect = mainCamera.gameObject.GetComponent<DigitalGlitch>();
        PPVolume.profile.TryGetSettings(out damageVignette);
        skyboxRotator = skybox.GetComponent<SkyboxRotator>();
        cutsceneManager = cameraUI.GetComponent<CutsceneManager>();

        // SET VARIABLES
        forwardSpeed = GlobalSpeed;
        currentLives = maxLives;
        musicSource.volume = volumeControl;
        lastCheckpoint = transform.position;
        
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
            if (reversedKeys == false){
                if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))  currentLane -= 1;
                if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) currentLane += 1;
            } else if (reversedKeys == true){
                if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))  currentLane += 1;
                if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) currentLane -= 1;
            }

            if (Input.GetKey(KeyCode.Space)){isBoosting = true;}
            else {isBoosting = false;}

            //If the player has fuel, increase their speed by 2x, and decrement their fuel for the time they are pressing the button.
            if (fuelAmount > 0 && isBoosting && !isRespawning && !isHurt){
                forwardSpeed = GlobalSpeed * 2f;
                fuelAmount -= Time.deltaTime;
                skyboxRotator.rotationSpeedY = forwardSpeed/2f;
            } else {forwardSpeed = GlobalSpeed;}

            if (fuelAmount < 0){fuelAmount = 0; forwardSpeed = GlobalSpeed;}
        }

        Vector3 targetFuelScale = new Vector3(1f, fuelAmount*3, 1f);
        fuelBar.transform.localScale = Vector3.Lerp(fuelBar.transform.localScale, targetFuelScale, Time.deltaTime * 6);


        // pan audio to current lane
        if (currentLane == 0)
            musicSource.panStereo = -0.2f;
        if (currentLane == 1)
            musicSource.panStereo = 0;
        if (currentLane == 2)
            musicSource.panStereo = 0.2f;

        if (currentLane != 1 && forceMiddleLane && !veerCooldown)
        {
            StartCoroutine(veerMiddle());
        }
    }

    public IEnumerator veerMiddle()
    {
        veerCooldown = true;
        yield return new WaitForSeconds(0.5f);
        currentLane = 1;
        veerCooldown = false;
    }


    public void collectFuel()
    {        
        StartCoroutine(playSFX("collect"));
        if (fuelAmount < 2.5) {fuelAmount += 1f;}
    }

    public IEnumerator triggeredCutscene(bool playerEnableCutscene)
    {
        if (playerEnableCutscene == true){

            glitchEffect.SetIntensitySmoothly(0.7f, transitionDuration);
            StartCoroutine(playSFX("corrupt"));

            while (forwardSpeed > (GlobalSpeed/3)){
                forwardSpeed -= 1;
                yield return new WaitForSeconds(0.1f);
            }
            musicSource.volume = 0;
            mysteryMusicSource.volume = volumeControl;


        } else if (playerEnableCutscene == false){
            
            glitchEffect.SetIntensitySmoothly(0, transitionDuration);
            StartCoroutine(playSFX("corrupt"));

            while (forwardSpeed < GlobalSpeed){
                forwardSpeed += 1;
                yield return new WaitForSeconds(0.1f);
            }
            forwardSpeed = GlobalSpeed;
            mysteryMusicSource.volume = 0;
            musicSource.volume = volumeControl;
            
        }
        yield return null;
    }

    System.Collections.IEnumerator damageSlowDown()
    {
        forwardSpeed = GlobalSpeed;
        forwardSpeed = forwardSpeed * 0.25f;

        yield return new WaitForSeconds(1.5f);

        forwardSpeed = GlobalSpeed;
    }


    public System.Collections.IEnumerator passedCheckpoint(int beforeLevel)
    {
        if (beforeLevel == 1 && isRespawning == false)
        { //checkpoint is before level
            passedByCheckpoint = 0;
            
            GlobalSpeed = 30f;
            forwardSpeed = GlobalSpeed;

            settingTimer = SetTimer(9f,true);
            StartCoroutine(settingTimer);
            //StartCoroutine(playSFX("speed"));

        } else if (beforeLevel == 1 && isRespawning == true)
        {
            settingTimer = SetTimer(9f,true);
            StartCoroutine(settingTimer);

        } else if (beforeLevel == 0){ //checkpoint is before text
            
            passedByCheckpoint = 1;
            currentLives = maxLives;
            damageVignette.active = false;
            heartbeat.Stop();

            GlobalSpeed = 20f;
            forwardSpeed = GlobalSpeed;
            StopCoroutine(settingTimer);
            StartCoroutine(SetTimer(0f,false));
        }

        fuelAmount = 0f;
        yield break;
    }

    public IEnumerator SetTimer(float clockTime, bool enableClock)
    {
        if (!enableClock)
        {
            for (int i = 0; i < 4; i++) // Flash 4
            {
                TimerText.color = Color.green;
                yield return new WaitForSeconds(0.3f);
                TimerText.color = new Color(255f,131f,0);
                yield return new WaitForSeconds(0.3f);
            }

            yield return new WaitForSeconds(2f);
            TimerText.color = Color.gray;
            TimerText.text = "--:--";
        }
        else // enableClock is true
        {
            float currentClockTime = clockTime;

            while (currentClockTime > 0)
            {
                currentClockTime -= Time.deltaTime;

                if (currentClockTime < 0)
                {
                    currentClockTime = 0;
                }

                int seconds = Mathf.FloorToInt(currentClockTime);
                int milliseconds = Mathf.FloorToInt((currentClockTime - seconds) * 100);

                string formattedSeconds = seconds.ToString("D2");
                string formattedMilliseconds = milliseconds.ToString("D2");

                if (currentClockTime <= 5f)
                {
                    TimerText.color = Color.red;
                    
                }
                else
                {
                    TimerText.color = new Color(255f,131f,0);
                }

                TimerText.text = $"{formattedSeconds}:{formattedMilliseconds}";

                yield return null;
            }

            TimerText.text = "00:00";
            StartCoroutine(Respawn(true));

        }
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
        }  else if (type == "corrupt"){
            corruptSource.Play();
        }
        
        yield return new WaitForSeconds(0.25f);
        musicSource.volume = volumeControl;
    }

    



#region DAMAGE & DEATH

    public void takeDamage()
    {
        if (!isHurt){StartCoroutine(tookDamage());}
    }

    public IEnumerator tookDamage()
    {
        isHurt = true;
        fuelBarDisabled.SetActive(true);

        currentLives--;
        if (currentLives <= 0)
        {
            StartCoroutine(Respawn(false));  
        }

        heartbeat.Play();
        StartCoroutine(playSFX("hurt"));
        StartCoroutine(damageSlowDown());
        damageVignette.active = true;

        yield return new WaitForSeconds(1.5f);

        fuelBarDisabled.SetActive(false);
        isHurt = false;
    }


    private IEnumerator Respawn(bool deathType) // TRUE FOR TIMER. FALSE FOR OBSTACLE
    {
        isRespawning = true;
        cutsceneManager.glitchState = true;

        if (deathType){DeathTextTimer.SetActive(true);}
        if (!deathType){DeathTextObstacle.SetActive(true);}

        musicSource.pitch = -1f;

        StopCoroutine(settingTimer);
        StartCoroutine(resetFuel());

        transform.position = lastCheckpoint;
        currentLane = 1;
        currentLives = maxLives;

        yield return new WaitForSeconds(1f);

        damageVignette.active = false;
        heartbeat.Stop();
        musicSource.pitch = 1f;

        DeathTextObstacle.SetActive(false);
        DeathTextTimer.SetActive(false);
        cutsceneManager.glitchState = false;
        
        isRespawning = false;
    }
    
    private IEnumerator resetFuel()
    {
        GameObject[] fuelObjects = GameObject.FindGameObjectsWithTag("Fuel");
        foreach (GameObject fuelObject in fuelObjects)
        {
            fuelObject.SendMessage("ResetCollectables");
        }
        yield return null;
    }

#endregion


#region CAMERA GIMMICKS

    public void firstPerson()
    {        
        StartCoroutine(moveCameraToFirstPerson());
        //if (FPActive == false)  {cameraRotatorScript.offset = defaultCamOffset;}
        //if (FPActive == true)   {cameraRotatorScript.offset = new Vector3(0,1f,0);}
    }

    public IEnumerator moveCameraToFirstPerson()
    {
        mainCamera.transform.localEulerAngles = new Vector3(0,0,0);
        float timeElapsed = 0f;
        while (timeElapsed < 1)
        {
            float t = timeElapsed / 1;
            mainCamera.transform.localPosition = Vector3.Lerp(mainCamera.transform.localPosition,new Vector3(0,0,-0.45f),t);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        Camera.main.transform.localPosition = new Vector3(0,0,-0.45f);

    }

    public void camTurn180(bool C180Active)
    {        
        //cameraRotatorScript.SmoothRotateCamera(180f,3);
        if(C180Active){mainCamera.transform.localEulerAngles = new Vector3(15f,0,180f); reversedKeys = true;}
        if(!C180Active){mainCamera.transform.localEulerAngles = new Vector3(15f,0,0f); reversedKeys = false;}
        
    }

    public void camTopDown(bool TDActive)
    {        
        if (TDActive == false)
        {
            mainCamera.transform.localPosition = new Vector3(0,2f,-5.5f);
            mainCamera.transform.localEulerAngles = new Vector3(15f,0,0);
        }
        if (TDActive == true)
        {
            mainCamera.transform.localPosition = new Vector3(0,15f,-2f);
            mainCamera.transform.localEulerAngles = new Vector3(60f,0,0);
        }
        
    }

#endregion

}