using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CloseButton : MonoBehaviour
{
    public Button closeButton;

    public void CloseSettingsMenu()
    {
        // Check to make sure there is a valid GameManager
        if (GameManager.instance != null)
        {
            GameManager.instance.ActivateMainMenu();
        }
    }
}
