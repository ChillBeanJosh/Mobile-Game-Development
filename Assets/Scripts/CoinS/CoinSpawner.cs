using System.Collections.Generic;
using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject prefab;
    [Space]

    [Header("Spawn Area")]
    [SerializeField] private Transform bottomCorner;
    [SerializeField] private Transform topCorner;
    [Space]

    [Header("Spawn Settings")]
    [SerializeField] private int maxSpawned = 5;
    [SerializeField] private float spawnInterval = 2f;
    [Space]

    private readonly List<GameObject> spawnedObjects = new List<GameObject>();

    private float spawnTimer;

    private void Update()
    {
        CleanupDestroyedObjects();

        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            if (spawnedObjects.Count < maxSpawned) SpawnPrefab();       
        }
    }

    private void SpawnPrefab()
    {
        if (prefab == null || bottomCorner == null || topCorner == null)return;

        Vector3 spawnPosition = GetRandomSpawnPosition();

        GameObject spawnedObject = Instantiate(prefab, spawnPosition, Quaternion.identity);
        spawnedObjects.Add(spawnedObject);
    }

    private Vector3 GetRandomSpawnPosition()
    {
        //Set The Min and Max Values For The Spawn Area:
        Vector3 min = Vector3.Min(bottomCorner.position,topCorner.position);
        Vector3 max = Vector3.Max(bottomCorner.position,topCorner.position);

        //Generate a Random Position Within The Spawn Area:
        return new Vector3
        (
            Random.Range(min.x, max.x),
            Random.Range(min.y, max.y),
            Random.Range(min.z, max.z)
        );
    }

    private void CleanupDestroyedObjects()
    {
        //Remove any null references from the list of spawned objects
        for (int i = spawnedObjects.Count - 1; i >= 0; i--)
        {
            if (spawnedObjects[i] == null)
            {
                spawnedObjects.RemoveAt(i);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (bottomCorner == null || topCorner == null) return;

        Vector3 min = Vector3.Min(bottomCorner.position, topCorner.position);
        Vector3 max = Vector3.Max(bottomCorner.position,topCorner.position);

        Vector3 center = (min + max) * 0.5f;
        Vector3 size = max - min;

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(center, size);
    }
}
