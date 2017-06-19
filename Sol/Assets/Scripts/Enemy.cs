using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SolLib;

public class Enemy : PhysicsEntity
{
    [Header("Movement variables")]
    [Range(1.0f, 100.0f)]
    public float m_Speed = 10.0f;
    public float m_WaitTime = 1.0f;
    public float m_Acceleration = 2.0f;

    [Header("Projectile Variables")]
    public GameObject m_ProjectilePrefab;
    public Color m_ProjectileColor = Color.red;

    [Header("Attacking variables")]
    [Range(0.0f, 75.0f)]
    public float m_AggroRange = 15.0f;
    [Range(0.1f, 10.0f)]
    public float m_FireRate = 1.0f;

    [Header("Raycast layermasks")]
    public LayerMask m_GroundMask;
    public LayerMask m_WallMask;

    //Movement vars
    private bool m_HorizontalHit = false;
    private bool m_Grounded = false;
    private int m_Direction = 1;
    //private bool m_Waiting = false;
    private float m_Acc = 0.0f;
    private bool[] m_Bools;

    //Shooting variables
    private Transform m_PlayerTransform;
    private float m_FireTimer = 0.0f;

    protected override void Awake()
    {
        base.Awake();

        if (Random.Range(0, 2) == 0)
            m_Direction = 1;
        else
        {
            m_Direction = -1;
            Vector3 scale = transform.localScale;
            scale.x = -1;
            transform.localScale = scale;
        }

        m_Bools = new bool[1];
        m_Bools[0] = false;
    }

    void Start()
    {
        m_PlayerTransform = Toolbox.Instance.GetPlayer().transform;
    }

    void Update()
    {
        MovementUpdate();
        ShootUpdate();
    }

    public override void MovementUpdate()
    {
        Vector2 pos = transform.position;
        pos.x += m_Collider.bounds.extents.x * 1.2f * m_Direction;
        m_Grounded = SolPhysics.DrawCast(pos, Vector2.down, 1.0f, m_GroundMask);

        m_HorizontalHit = SolPhysics.DrawCast(pos, transform.right * m_Direction, 3.0f, m_WallMask);

        if (m_Grounded)
        {
            if (!m_Bools[0])
            {
                m_Rigidbody.velocity = new Vector2(m_Direction * m_Speed * m_Acc, m_Rigidbody.velocity.y);
                m_Acc = Mathf.Lerp(m_Acc, 1.0f, m_Acceleration * Time.deltaTime);
            }

            if (m_HorizontalHit)
            {
                m_Direction *= -1;
                Vector3 scale = transform.localScale;
                scale.x *= -1;
                transform.localScale = scale;
                m_Acc = 0.0f;
                StartCoroutine(WaitForTime(m_WaitTime, 0));
            }
        }
    }

    void ShootUpdate()
    {
        m_FireTimer += Time.deltaTime;
        if (m_FireTimer >= m_FireRate)
        {
            m_FireTimer = 0.0f;
            Shoot();
        }
    }

    void Shoot()
    {
        GameObject go = Instantiate(m_ProjectilePrefab, transform.position + (m_PlayerTransform.position - transform.position).normalized, Quaternion.identity);
        go.GetComponent<Rigidbody2D>().AddForce((m_PlayerTransform.position - transform.position).normalized * 10.0f, ForceMode2D.Impulse);
        go.GetComponent<SpriteRenderer>().color = m_ProjectileColor;
    }

    IEnumerator WaitForTime(float time, int boolIndex)
    {
        float timer = 0.0f;
        m_Bools[boolIndex] = true;
        while(timer < time)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        m_Bools[boolIndex] = false;
    }

    public override void OnHit()
    {
        Toolbox.Instance.GetEventManager().OnEnemyDeath(transform.position);
        Destroy(gameObject);
    }
}
