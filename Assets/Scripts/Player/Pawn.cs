using FXV;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Pawn : MonoBehaviour
{
    /* Controller */
    public Controller controller;

    /* Movement Variables */
    public float moveSpeed = 0f;
    public float turnSpeed = 0f;

    /* Shooting Variables */
    public GameObject shellPrefab;
    public float shellForce;
    public float damageDone;
    public float shellLifespan;
    public float fireRate;

    /* Noise Variables */
    public float noiseMakerVolume;

    /* Components */
    [HideInInspector]
    public Mover mover;
    [HideInInspector]
    public TankShooter shooter;
    [HideInInspector]
    public NoiseMaker noiseMaker;

    /* Armor Shield */
    public GameObject shieldPrefab;
    [HideInInspector]
    public Shield shield;
    public Transform shieldTransform;

    /* UI */
    public Image healthImage;
    public Image armorImage;
    public Image damageBoost;
    public Text livesText;
    public Text scoreText;

    public virtual void Start()
    {
        mover = GetComponent<Mover>();
        shooter = GetComponent<TankShooter>();
        noiseMaker = GetComponent<NoiseMaker>();
        shield = GetComponent<Shield>();
    }

    public virtual void Update()
    {
        
    }

    public abstract void MoveForward();
    public abstract void MoveBackward();
    public abstract void RotateClockwise();
    public abstract void RotateCounterClockwise();
    public abstract void Shoot();
    public abstract void MakeNoise();
    public abstract void StopNoise();

    /* AI Methods */
    public abstract void RotateTowards(Vector3 targetPosition);
}
