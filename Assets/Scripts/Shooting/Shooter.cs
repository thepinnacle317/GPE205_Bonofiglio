using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public  abstract class Shooter : MonoBehaviour
{
    public float shotDelay;
    public abstract void Start();
    public abstract void Update();
    public abstract void Shoot(GameObject tankhellPrefab, float shellForce, float damageDone, float lifespan);


}
