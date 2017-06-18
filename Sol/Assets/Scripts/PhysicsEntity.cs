using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PhysicsEntity : Entity
{
    //Component vars
    protected Rigidbody2D m_Rigidbody;
    protected Collider2D m_Collider;

    protected virtual void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
        m_Collider = GetComponent<Collider2D>();
    }

    public override void MovementUpdate()
    {
        Debug.LogError("PhysicsEntity base class does not have an implementation for movementupdate!");
    }

    public override void OnHit()
    {
        Debug.LogError("PhysicsEntity base class does not have an implementation for OnHit!");
    }
}
