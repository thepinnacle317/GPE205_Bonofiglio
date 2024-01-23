using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Pawn : MonoBehaviour
{
    public float moveSpeed = 0f;
    public float turnSpeed = 0f;

    public Mover mover;

    public virtual void Start()
    {
        mover = GetComponent<Mover>();
    }

    public virtual void Update()
    {
        
    }

    public abstract void MoveForward();
    public abstract void MoveBackward();
    public abstract void RotateClockwise();
    public abstract void RotateCounterClockwise();
}
