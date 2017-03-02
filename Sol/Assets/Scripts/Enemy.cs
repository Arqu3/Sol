using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : PhysicsEntity
{
    [Header("Movement variables")]
    [Range(1.0f, 100.0f)]
    public float m_Speed = 10.0f;

    [Header("Raycast layermasks")]
    public LayerMask m_GroundMask;
    public LayerMask m_WallMask;

    //Movement vars
    private bool m_Hit = false;
    private bool m_Grounded = false;
    private int m_Direction = 1;

    protected override void Awake()
    {
        base.Awake();

        if (Random.Range(0, 2) == 0)
            m_Direction = 1;
        else
            m_Direction = -1;
    }

    void Update()
    {
        MovementUpdate();
    }

    public override void MovementUpdate()
    {
        Vector2 pos = transform.position;
        pos.x += m_Collider.bounds.extents.x * 1.2f * m_Direction;
        m_Grounded = DrawCast(pos, Vector2.down, 1.0f, m_GroundMask);

        m_Hit = DrawCast(pos, transform.right * m_Direction, 3.0f, m_WallMask);

        if (m_Grounded)
        {
            m_Rigidbody.velocity = new Vector2(m_Direction * m_Speed, m_Rigidbody.velocity.y);

            if (m_Hit)
                m_Direction *= -1;
        }
    }

    public override void OnHit()
    {
        Destroy(gameObject);
    }
}
