using UnityEngine;

public class GroundSpawner : MonoBehaviour
{
    public GameObject[] groundTilePrefabs; // Array para armazenar os prefabs
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
        if (groundTilePrefabs.Length == 0)
        {
            Debug.LogError("N�o h� prefabs de ch�o atribu�dos no Inspector!");
            return;
        }

        // Escolher aleatoriamente um prefab
        int randomIndex = Random.Range(0, groundTilePrefabs.Length);
        GameObject selectedTile = groundTilePrefabs[randomIndex];

        // Instanciar o prefab selecionado na posi��o desejada
        GameObject temp = Instantiate(selectedTile, nextSpawnPoint, Quaternion.identity);

        // Atualiza o pr�ximo ponto de spawn
        nextSpawnPoint = temp.transform.Find("NextSpawnPoint").position;
    }
}
