using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class Controller : MonoBehaviour
{
    // Used to hold the pawn.
    public Pawn pawn;
    public int currentLives;
    public int maxLives = 3;
    public int score;

    public virtual void Start()
    {
        pawn.GetComponent<Health>().UpdateLivesUI(pawn.controller);
    }

    public virtual void Update()
    {
        pawn.scoreText.text = score.ToString();
    }

    public virtual void ProcessInputs()
    {

    }

    public void AddToScore(int amount)
    {
        score += amount;
        Debug.Log("Your current score is: " + score);
    }
}
