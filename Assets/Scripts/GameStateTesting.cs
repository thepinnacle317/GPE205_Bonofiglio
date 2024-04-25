using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateTesting : MonoBehaviour
{
    // Game State Changing Key Bindings
    public KeyCode mainmenuKey;
    public KeyCode titlescreenKey;
    public KeyCode optionsKey;
    public KeyCode levelselectKey;
    public KeyCode leaderboardKey;
    public KeyCode gameplayKey;
    public KeyCode levelWon;
    public KeyCode gameoverKey;
    public KeyCode creditsKey;

    // Game Manager
    public GameObject gameManager;
    private GameManager manager;

    // Start is called before the first frame update
    void Start()
    {
        manager = gameManager.GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        ReadInputs();
    }

    public void ReadInputs()
    {
        if (Input.GetKeyDown(titlescreenKey))
        {
            manager.ActivateTitleScreen();
        }
        if (Input.GetKeyDown(mainmenuKey))
        {
            manager.ActivateMainMenu();
        }
        if (Input.GetKeyDown(optionsKey))
        {
            manager.ActivateOptionsMenu();
        }
        if (Input.GetKeyDown(leaderboardKey))
        {
            manager.ActivateLeaderboard();
        }
        if (Input.GetKeyDown(levelselectKey))
        {
            manager.ActivateLevelSelectScreen();
        }
        if (Input.GetKeyDown(gameplayKey))
        {
            manager.ActivateGamePlayState();
        }
        if (Input.GetKeyDown(gameoverKey))
        {
            manager.ActivateGameOverScreen();
        }
        if (Input.GetKeyDown(creditsKey))
        {
            manager.ActivateCreditsScreen();
        }
        if (Input.GetKeyDown(levelWon))
        {
            manager.ActivateLevelWonState();
        }
    }
}
