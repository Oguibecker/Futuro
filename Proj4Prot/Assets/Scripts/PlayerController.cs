using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float forwardSpeed = 5f;
    public float laneDistance = 2.5f; // Distância entre faixas (esquerda/centro/direita)
    public float jumpForce = 7f;
    public CharacterController controller;

    private int currentLane = 1; // 0 = esquerda, 1 = centro, 2 = direita
    private Vector3 moveDirection;
    private float verticalVelocity;
    private float gravity = 20f;

    void Start()
    {
        if (controller == null)
            controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Avança automaticamente
        moveDirection = Vector3.forward * forwardSpeed;

        // Detecta tecla A ou seta esquerda
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            MoveLane(false);
        }

        // Detecta tecla D ou seta direita
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            MoveLane(true);
        }

        // Detecta pulo
        if (controller.isGrounded)
        {
            verticalVelocity = -0.5f; // Mantém no chão
            if (Input.GetKeyDown(KeyCode.Space))
            {
                verticalVelocity = jumpForce;
            }
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }

        // Aplica direção vertical (pulo/gravity)
        moveDirection.y = verticalVelocity;

        // Move para a faixa (lane) correta
        Vector3 targetPosition = transform.position.z * Vector3.forward;
        if (currentLane == 0)
            targetPosition += Vector3.left * laneDistance;
        else if (currentLane == 2)
            targetPosition += Vector3.right * laneDistance;

        Vector3 moveTo = Vector3.Lerp(transform.position, targetPosition, 10 * Time.deltaTime);
        Vector3 moveVector = moveTo - transform.position;
        moveVector.y = moveDirection.y * Time.deltaTime;
        moveVector.z = moveDirection.z * Time.deltaTime;

        controller.Move(moveVector);
    }

    void MoveLane(bool goingRight)
    {
        currentLane += goingRight ? 1 : -1;
        currentLane = Mathf.Clamp(currentLane, 0, 2);
    }
}
