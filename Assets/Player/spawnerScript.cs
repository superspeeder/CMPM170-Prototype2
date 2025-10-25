using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public int numberOfEnemies = 10;

    public float minX = -20f;
    public float maxX = 20f;
    public float minZ = -150f;
    public float maxZ = 150f;

    public float spawnHeightY = 1.5f;

    void Start()
    {
        SpawnEnemies();
    }

    void SpawnEnemies()
    {
        for (int i = 0; i < numberOfEnemies; i++)
        {
            float randomX = Random.Range(minX, maxX);
            float randomZ = Random.Range(minZ, maxZ);

            Vector3 spawnPosition = new Vector3(randomX, spawnHeightY, randomZ);

            Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        }
    }
}