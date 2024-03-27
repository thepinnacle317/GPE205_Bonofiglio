using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LS_BackButton : MonoBehaviour
{
    public Button lsBackButton;

    public void ReturnToMainMenu()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.ActivateMainMenu();
        }
    }
}
