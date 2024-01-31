using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageOnHit : MonoBehaviour
{
    // Damage to dealt variable
    public float damageDone;
    // Instigator
    public Pawn Owner;
    
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void OnTriggerEnter(Collider other)
    {
        // Get the Health Component of the hit game object.
        Health targetHealth = other.gameObject.GetComponent<Health>();

        // Only damage the hit object if it has a valid Health Component
        if (targetHealth != null)
        {
            targetHealth.TakeDamage(damageDone, Owner);
        }

        // Destroy itself on hit
        Destroy(gameObject);
    }
}
