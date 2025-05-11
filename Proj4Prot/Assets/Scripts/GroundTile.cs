using UnityEngine;

public class GroundTile : MonoBehaviour
{
    GroundSpawner groundSpawner;
    private bool hasSpawned = false;

    void Start()
    {
        groundSpawner = FindObjectOfType<GroundSpawner>();
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && !hasSpawned)
        {
            hasSpawned = true;
            groundSpawner.SpawnTile();
        }
    }
}
