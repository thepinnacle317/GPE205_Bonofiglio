using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Variable to access the game manager.
    public static GameManager instance;

    // List to hold player(s) in the game.
    public List<PlayerController> players;

    // Holds a list of the AIControlles in the scene
    public List<AIController> aiControllers;

    public GameObject mapGeneratorPrefab;

    private AISpawners aiSpawner;

    private Transform aiSpawnPoint;


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

    // Update is called once per frame
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

        // Assign the AI Pawn to the controller
        aiController.pawn = newAIPawn;

        aiController.target = tankPawnPrefab;
    }

    private GameObject GetRandomAIType()
    {
        return aiControllerPrefab[Random.Range(0, aiControllerPrefab.Length)];
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
        return playerSpawners[Random.Range(0, playerSpawners.Length)];
    }

    private void SpawnMapGenerator()
    {
        GameObject newObj = Instantiate(mapGeneratorPrefab, Vector3.zero, Quaternion.identity);
    }

    /* Prefabs */
    public GameObject playerControllerPrefab;
    public GameObject[] aiControllerPrefab;
    public GameObject aiTankPrefab;
    public GameObject tankPawnPrefab;
}
