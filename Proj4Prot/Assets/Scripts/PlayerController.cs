using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float forwardSpeed = 15f;
    public float laneDistance = 2f;
    public float jumpForce = 8f;
    public float gravity = 20f;
    public CharacterController controller;

    public int maxLives = 3;
    private int currentLives;
    public Vector3 lastCheckpoint;
    public float currentCollectableNumber;

    public int currentLane = 1; // 0 = esquerda, 1 = centro, 2 = direita
    private Vector3 moveDirection;
    private float verticalVelocity;
    public int laneMovementEnabled = 0; //0 = no movement / 1 = movement

    private bool isRespawning = false;  

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
        Debug.Log("Speeding Up - Speed = " + forwardSpeed);
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
        StartCoroutine(playSFX("collect"));
        currentCollectableNumber += 1;
        Debug.Log("Collectable = " + currentCollectableNumber);
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
            Debug.Log(collectSource.pitch);
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
        
        yield return new WaitForSeconds(0.5f);

        transform.position = lastCheckpoint;

        currentLives = maxLives;

        yield return new WaitForSeconds(0.5f); 
        musicSource.pitch = 1f;
        isRespawning = false;  
    }

    public int GetLives()
    {
        return currentLives;
    }
}
