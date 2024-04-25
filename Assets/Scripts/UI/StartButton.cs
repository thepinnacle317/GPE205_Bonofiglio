using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartButton : MonoBehaviour
{
    public Button startButton;
    public AudioClip clip;
    public AudioSource audioSource;

    public void Start()
    {
        if (GameManager.instance != null)
        {
            audioSource = GameManager.instance.GetComponent<AudioSource>();
        }
    }
    public void StartButtonPressed()
    {
            audioSource.PlayOneShot(clip);
            GameManager.instance.ActivateMainMenu();
    }
}
