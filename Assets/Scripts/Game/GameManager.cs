using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    // Variable to access the game manager.
    public static GameManager instance;

    // List to hold player(s) in the game.
    public List<PlayerController> players;

    // Holds a list of the AIControlles in the scene
    public List<AIController> aiControllers;

    // Bool used to determine if we are calling restart
    public bool restart = false;

    // Bool used to determine if the game mode is split screen or single player
    public bool bIsSinglePlayer = true;

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

    /* Game States */
    public enum GameStates {TitleScreen, MainMenu, Options, Leaderboard, LevelSelect, GamePlay, LevelWon, GameOver, Credits};
    public GameStates currentGameState;
    public GameObject TitleScreenStateObject;
    public GameObject MainMenuStateObject;
    public GameObject OptionsScreenStateObject;
    public GameObject LeaderboardStateObject;
    public GameObject LevelSelectStateObject;
    public GameObject GamePlayStateObject;
    public GameObject LevelWonStateObject;
    public GameObject GameOverStateObject;
    public GameObject CreditsStateObject;

    /* Scoring System */
    protected int highScore;
    protected string highScoreName;
    protected int playerOneScore;
    protected int playerTwoScore;


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

        // Spawn the map generator object in the current scene
        SpawnMapGenerator();
    }

    void Start()
    {
        currentGameState = GameStates.TitleScreen;
        ActivateTitleScreen();
    }

    void Update()
    {
        if (players.Count > 0)
        {
            if (players[0])
            {
                players[0].score += playerOneScore;
            }
            if (players.Count > 1 && players[1])
            {
                players[1].score += playerTwoScore;
            }
        }

        Debug.Log(restart);
    }

    public void SpawnPlayer()
    {
        // Spawn the Player Controller at world origin.
        GameObject newControllerObj = Instantiate(playerControllerPrefab, Vector3.zero, Quaternion.identity)
            as GameObject;

        // Spawn the pawn at a random position and assign it to the controller
        GameObject newPawnObj = Instantiate(tankPawnPrefab, GetRandomPlayerSpawnpoint().transform.position, GetRandomPlayerSpawnpoint().transform.rotation)
            as GameObject;

        newPawnObj.AddComponent<NoiseMaker>();

        // Set the controller component to the controller object.
        Controller newController = newControllerObj.GetComponent<Controller>();

        // Set the pawn component to the pawn object
        Pawn newPawn = newPawnObj.GetComponent<Pawn>();

        newPawn.noiseMaker = newPawnObj.GetComponent<NoiseMaker>();
        // Sets the noisemaker volume.
        newPawn.noiseMakerVolume = 5;

        // Assigns the pawn to the controller
        newController.pawn = newPawn;

        // Assigns the Controller to the Pawn
        newPawn.controller = newController;
    }

    public void RespawnPlayer()
    {
        Debug.Log("Respawning player");

        // Spawn the pawn at a random position and assign it to the controller
        GameObject newPawnObj = Instantiate(tankPawnPrefab, GetRandomPlayerSpawnpoint().transform.position, GetRandomPlayerSpawnpoint().transform.rotation);

        // Set the pawn component to the pawn object
        Pawn newPawn = newPawnObj.GetComponent<Pawn>();

        // Add the noismaker component to the pawn object
        newPawnObj.AddComponent<NoiseMaker>();

        // Set the noismaker variable for the pawn to the component just attached.
        newPawn.noiseMaker = newPawnObj.GetComponent<NoiseMaker>();

        // Sets the noisemaker volume.
        newPawn.noiseMakerVolume = 5;

        players[0].pawn = newPawn;
        newPawn.controller = players[0];
        Debug.Log("Assigning the new player the the existing controller");

        // TODO: Check each controller if the pawn value is null.  If so then set the new pawn to the controller and vice versa.
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

        // Assigns the Controller to the Pawn
        newAIPawn.controller = aiController;
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
        if (mapGeneratorPrefab != null)
        {
            GameObject newObj = Instantiate(mapGeneratorPrefab, Vector3.zero, Quaternion.identity);
            // Set the variables from the map generator to be controlled from the game manager.
            mapGeneratorPrefab.GetComponent<MapGeneration>().cols = gmCols;
            mapGeneratorPrefab.GetComponent<MapGeneration>().rows = gmRows;
            mapGeneratorPrefab.GetComponent<MapGeneration>().tileHeight = gmTileHeight;
            mapGeneratorPrefab.GetComponent<MapGeneration>().tileWidth = gmTileWidth;

            
        } 
    }

    public void RestartGame()
    {
        /*
        // Clean up the map
        foreach(var tile in mapGeneratorPrefab.GetComponent<MapGeneration>().gridPrefabs)
        {
            Destroy(tile);
        }
        Destroy(mapGeneratorPrefab);

        // Clear and remove the AI and controllers
        aiControllers.Clear();
        aiControllers.TrimExcess();
        

        // Clear and remove the players and controllers
        players.Clear();

        // Reset the players health
        foreach (var player in players)
        {
            player.currentLives = player.maxLives;
        }

        // Reset the players score
        foreach (var player in players)
        {
            player.score = 0;
        }

        // Regenerate the map
        SpawnMapGenerator();

        // Spawn Players and AI
        ActivateGamePlayState();
        */
        LevelSelectStateObject = GameObject.Find("LevelSelect");
        SceneManager.LoadSceneAsync("Main");
        ActivateLevelSelectScreen();
    }

    public void DoMainMenuState()
    {
        Debug.Log("Main Menu State Active");
        currentGameState = GameStates.MainMenu;
    }

    public void DoTitleScreenState()
    {
        Debug.Log("Title Screen State Active");
        currentGameState = GameStates.TitleScreen;
    }

    public void DoOptionsMenuState()
    {
        Debug.Log("Options Menu State Active");
        currentGameState = GameStates.Options;

        // TODO: Options for Music volume, SFX volume, map size slider from 2 - 5
    }

    public void DoLeaderboardState()
    {
        Debug.Log("Leaderboard State Active");
        currentGameState = GameStates.Leaderboard;

        // TODO: Add the players name and high score to the leaderboard list.
    }

    public void DoGameOverState()
    {
        Debug.Log("Game Over State Active");
        currentGameState = GameStates.GameOver;

        // TODO: If Main Menu: Clean up the map and all the AI and Players. Change State to main menu
        // If Retry: Regenerate the map and spawn everything back in using ActivateGamePlayState().
    }

    public void DoLevelWonState()
    {
        Debug.Log("Level Won State Active");
        currentGameState = GameStates.LevelWon;

        // TODO: Show players scores and achievements.
        // Remove the player HUD.
        // Disable tank movement
        // Display the players score in a panel.

        // Cool stats that could be displayed
        // Shots fired
        // Damage Done
        // Accuracy
        // Ask the player if they would like to play a new level or return to main menu.
    }

    public void DoLevelSelectState()
    {
        Debug.Log("Level Select State Active");
        currentGameState = GameStates.LevelSelect;
    }

    public void DoCreditsState()
    {
        Debug.Log("Credits State Active");
        currentGameState = GameStates.Credits;
    }

    public void DoGamePlayState()
    {
        Debug.Log("Game Play State Active");

        mapGeneratorPrefab.GetComponent<MapGeneration>().GenerateMap();
        SpawnPlayer();
        SpawnRandomEnemy();
    }

    /* Game State Transitions */
    private void DeactivateAll()
    {
        // Set all Game States to Inactive
        if (TitleScreenStateObject != null)
        {
            TitleScreenStateObject.SetActive(false);
        }
        else
        {
            Debug.Log("Title Screen Object is not set");
        }

        if (MainMenuStateObject != null) 
        {
            MainMenuStateObject.SetActive(false);
        }
        else
        {
            Debug.Log("Main Menu Object is not set");
        }

        if (OptionsScreenStateObject != null)
        {
            OptionsScreenStateObject.SetActive(false);
        }
        else
        {
            Debug.Log("Options Menu Object is not set");
        }

        if (LeaderboardStateObject != null)
        {
            LeaderboardStateObject.SetActive(false);
        }
        else
        {
            Debug.Log("Leaderboard Menu Object is not set");
        }

        if (LevelSelectStateObject != null)
        {
            LevelSelectStateObject.SetActive(false);
        }
        else
        {
            Debug.Log("Level Select Menu Object is not set");
        }

        if (GamePlayStateObject != null) 
        {
            GamePlayStateObject.SetActive(false);
        }
        else
        {
            Debug.Log("Game Play Object is not set");
        }

        if (GameOverStateObject != null)
        {
            GameOverStateObject.SetActive(false);
        }
        else
        {
            Debug.Log("Game Over Menu Object is not set");
        }

        if (CreditsStateObject != null)
        {
            CreditsStateObject.SetActive(false);
        }
        else
        {
            Debug.Log("Credits Screen Object is not set");
        }
    }

    public void ActivateTitleScreen()
    {
        // Set all states to inactive
        DeactivateAll();
        // Activate the associated state
        TitleScreenStateObject.SetActive(true);
        // Exectue Title Screen Logic
        DoTitleScreenState();
    }

    public void ActivateMainMenu()
    {
        // Set all states to inactive
        DeactivateAll();
        // Activate the associated state
        MainMenuStateObject.SetActive(true);
        // Execute Main Menu Logic
        DoMainMenuState();
    }

    public void ActivateOptionsMenu()
    {
        // Set all states to inactive
        DeactivateAll();
        // Activate the associated state
        OptionsScreenStateObject.SetActive(true);
        // Execute Options Menu Logic
        DoOptionsMenuState();
    }

    public void ActivateLeaderboard()
    {
        // Set all states to inactive
        DeactivateAll();
        // Activate the associated state
        LeaderboardStateObject.SetActive(true);
        // Execute Leaderboard Logic
        DoLeaderboardState();
    }

    public void ActivateLevelSelectScreen()
    {
        // Set all states to inactive
        DeactivateAll();
        // Activate the associated state
        LevelSelectStateObject.SetActive(true);
        // Execute Level Select Logic
        DoLevelSelectState();
    }

    public void ActivateGamePlayState()
    {
        // Set all states to inactive
        DeactivateAll();
        // Activate the associated state
        GamePlayStateObject.SetActive(true);
        // Execute Game Play Logic
        DoGamePlayState();

        restart = false;
    }

    public void ActivateLevelWonState()
    {
        DeactivateAll();
        LevelWonStateObject.SetActive(true);
    }

    public void ActivateGameOverScreen()
    {
        // Set all states to inactive
        DeactivateAll();
        // Activate the associated state
        GameOverStateObject.SetActive(true);
        // Execute Game Over Logic
        DoGameOverState();
    }

    public void ActivateCreditsScreen()
    {
        // Set all states to inactive
        DeactivateAll();
        // Activate the associated state
        CreditsStateObject.SetActive(true);
        // Execute Credits Logic
        DoCreditsState();
    }

    public void SetMapType(MapType type)
    {
        mapType = type;
        if (mapGeneratorPrefab != null)
        {
            mapGeneratorPrefab.GetComponent<MapGeneration>().mapSeed = gmMapSeed;

            mapGeneratorPrefab.GetComponent<MapGeneration>().currentMapGenerationMethod = (MapGeneration.GenerationMethod)mapType;
        }
    }

    /* Prefabs */
    public GameObject playerControllerPrefab;
    public GameObject[] aiControllerPrefab;
    public GameObject aiTankPrefab;
    public GameObject tankPawnPrefab;
}
