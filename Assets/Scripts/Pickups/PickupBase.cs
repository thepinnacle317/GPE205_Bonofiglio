using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupBase : MonoBehaviour
{
    public float rotationRate = .1f;
    public bool bdoesRotate;
    public float verticalRate = 2f;
    public bool bdoesBounce;
    public float verticalDistance = .25f;
    private Vector3 startingPosition;

    public virtual void Start()
    {
        startingPosition = transform.position;
        
    }

    public virtual void Update()
    {
        if (bdoesBounce)
        {
            transform.position = new Vector3(startingPosition.x, startingPosition.y + Mathf.Sin(verticalRate * Time.time) * verticalDistance, startingPosition.z);
        }
        Spin();
       
    }

    public void Spin()
    {
        if (bdoesRotate == true)
        {
            transform.Rotate(0, rotationRate, 0);
        }
    }
}
