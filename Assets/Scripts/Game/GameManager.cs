using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    // Variable to access the game manager.
    public static GameManager instance;

    // List to hold player(s) in the game.
    public List<PlayerController> players;

    // Holds a list of the AIControlles in the scene
    public List<AIController> aiControllers;

    // Map Generator Prefab variable.  Must be set!!!
    public GameObject mapGeneratorPrefab;

    // AI spawn point transform.  See spawnRandomEnemy.
    private Transform aiSpawnPoint;

    // Map Generation Variables
    public int gmRows;
    public int gmCols;
    public float gmTileWidth = 50f;
    public float gmTileHeight = 50f;
    public enum MapType { MapOfTheDay, MapSeed, RandomTimeGeneration };
    public MapType mapType;
    public int gmMapSeed;


    // Called when the object is first created.
    private void Awake()
    {
        // Check if the instance exists or not yet.
        if (instance == null)
        {
            instance = this;
            // Don't destroy the GameManager if we load a new scene
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // If there is already an instance than destroy this one.
            Destroy(gameObject);
        }
        SpawnMapGenerator();
        mapGeneratorPrefab.GetComponent<MapGeneration>().GenerateMap();
    }

    void Start()
    {
       SpawnPlayer();
        SpawnRandomEnemy();
    }

    void Update()
    {
        
    }

    private void SpawnPlayer()
    {
        // Spawn the Player Controller at world origin.
        GameObject newPlayerObj = Instantiate(playerControllerPrefab, Vector3.zero, Quaternion.identity)
            as GameObject;

        // Spawn the pawn at a random position and assign it to the controller
        GameObject newPawnObj = Instantiate(tankPawnPrefab, GetRandomPlayerSpawnpoint().transform.position, GetRandomPlayerSpawnpoint().transform.rotation)
            as GameObject;

        newPawnObj.AddComponent<NoiseMaker>();

        Controller newController = newPlayerObj.GetComponent<Controller>();
        Pawn newPawn = newPawnObj.GetComponent<Pawn>();

        newPawn.noiseMaker = newPawnObj.GetComponent<NoiseMaker>();
        // Sets the noisemaker volume.
        newPawn.noiseMakerVolume = 5;

        // Assigns the pawn to the controller
        newController.pawn = newPawn;
    }

    private void SpawnAITanks()
    {
        // Spawn a random AI Controller
        GameObject newAIController = Instantiate(GetRandomAIType(), Vector3.zero, Quaternion.identity);

        // Spawn the AI Tank at a random spawn point in the map tiles
        GameObject newAITank = Instantiate(aiTankPrefab, aiSpawnPoint.position, aiSpawnPoint.rotation);

        AIController aiController = newAIController.GetComponent<AIController>();
        Pawn newAIPawn = newAITank.GetComponent<Pawn>();

        // Get all the patrol points(Prefab) in the world and then add them to the waypoints array(Transform)
        PatrolPoint[] patrolPoints = FindObjectsOfType<PatrolPoint>();
        aiController.patrolPoints = patrolPoints;

        // Assign the AI Pawn to the controller
        aiController.pawn = newAIPawn;
    }

    private GameObject GetRandomAIType()
    {
        return aiControllerPrefab[UnityEngine.Random.Range(0, aiControllerPrefab.Length)];
    }

    private void SpawnRandomEnemy()
    {
        AISpawners[] aiSpawners = FindObjectsOfType<AISpawners>();
        foreach (var aiSpawner in aiSpawners)
        {
            aiSpawnPoint = aiSpawner.transform;
            SpawnAITanks();
        }
    }

    private PawnSpawnPoint GetRandomPlayerSpawnpoint()
    {
        PawnSpawnPoint[] playerSpawners = FindObjectsOfType<PawnSpawnPoint>();
        return playerSpawners[UnityEngine.Random.Range(0, playerSpawners.Length)];
    }

    private void SpawnMapGenerator()
    {
        GameObject newObj = Instantiate(mapGeneratorPrefab, Vector3.zero, Quaternion.identity);

        // Set the variables from the map generator to be controlled from the game manager.
        mapGeneratorPrefab.GetComponent<MapGeneration>().cols = gmCols;
        mapGeneratorPrefab.GetComponent<MapGeneration>().rows = gmRows;
        mapGeneratorPrefab.GetComponent<MapGeneration>().tileHeight = gmTileHeight;
        mapGeneratorPrefab.GetComponent<MapGeneration>().tileWidth = gmTileWidth;
        mapGeneratorPrefab.GetComponent<MapGeneration>().mapSeed = gmMapSeed;

        mapGeneratorPrefab.GetComponent<MapGeneration>().currentMapGenerationMethod = (MapGeneration.GenerationMethod)mapType;
    }

    /* Prefabs */
    public GameObject playerControllerPrefab;
    public GameObject[] aiControllerPrefab;
    public GameObject aiTankPrefab;
    public GameObject tankPawnPrefab;
}
