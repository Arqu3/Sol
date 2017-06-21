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

    [Header("Bounce variables")]
    public bool m_Bounce = false;
    [Range(1, 10)]
    public int m_NumBounce = 2;

    //Bounce vars
    private int m_CurBounce = 0;
    private Vector2 m_Velocity = Vector2.zero;

    //Component vars
    private Rigidbody2D m_Rigidbody;

    void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
        m_Velocity = m_Rigidbody.velocity;
        
    }

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

        if (m_Bounce)
        {
            if (m_CurBounce >= m_NumBounce) Destroy(gameObject);
            else
            {
                ++m_CurBounce;
                m_Rigidbody.velocity = Vector2.Reflect(m_Velocity, normal);
                m_Velocity = m_Rigidbody.velocity;
            }
        }
        else Destroy(gameObject);
    }

    public void SetColor(Color col)
    {
        GetComponent<SpriteRenderer>().color = col;
        TrailRenderer trails = GetComponent<TrailRenderer>();
        trails.startColor = col;
        trails.endColor = col;
    }

    public void SetLayer(bool player)
    {
        if (player) gameObject.layer = 11;
        else gameObject.layer = 12;
    }
}
