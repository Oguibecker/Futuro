using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float forwardSpeed = 7f;
    public float laneDistance = 2.5f;
    public float jumpForce = 8f;
    public float gravity = 20f;
    public CharacterController controller;

    private int currentLane = 1; // 0 = esquerda, 1 = centro, 2 = direita
    private float verticalVelocity;
    private Vector3 moveDirection;

    void Start()
    {
        if (controller == null)
            controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Movimento para frente
        moveDirection = Vector3.forward * forwardSpeed;

        // Movimento lateral baseado na faixa
        Vector3 targetPosition = transform.position;
        targetPosition.x = (currentLane - 1) * laneDistance;

        Vector3 difference = targetPosition - transform.position;
        Vector3 lateralMove = new Vector3(difference.x * 10f, 0, 0);
        moveDirection.x = lateralMove.x;

        // Gravidade e pulo
        if (controller.isGrounded)
        {
            verticalVelocity = -1f;
            if (Input.GetKeyDown(KeyCode.Space))
                verticalVelocity = jumpForce;
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }

        moveDirection.y = verticalVelocity;

        // Aplica movimento
        controller.Move(moveDirection * Time.deltaTime);

        // Mudar de faixa
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            MoveLane(false);
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            MoveLane(true);
    }

    void MoveLane(bool right)
    {
        currentLane += right ? 1 : -1;
        currentLane = Mathf.Clamp(currentLane, 0, 2);
    }
}
