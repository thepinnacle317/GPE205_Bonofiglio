using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RestartButton : MonoBehaviour
{
    public Button restartButton;

    public void RestartButtonPressed()
    {
        // Check to make sure there is a valid GameManager
        if (GameManager.instance != null)
        {
            GameManager.instance.restart = true;
            GameManager.instance.RestartGame();
        }
    }
}
