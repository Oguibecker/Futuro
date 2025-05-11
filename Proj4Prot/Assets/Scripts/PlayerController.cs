using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float forwardSpeed = 7f;
    public float laneDistance = 2.5f;
    public float jumpForce = 8f;
    public float gravity = 20f;
    public CharacterController controller;

    private int currentLane = 1; // 0 = esquerda, 1 = centro, 2 = direita
    private Vector3 moveDirection;
    private float verticalVelocity;

    private float originalSpeed;
    private bool isSlowed = false;
    private float slowDuration = 1f;
    private float slowTimer = 0f;

    void Start()
    {
        if (controller == null)
            controller = GetComponent<CharacterController>();

        originalSpeed = forwardSpeed;
    }

    void Update()
    {
        moveDirection = Vector3.forward * forwardSpeed;

        if (controller.isGrounded)
        {
            verticalVelocity = -0.5f;
            if (Input.GetKeyDown(KeyCode.Space))
                verticalVelocity = jumpForce;
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }

        // Define posição-alvo com base na faixa atual
        Vector3 targetPosition = transform.position.z * Vector3.forward;
        if (currentLane == 0)
            targetPosition += Vector3.left * laneDistance;
        else if (currentLane == 2)
            targetPosition += Vector3.right * laneDistance;

        Vector3 difference = targetPosition - transform.position;
        Vector3 moveVector = new Vector3(difference.x * 10f, verticalVelocity, forwardSpeed);
        controller.Move(moveVector * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            MoveLane(false);
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            MoveLane(true);

        if (isSlowed)
        {
            slowTimer -= Time.deltaTime;
            if (slowTimer <= 0f)
            {
                forwardSpeed = originalSpeed;
                isSlowed = false;
            }
        }
    }

    void MoveLane(bool toRight)
    {
        currentLane += toRight ? 1 : -1;
        currentLane = Mathf.Clamp(currentLane, 0, 2);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Obstacle"))
        {
            if (!isSlowed)
            {
                forwardSpeed = originalSpeed / 2f;
                slowTimer = slowDuration;
                isSlowed = true;
            }

            // Muda de faixa automaticamente
            if (currentLane == 1) // No centro? Vai para a esquerda
                currentLane = 0;
            else // Se já estiver nas laterais, volta pro centro
                currentLane = 1;
        }
    }
}
