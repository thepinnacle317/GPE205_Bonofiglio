using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace FXV.ShieldDemo
{
    public class fxvDemo : MonoBehaviour
    {
        [SerializeField]
        Text shieldToggleText;

        [SerializeField]
        GameObject[] shieldTypes;

        int currentType = 0;

        void Start()
        {
            shieldToggleText.gameObject.SetActive(false);
        }

        public void SetActionTex(string text)
        {
            if (text != null)
            {
                shieldToggleText.gameObject.SetActive(true);
                shieldToggleText.text = text;
            }
            else
            {
                shieldToggleText.gameObject.SetActive(false);
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                Shield[] shields = GameObject.FindObjectsByType<Shield>(FindObjectsSortMode.None);
                foreach (Shield s in shields)
                {

                    if (s.GetIsShieldActive())
                    {
                       // if (Random.Range(0, 100) < 50)
                        {
                            s.SetShieldActive(false);
                        }
                    }
                    else
                        s.SetShieldActive(true);
                }
            }
        }

        public void NextShieldType()
        {
            currentType++;

            if(currentType >= shieldTypes.Length)
            {
                currentType = 0;
            }

            for (int i = 0; i <  shieldTypes.Length; i++)
            {
                shieldTypes[i].SetActive(i == currentType);
            }
        }

        public void EnableShields()
        {
            Shield[] shields = GameObject.FindObjectsByType<Shield>(FindObjectsSortMode.None);
            foreach (Shield s in shields)
                s.SetShieldActive(true);
        }

        public void DisableShiedls()
        {
            Shield[] shields = GameObject.FindObjectsByType<Shield>(FindObjectsSortMode.None);
            foreach (Shield s in shields)
                s.SetShieldActive(false);
        }
    }

}