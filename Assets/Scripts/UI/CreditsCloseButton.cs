using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreditsCloseButton : MonoBehaviour
{
    public Button creditsCloseButton;

    public void CloseCreditsMenu()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.ActivateMainMenu();
        }
    }
}
