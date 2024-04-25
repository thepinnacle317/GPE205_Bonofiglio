using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.Audio;

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
    public bool bIsSinglePlayer;

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
    public int playerOneScore;
    public int playerTwoScore;

    /* Audio */
    public AudioMixer audioMixer;
    private AudioSource audioSource;
    public AudioClip mainMenuMusic;
    public AudioClip gameplayMusic;

    /* Cameras */
    public Camera player1Camera;
    public Camera mainCamera;
    public GameObject mainCameraPrefab;

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

        // Set the UI Game Objects
        SetUIObjects();

        // Get the Audio Source Component
        audioSource = GetComponent<AudioSource>();

        //Set the clip to be played
        audioSource.clip = mainMenuMusic;
    }

    void Start()
    {
        currentGameState = GameStates.TitleScreen;
        ActivateTitleScreen();
       
        // Play the Main Music Source
        audioSource.Play();

        Vector3 cameraSpawnLoc = new Vector3(0f, 0f, -391);

        GameObject newMainCamera = Instantiate(mainCameraPrefab, cameraSpawnLoc, Quaternion.identity);
        DontDestroyOnLoad(newMainCamera);

        // Find the main camera at startup
        mainCamera = FindObjectOfType<Camera>();

        // Keep the reference to the camera component for the menu system.
        DontDestroyOnLoad(mainCamera);
    }

    void Update()
    {
        if (players.Count > 0)
        {
            if (players[0])
            {
                playerOneScore = players[0].score;
            }
            if (players.Count > 1 && players[1])
            {
                playerTwoScore = players[1].score;
            }
        }
    }

    private void LateUpdate()
    {
        if (currentGameState == GameStates.GamePlay)
        {
            if (playerOneScore >= (aiControllers.Count * 15) + (aiControllers.Count * 10) - 30 || playerTwoScore >= (aiControllers.Count * 15) + (aiControllers.Count * 10) - 30)
            {
                ActivateLevelWonState();
            }           
        }   
    }

    public void SpawnPlayers()
    {
        // Spawn the Player Controller at world origin.
        GameObject newControllerObj1 = Instantiate(player1ControllerPrefab, Vector3.zero, Quaternion.identity)
            as GameObject;

        // Spawn the pawn at a random position and assign it to the controller
        GameObject newPawnObj1 = Instantiate(tank1PawnPrefab, GetRandomPlayerSpawnpoint().transform.position, GetRandomPlayerSpawnpoint().transform.rotation)
            as GameObject;

            newPawnObj1.AddComponent<NoiseMaker>();

        // Set the controller component to the controller object.
        Controller newController1 = newControllerObj1.GetComponent<Controller>();

        // Set the pawn component to the pawn object
        Pawn newPawn1 = newPawnObj1.GetComponent<Pawn>();

        newPawn1.noiseMaker = newPawnObj1.GetComponent<NoiseMaker>();
        // Sets the noisemaker volume.
        newPawn1.noiseMakerVolume = 5;

        // Assigns the pawn to the controller
        newController1.pawn = newPawn1;

        // Assigns the Controller to the Pawn
        newPawn1.controller = newController1;

        // Set the Player One camera to for split-screen
        if (bIsSinglePlayer == false)
        {         
            SpawnPlayer2();
            player1Camera = newPawnObj1.GetComponent<TankPawn>().playerCamera;
            player1Camera.rect = new Rect(0, 0, .5f, 1f);
        }
    }

    public void SpawnPlayer2()
    {
        // Spawn the Player Controller at world origin.
        GameObject newControllerObj2 = Instantiate(player2ControllerPrefab, Vector3.zero, Quaternion.identity)
            as GameObject;

        // Spawn the pawn at a random position and assign it to the controller
        GameObject newPawnObj2 = Instantiate(tank2PawnPrefab, GetRandomPlayerSpawnpoint().transform.position, GetRandomPlayerSpawnpoint().transform.rotation)
            as GameObject;

        newPawnObj2.AddComponent<NoiseMaker>();

        // Set the controller component to the controller object.
        Controller newController2 = newControllerObj2.GetComponent<Controller>();

        // Set the pawn component to the pawn object
        Pawn newPawn2 = newPawnObj2.GetComponent<Pawn>();

        newPawn2.noiseMaker = newPawnObj2.GetComponent<NoiseMaker>();
        // Sets the noisemaker volume.
        newPawn2.noiseMakerVolume = 5;

        // Assigns the pawn to the controller
        newController2.pawn = newPawn2;

        // Assigns the Controller to the Pawn
        newPawn2.controller = newController2;
    }

    public void RespawnPlayer()
    {
        Debug.Log("Respawning player");

        // Spawn the pawn at a random position and assign it to the controller
        GameObject newPawnObj = Instantiate(tank1PawnPrefab, GetRandomPlayerSpawnpoint().transform.position, GetRandomPlayerSpawnpoint().transform.rotation);

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
        SceneManager.LoadSceneAsync("Main");
        mainCamera.gameObject.SetActive(true);

        ActivateLevelSelectScreen();

    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadSceneAsync("Main");
        mainCamera.gameObject.SetActive(true);
        ActivateMainMenu();
    }

    public void DoMainMenuState()
    {
        Debug.Log("Main Menu State Active");
        currentGameState = GameStates.MainMenu;
        mainCamera.gameObject.SetActive(true);
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
        mainCamera.gameObject.SetActive(true);
    }

    public void DoLevelWonState()
    {
        Debug.Log("Level Won State Active");
        currentGameState = GameStates.LevelWon;
        mainCamera.gameObject.SetActive(true);


        // TODO: Show players scores and achievements.
        // Remove the player HUD.
        // Disable tank movement
        // Display the players score in a panel.

        // Cool stats that could be displayed
        // Shots fired
        // Damage Done
        // Accuracy
    }

    public void DoLevelSelectState()
    {
        Debug.Log("Level Select State Active");
        //mainCamera.gameObject.SetActive(true);
        currentGameState = GameStates.LevelSelect;
    }

    public void DoCreditsState()
    {
        Debug.Log("Credits State Active");
        mainCamera.gameObject.SetActive(true);
        currentGameState = GameStates.Credits;
    }

    public void DoGamePlayState()
    {
        Debug.Log("Game Play State Active");
        currentGameState = GameStates.GamePlay;

        // Background Audio
        audioSource.Stop();
        audioSource.clip = gameplayMusic;
        audioSource.Play();

        // Disable the main menu camera
        mainCamera.gameObject.SetActive(false);

        // Handle map generation and spawning in pawns
        mapGeneratorPrefab.GetComponent<MapGeneration>().GenerateMap();
        SpawnPlayers();
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

        if (LevelWonStateObject != null)
        {
            LevelWonStateObject.SetActive(false);
        }
        else
        {
            Debug.Log("Level Won Object is not set");
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

    public void SetUIObjects()
    {
        TitleScreenStateObject = Instantiate(TitleScreenStateObject, transform);
        MainMenuStateObject = Instantiate(MainMenuStateObject, transform);
        OptionsScreenStateObject = Instantiate(OptionsScreenStateObject, transform);
        LevelSelectStateObject = Instantiate(LevelSelectStateObject, transform);
        LeaderboardStateObject = Instantiate(LeaderboardStateObject, transform);
        GamePlayStateObject = Instantiate(GamePlayStateObject, transform);
        LevelWonStateObject = Instantiate(LevelWonStateObject, transform);
        GameOverStateObject = Instantiate(GameOverStateObject, transform); 
        CreditsStateObject = Instantiate(CreditsStateObject, transform);
    }

    /* Prefabs */
    public GameObject player1ControllerPrefab;
    public GameObject player2ControllerPrefab;
    public GameObject[] aiControllerPrefab;
    public GameObject aiTankPrefab;
    public GameObject tank1PawnPrefab;
    public GameObject tank2PawnPrefab;
}
