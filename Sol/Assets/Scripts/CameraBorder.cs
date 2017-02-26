using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[ExecuteInEditMode]
public class CameraBorder : MonoBehaviour
{
    [Header("Sort ID")]
    public int m_ID = 0;

    //Collision vars
    public bool m_IsColliding = false;

    void OnTriggerEnter2D(Collider2D col)
    {
        m_IsColliding = true;
    }

    void OnTriggerExit2D(Collider2D col)
    {
        m_IsColliding = false;
    }

    public void SetColliding(bool state)
    {
        m_IsColliding = state;
    }

    public bool GetColliding()
    {
        return m_IsColliding;
    }

#if UNITY_EDITOR

    void OnDrawGizmos()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawCube(col.bounds.center, col.bounds.size);
        }
    }

#endif
}
