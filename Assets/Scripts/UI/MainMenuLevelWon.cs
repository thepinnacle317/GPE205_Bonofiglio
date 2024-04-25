using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuLevelWon : MonoBehaviour
{
    public void ReturnToMainMenu()
    {
        // Check to make sure there is a valid GameManager
        if (GameManager.instance != null)
        {
            GameManager.instance.restart = true;
            GameManager.instance.ReturnToMainMenu();
        }
    }
}
