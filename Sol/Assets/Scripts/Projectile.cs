using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    [Header("Projectile damage")]
    [Range(0, 100)]
    public int m_Damage = 1;

    [Header("On death particles")]
    public GameObject m_ParticlePrefab;

    void OnCollisionEnter2D(Collision2D col)
    {
        if (m_ParticlePrefab)
        {
            GameObject particles = (GameObject)Instantiate(m_ParticlePrefab, col.contacts[0].point, Quaternion.identity);
            Vector2 normal = col.contacts[0].normal;
            particles.transform.rotation = Quaternion.FromToRotation(Vector3.forward, normal);
            ProjectileParticles pp = particles.GetComponent<ProjectileParticles>();
            if (pp)
                pp.SetColor(GetComponent<SpriteRenderer>().color);
        }

        PhysicsEntity pEntity = col.gameObject.GetComponent<PhysicsEntity>();
        if (pEntity)
            pEntity.OnHit();

        Destroy(gameObject);
    }
}
