using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Mover : MonoBehaviour
{
    public abstract void Start();

    public abstract void Move(Vector3 direction, float speed);
    public abstract void Rotate(float rotationspeed);

}
