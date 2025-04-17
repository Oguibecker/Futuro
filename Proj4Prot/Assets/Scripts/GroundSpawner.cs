using UnityEngine;

public class GroundSpawner : MonoBehaviour
{
    public GameObject groundTilePrefab;
    private Vector3 nextSpawnPoint;

    void Start()
    {
        for (int i = 0; i < 5; i++)
        {
            SpawnTile();
        }
    }

    public void SpawnTile()
    {
        GameObject temp = Instantiate(groundTilePrefab, nextSpawnPoint, Quaternion.identity);
        nextSpawnPoint = temp.transform.Find("NextSpawnPoint").position;
    }
}