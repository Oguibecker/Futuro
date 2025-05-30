using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float forwardSpeed = 15f;
    public float laneDistance = 2f;
    public float jumpForce = 8f;
    public float gravity = 20f;
    public CharacterController controller;
    public AudioSource musicSource;
    public AudioSource hurtSource;

    public int maxLives = 3;
    private int currentLives;
    private Vector3 lastCheckpoint;

    public int currentLane = 1; // 0 = esquerda, 1 = centro, 2 = direita
    private Vector3 moveDirection;
    private float verticalVelocity;
    public int laneMovementEnabled = 0; //0 = no movement / 1 = movement

    private bool isRespawning = false;  

    void Start()
    {
        
        if (controller == null)
            controller = GetComponent<CharacterController>();

        currentLives = maxLives;
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

    void TakeDamage()
    {
        currentLives--;
        Debug.Log("Vidas restantes: " + currentLives);
        if (currentLives <= 0)
        {
            StartCoroutine(Respawn());  
        } else {
            StartCoroutine(damageSound());
        }
    }

    System.Collections.IEnumerator damageSound()
    {
        musicSource.volume = 0.1f;
        hurtSource.Play();
        yield return new WaitForSeconds(0.25f);
        musicSource.volume = 0.5f;
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

        yield return new WaitForSeconds(0.2f); 
        musicSource.pitch = 1f;
        isRespawning = false;  
    }

    public void SetCheckpoint(Vector3 checkpoint)
    {
        lastCheckpoint = checkpoint;
    }

    public int GetLives()
    {
        return currentLives;
    }
}
