using UnityEngine;

public class Obstacle : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Game Over!");
            // Aqui você pode chamar a lógica de fim de jogo
        }
    }
}