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

    //SKYBOX ROTATION, UI
    [Header("Camera, Skybox Rotation")]
    [Space]
    public Camera mainCamera;
    private CameraFollow cameraRotatorScript;
    public GameObject skybox;
    private SkyboxRotator skyboxRotator;
    public Vector3 defaultCamOffset;

    public DigitalGlitch glitchEffect;
    public float transitionDuration = 1.5f;
    public float glitchDesiredIntensity;

    [Header("UI")]
    [Space]
    public Text GasolineText;
    public Text BoosterText;
    public Text TimerText;
    public PostProcessVolume PPVolume;
    private Vignette damageVignette;
    public GameObject fuelGaugeArrow;
    private float currentFuelGaugeAngle;
    private Coroutine currentFuelGaugeCoroutine;
    public GameObject fuelBar;
    public GameObject fuelBarDisabled;

    public GameObject DeathText;
    private RectTransform deathTextRectTransform;


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

    public Vector2 targetTextLocation;


    void Start()
    {
        if (controller == null)
            controller = GetComponent<CharacterController>();
        
        cameraRotatorScript = mainCamera.gameObject.GetComponent<CameraFollow>();

        glitchEffect = mainCamera.gameObject.GetComponent<DigitalGlitch>();

        deathTextRectTransform = DeathText.GetComponent<RectTransform>();



        PPVolume.profile.TryGetSettings(out damageVignette);

        forwardSpeed = GlobalSpeed;

        currentLives = maxLives;
        musicSource.volume = volumeControl;
        lastCheckpoint = transform.position;
        skyboxRotator = skybox.GetComponent<SkyboxRotator>();
        targetTextLocation = new Vector2(0f, 950f);
        
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

    }

    private IEnumerator OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Obstacle"))
        {   
            isHurt = true;
            fuelBarDisabled.transform.localScale = new Vector3(1f,9f,1f);
            currentLives--;
            if (currentLives <= 0)
            {
                StartCoroutine(Respawn());  
            }
            heartbeat.Play();
            StartCoroutine(playSFX("hurt"));
            //StartCoroutine(damageSlowDown());
            damageVignette.active = true;

            Collider playerCollider = hit.collider;
            if (playerCollider != null){StartCoroutine(TemporarilyDisableCollider(playerCollider));}

            yield return new WaitForSeconds(1.5f);
            fuelBarDisabled.transform.localScale = new Vector3(1f,0f,1f);
            isHurt = false;


        }
    }


    public void collectFuel()
    {        
        StartCoroutine(playSFX("collect"));
        if (fuelAmount < 2.5) {fuelAmount += 1f;}
        //StartCoroutine(updateFuelGauge(1));
    }

    /*public IEnumerator updateFuelGauge(int gaugeMode)
    {
        previousCollectableNumber = currentCollectableNumber;
        if (gaugeMode == 1) // ON MODE 1 - INCREMENT THE ARROW ONE STEP
        {
            currentCollectableNumber += 1;
        }
        else if (gaugeMode == 0) // ON MODE 0 - DECREMENT THE ARROW ALL THE WAY
        {
            currentCollectableNumber = 0;
        }

        if (currentFuelGaugeCoroutine != null)
        {
            StopCoroutine(currentFuelGaugeCoroutine);
        }

        currentFuelGaugeAngle = Mathf.Clamp((currentCollectableNumber * -30f) + 90f, -85f, 85f);
        currentFuelGaugeCoroutine = StartCoroutine(SmoothlyRotateArrow(currentFuelGaugeAngle,gaugeMode));
        // ROTATE THE ARROW SMOOTHLY TO THE DESIRED ANGLE
        // IF MODE IS 1, ARROW GOES FAST. IF MODE IS 0, ARROW TAKES TIME EQUAL TO THE NUMBER OF FUEL.
        yield return currentFuelGaugeCoroutine;
    }

    private IEnumerator SmoothlyRotateArrow(float targetZAngle, int smoothMode)
    {
        float startZAngle = fuelGaugeArrow.transform.localEulerAngles.z;
        float elapsedTime = 0f;
        float smoothRotationTime = 0f;
        if (smoothMode == 1){smoothRotationTime = 0.2f;}
        else if (smoothMode == 0) {smoothRotationTime = previousCollectableNumber;}

        while (elapsedTime < smoothRotationTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / smoothRotationTime;
            float interpolatedZAngle = Mathf.LerpAngle(startZAngle, targetZAngle, t);
            fuelGaugeArrow.transform.localEulerAngles = new Vector3(0f, 0f, interpolatedZAngle);
            yield return null;
        }

        fuelGaugeArrow.transform.localEulerAngles = new Vector3(0f, 0f, targetZAngle);
    }*/

    public IEnumerator WriteText(string textToType, Text textBox)
    {
        textBox.text = "";

        for (int i = 0; i < textToType.Length; i++)
        {
            textBox.text += textToType[i];
            yield return new WaitForSeconds(0.02f);
        }
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
        forwardSpeed = forwardSpeed * 0.5f;

        keyCooldown = 1;
        BoosterText.color = Color.red;
        BoosterText.text = "-/-/-";

        yield return new WaitForSeconds(1.5f);

        forwardSpeed = GlobalSpeed;

        keyCooldown = 0;
        BoosterText.color = Color.white;
        BoosterText.text = "Boosters Ready";
    }

    /*
    System.Collections.IEnumerator KeySpeedBoost()
    {
        float previousSkyboxRotation = skyboxRotator.rotationSpeedY;
        float fovTimer = 0f;
        float fovFXduration = 0.5f;

        forwardSpeed = GlobalSpeed;
        forwardSpeed = forwardSpeed * 1.5f;

        skyboxRotator.rotationSpeedY = skyboxRotator.rotationSpeedY * 10f;

        while (fovTimer < fovFXduration)
        {
            fovTimer += Time.deltaTime;
            mainCamera.fieldOfView = Mathf.Lerp(120, 140, fovTimer / fovFXduration);
            yield return null;
        }

        yield return new WaitForSeconds(previousCollectableNumber);


        fovTimer = 0f;
        while (fovTimer < fovFXduration)
        {
            fovTimer += Time.deltaTime;
            mainCamera.fieldOfView = Mathf.Lerp(140, 120, fovTimer / fovFXduration);
            yield return null;
        }

        forwardSpeed = GlobalSpeed;
        skyboxRotator.rotationSpeedY = previousSkyboxRotation;
    }

    System.Collections.IEnumerator SpaceKeyCooldown()
    {
        string DashBack = "Boosters Engaged";
        BoosterText.color = Color.blue;
        StartCoroutine(WriteText(DashBack,BoosterText));

        keyCooldown = 1;
        StartCoroutine(playSFX("speed"));
        //StartCoroutine(updateFuelGauge(0));

        yield return new WaitForSeconds(previousCollectableNumber);

        if (passedByCheckpoint == 1){keyCooldown = 0; yield break;}

        //StartCoroutine(playSFX("collect"));
        keyCooldown = 0;

        DashBack = "Boosters Ready";
        BoosterText.color = Color.white;
        StartCoroutine(WriteText(DashBack,BoosterText));
    }*/

    public System.Collections.IEnumerator passedCheckpoint(int beforeLevel)
    {
        if (beforeLevel == 1 && isRespawning == false)
        { //checkpoint is before level
            passedByCheckpoint = 0;
            string DashBack = "Boosters Ready";
            BoosterText.color = Color.white;
            StartCoroutine(WriteText(DashBack,BoosterText));
            
            GlobalSpeed = 30f;
            forwardSpeed = GlobalSpeed;

            settingTimer = SetTimer(9f,true);
            StartCoroutine(settingTimer);
            StartCoroutine(playSFX("speed"));

        } else if (beforeLevel == 1 && isRespawning == true)
        {
            settingTimer = SetTimer(9f,true);
            StartCoroutine(settingTimer);

        } else if (beforeLevel == 0){ //checkpoint is before text
            passedByCheckpoint = 1;
            string DashBack = "Boosters Offline";
            BoosterText.color = Color.gray;
            StartCoroutine(WriteText(DashBack,BoosterText));
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
            StartCoroutine(Respawn());

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

    System.Collections.IEnumerator TemporarilyDisableCollider(Collider col)
    {
        col.enabled = false;
        yield return new WaitForSeconds(3f);
        col.enabled = true;
    }

    System.Collections.IEnumerator Respawn()
    {
        deathTextRectTransform.anchoredPosition = new Vector2(0f, 0f);
        isRespawning = true;  
        musicSource.pitch = -1f;
        StopCoroutine(settingTimer);
        StartCoroutine(resetFuel());

        yield return new WaitForSeconds(0.5f);

        transform.position = lastCheckpoint;
        currentLane = 1;
        currentLives = maxLives;
        damageVignette.active = false;
        heartbeat.Stop();

        yield return new WaitForSeconds(0.2f); 
        musicSource.pitch = 1f;
        isRespawning = false;

        deathTextRectTransform.anchoredPosition = new Vector2(0f, 1500f);

        //deathString = "";
        //StartCoroutine(WriteText(deathString,DeathText));
    }

    System.Collections.IEnumerator resetFuel()
    {
        GameObject[] fuelObjects = GameObject.FindGameObjectsWithTag("Fuel");

        foreach (GameObject fuelObject in fuelObjects)
        {

            fuelObject.SendMessage("ResetCollectables");

        }
        yield return null;
    }


#region CAMERA GIMMICKS

    public void firstPerson(bool FPActive)
    {        
        //cameraRotatorScript.SmoothRotateCamera(30f,1); new Vector3(0,1.5f,-3f);
        if (FPActive == false)  {cameraRotatorScript.offset = defaultCamOffset;}
        if (FPActive == true)   {cameraRotatorScript.offset = new Vector3(0,1f,0);}
    }

    public void camTurn180(bool C180Active)
    {        
        cameraRotatorScript.SmoothRotateCamera(180f,3);
        reversedKeys = !reversedKeys;
        if (C180Active == false)  {cameraRotatorScript.offset = defaultCamOffset;}
        else if (C180Active == true)   {cameraRotatorScript.offset = new Vector3(0,5f,-3f);}
    }

    public void camTopDown(bool TDActive)
    {        
        if (TDActive == false)
        {
            cameraRotatorScript.SmoothRotateCamera(-45f,1);
            cameraRotatorScript.offset = defaultCamOffset;
        }
        if (TDActive == true)
        {
            cameraRotatorScript.SmoothRotateCamera(45f,1);
            cameraRotatorScript.offset = new Vector3(0,20f,0f);
        }
        
    }

#endregion

}