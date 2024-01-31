using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankMovementComp : Mover
{
    private Rigidbody rigid_body;
    private Transform tank_transform;

    public override void Start()
    {
        // Initialize our private variables to the components.
        rigid_body = GetComponent<Rigidbody>();
        tank_transform = GetComponent<Transform>();
    }

    public override void Move(Vector3 direction, float speed)
    {
        // This method handles the tank movement on the X and Z axis.
        Vector3 movement = direction.normalized * speed * Time.deltaTime;
        rigid_body.MovePosition(rigid_body.position + movement);
    }

    public override void Rotate(float rotationspeed)
    {
        // This method handles the rotation of the tank on the Y axis. 
        tank_transform.Rotate(0, rotationspeed * Time.deltaTime, 0);
    }
}
