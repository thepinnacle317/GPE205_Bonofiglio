using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelect : MonoBehaviour
{
    public Button modButton;
    public Button randomButton;
    public InputField seedInput;

    public void PlayMOD()
    {
        if (GameManager.instance != null)
        {
            // Set the GameManager to the selected map type
            GameManager.instance.SetMapType(GameManager.MapType.MapOfTheDay);
            Debug.Log(GameManager.instance.mapType);

            // Start the game with the given map type
            GameManager.instance.ActivateGamePlayState();
        }
    }

    public void PlaySeedMap()
    {
        

        if (GameManager.instance != null)
        {
            // TODO: Check input field for seed first
            GameManager.instance.gmMapSeed = int.Parse(seedInput.text);

            GameManager.instance.SetMapType(GameManager.MapType.MapSeed);
            Debug.Log(GameManager.instance.mapType);
            Debug.Log(GameManager.instance.gmMapSeed);

            // Start the game with the given map type
            GameManager.instance.ActivateGamePlayState();
        }
    }

    public void PlayRandomMap()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.SetMapType(GameManager.MapType.RandomTimeGeneration);
            Debug.Log(GameManager.instance.mapType);

            // Start the game with the given map type
            GameManager.instance.ActivateGamePlayState();
        }
    }
}
