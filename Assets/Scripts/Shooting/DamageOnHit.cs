using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageOnHit : MonoBehaviour
{
    // Damage to dealt variable
    public float damageDone;

    // Pawns Involved 
    public Pawn target;
    public Pawn attacker;
    
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
        target = other.gameObject.GetComponent<Pawn>();
        // Only damage the hit object if it has a valid Health Component
        if (targetHealth != null)
        {
            targetHealth.TakeDamage(damageDone, attacker, target);
        }

        // Destroy itself on hit
        Destroy(gameObject);
    }
}
