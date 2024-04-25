using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SFXSlider : MonoBehaviour
{
    public Slider sfxSlider;
    public AudioMixer mixer;

    public void UpdateSFXVolume()
    {
        // Check to make sure there is a valid GameManager
        if (GameManager.instance != null)
        {
            float newVolume = sfxSlider.value;
            if (newVolume <= 0)
            {
                newVolume = -80f;
            }
            else
            {
                newVolume = Mathf.Log10(newVolume) * 20;
                mixer = GameManager.instance.audioMixer;
                mixer.SetFloat("SFX_Volume", newVolume);
            }
        }
    }
}
