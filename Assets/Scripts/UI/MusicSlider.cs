using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MusicSlider : MonoBehaviour
{
    public Slider musicSlider;
    public AudioMixer Mixer;

    public void UpdateMusicVolume()
    {
        // Check to make sure there is a valid GameManager
        if (GameManager.instance != null)
        {
            float newVolume = musicSlider.value;
            if (newVolume <= 0)
            {
                newVolume = -80f;
            }
            else
            {
                newVolume = Mathf.Log10(newVolume) * 20;
                Mixer = GameManager.instance.audioMixer;
                Mixer.SetFloat("Music_Volume", newVolume);
                
            }
           
        }
    }
}
