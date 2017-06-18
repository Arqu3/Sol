using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SolLib;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    [Header("Projectile damage")]
    [Range(0, 100)]
    public int m_Damage = 1;

    void OnCollisionEnter2D(Collision2D col)
    {
        GameObject particles = (GameObject)SolGame.SpawnResourceByName("ProjectileParticles", col.contacts[0].point, 1.0f);
        Vector2 normal = col.contacts[0].normal;
        particles.transform.rotation = Quaternion.FromToRotation(Vector3.forward, normal);
        ProjectileParticles pp = particles.GetComponent<ProjectileParticles>();
        if (pp)
            pp.SetColor(GetComponent<SpriteRenderer>().color);

        PhysicsEntity pEntity = col.gameObject.GetComponent<PhysicsEntity>();
        if (pEntity)
            pEntity.OnHit();

        Destroy(gameObject);
    }
}
