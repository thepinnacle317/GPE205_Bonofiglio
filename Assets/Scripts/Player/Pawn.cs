using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Pawn : MonoBehaviour
{
    // Movement Variables
    public float moveSpeed = 0f;
    public float turnSpeed = 0f;

    // Shooting Variables
    public GameObject shellPrefab;
    public float shellForce;
    public float damageDone;
    public float shellLifespan;
    public float fireRate;

    // Components
    [HideInInspector]
    public Mover mover;
    [HideInInspector]
    public Shooter shooter;

    public virtual void Start()
    {
        mover = GetComponent<Mover>();
        shooter = GetComponent<Shooter>();
    }

    public virtual void Update()
    {
        
    }

    public abstract void MoveForward();
    public abstract void MoveBackward();
    public abstract void RotateClockwise();
    public abstract void RotateCounterClockwise();
    public abstract void Shoot();

    /* AI Methods */
    public abstract void RotateTowards(Vector3 targetPosition);
    public abstract void Ocillate(float angle);
}
