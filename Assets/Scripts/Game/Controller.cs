using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class Controller : MonoBehaviour
{
    // Used to hold the pawn.
    public Pawn pawn;

    public int score;

    public virtual void Start()
    {
        
    }

    public virtual void Update()
    {
        
    }

    public virtual void ProcessInputs()
    {

    }

    public void AddToScore(int amount)
    {
        score += amount;
    }
}
