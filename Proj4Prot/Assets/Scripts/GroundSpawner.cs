using UnityEngine;
using System.Collections.Generic;


public class GroundSpawner : MonoBehaviour
{
    public GameObject[] groundTilePrefabs;
    private Vector3 nextSpawnPoint;
    private Queue<GameObject> spawnedTiles = new Queue<GameObject>();

    public int maxTiles = 7;

    void Start()
    {
        for (int i = 0; i < maxTiles; i++)
        {
            SpawnTile();
        }
    }

    public void SpawnTile()
    {
        if (groundTilePrefabs.Length == 0) return;

        // Seleciona prefab aleatório
        int randomIndex = Random.Range(0, groundTilePrefabs.Length);
        GameObject selectedTile = groundTilePrefabs[randomIndex];

        // Instancia e posiciona
        GameObject newTile = Instantiate(selectedTile, nextSpawnPoint, Quaternion.identity);

        // Atualiza ponto de spawn
        nextSpawnPoint = newTile.transform.Find("NextSpawnPoint").position;

        // Armazena tile
        spawnedTiles.Enqueue(newTile);

        // Se passou do limite, destrói a mais antiga
        if (spawnedTiles.Count > maxTiles)
        {
            GameObject oldTile = spawnedTiles.Dequeue();
            Destroy(oldTile);
        }
    }
}
