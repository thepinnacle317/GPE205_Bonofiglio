using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject pickupPrefab;
    public float spawnDelay;
    private float nextSpawnTime;
    private GameObject spawnedPickup;

    public bool bRespawns;


    void Start()
    {
        if (bRespawns == false)
        {
            spawnedPickup = Instantiate(pickupPrefab, transform.position, Quaternion.identity) as GameObject;
        }
    }

    void Update()
    {
        if (bRespawns == true)
        {
            // If it is there then nothing will spawn
            if (spawnedPickup == null)
            {
                // Check if it is time to spawn
                if (Time.time > nextSpawnTime)
                {
                    // Spawn the pickup and set its next time to spawn
                    spawnedPickup = Instantiate(pickupPrefab, transform.position, Quaternion.identity) as GameObject;
                    nextSpawnTime = Time.time + spawnDelay;
                }
            }
            else
            {
                // If there is already a spawned pickup, postpone the spawn.
                nextSpawnTime = Time.time + spawnDelay;
            }
        }
    }
}
