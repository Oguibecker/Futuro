using UnityEngine;

public class Tile : MonoBehaviour
{
    GroundSpawner groundSpawner;

    void Start()
    {
        groundSpawner = GameObject.FindObjectOfType<GroundSpawner>();
        groundSpawner.SpawnTile();
        SpawnObstacle();
        Destroy(gameObject, 10f);
    }

    void SpawnObstacle()
    {
        if (Random.Range(0, 3) == 0)
        {
            int lane = Random.Range(0, 3);
            Vector3 spawnPos = transform.position + Vector3.forward * 4;
            if (lane == 0) spawnPos += Vector3.left * 3;
            else if (lane == 2) spawnPos += Vector3.right * 3;

            GameObject obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obstacle.transform.position = spawnPos + Vector3.up * 0.5f;
            obstacle.transform.localScale = new Vector3(1, 1, 1);
            obstacle.tag = "Obstacle";
            obstacle.AddComponent<BoxCollider>();
            obstacle.AddComponent<Rigidbody>().isKinematic = true;
        }
    }
}