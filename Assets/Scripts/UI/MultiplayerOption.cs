using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerOption : MonoBehaviour
{
    public Toggle multiplayerToggle;

    public void MultiplayerToggle()
    {
        // Check to make sure there is a valid GameManager
        if (GameManager.instance != null)
        {
            if (!multiplayerToggle.isOn)
            {
                multiplayerToggle.isOn = true;
            }
            else
            {
                multiplayerToggle.isOn = false;
            }
            

            // TODO: set single player of multiplayer.
        }
    }
}
