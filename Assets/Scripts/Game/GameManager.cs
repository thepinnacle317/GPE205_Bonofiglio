using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Variable to access the game manager.
    public static GameManager instance;

    // Player Spawn Transform
    public Transform playerSpawnPoint;

    // List to hold player(s) in the game.
    public List<PlayerController> players;

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
    }

    void Start()
    {
       SpawnPlayer();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SpawnPlayer()
    {
        // Spawn the Player Controller at world origin.
        GameObject newPlayerObj = Instantiate(playerControllerPrefab, Vector3.zero, Quaternion.identity)
            as GameObject;

        // Spawn the pawn and assign it to the controller
        GameObject newPawnObj = Instantiate(tankPawnPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation)
            as GameObject;

        Controller newController = newPlayerObj.GetComponent<Controller>();
        Pawn newPawn = newPawnObj.GetComponent<Pawn>();

        // Assigns the pawn to the controller
        newController.pawn = newPawn;
    }

    /* Prefabs */
    public GameObject playerControllerPrefab;
    public GameObject tankPawnPrefab;
}
