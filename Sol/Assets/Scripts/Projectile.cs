using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    [Header("Projectile damage")]
    [Range(0, 100)]
    public int m_Damage = 1;

    void OnCollisionEnter2D(Collision2D col)
    {
        Destroy(gameObject);
    }
}
