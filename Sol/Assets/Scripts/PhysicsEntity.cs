using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsEntity : Entity
{
    public override void MovementUpdate()
    {
        Debug.LogError("PhysicsEntity base class does not have an implementation for movementupdate!");
    }

    protected RaycastHit2D DrawCast(Vector2 position, Vector2 direction, float distance)
    {
        RaycastHit2D hit = Physics2D.Raycast(position, direction, distance);
        Color col = hit ? Color.green : Color.red;
        Debug.DrawRay(position, direction * distance, col);
        return hit;
    }

    protected RaycastHit2D DrawCast(Vector2 position, Vector2 direction, float distance, int layermask)
    {
        RaycastHit2D hit = Physics2D.Raycast(position, direction, distance, layermask);
        Color col = hit ? Color.green : Color.red;
        Debug.DrawRay(position, direction * distance, col);
        return hit;
    }
}
