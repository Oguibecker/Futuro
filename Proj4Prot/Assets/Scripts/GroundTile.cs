using UnityEngine;

public class GroundTile : MonoBehaviour
{
    GroundSpawner groundSpawner;

    void Start()
    {
        groundSpawner = GameObject.FindObjectOfType<GroundSpawner>();
        Invoke("SpawnAndDestroy", 1f); // Aguarda antes de executar
    }

    void SpawnAndDestroy()
    {
        if (groundSpawner != null)
        {
            groundSpawner.SpawnTile();
        }

        // Espera mais alguns segundos antes de destruir
        Destroy(gameObject, 4f); // Tempo total = 1s (invoke) + 4s = 5s
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Destroy(gameObject, 6f);
        }
    }

}
