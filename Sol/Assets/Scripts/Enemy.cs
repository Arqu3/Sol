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

    [Header("Raycast layermasks")]
    public LayerMask m_GroundMask;
    public LayerMask m_WallMask;

    //Movement vars
    private bool m_Hit = false;
    private bool m_Grounded = false;
    private int m_Direction = 1;
    //private bool m_Waiting = false;
    private float m_Acc = 0.0f;
    private bool[] m_Bools;

    protected override void Awake()
    {
        base.Awake();

        if (Random.Range(0, 2) == 0)
            m_Direction = 1;
        else
            m_Direction = -1;

        m_Bools = new bool[1];
        m_Bools[0] = false;
    }

    void Update()
    {
        MovementUpdate();
    }

    public override void MovementUpdate()
    {
        Vector2 pos = transform.position;
        pos.x += m_Collider.bounds.extents.x * 1.2f * m_Direction;
        m_Grounded = SolPhysics.DrawCast(pos, Vector2.down, 1.0f, m_GroundMask);

        m_Hit = SolPhysics.DrawCast(pos, transform.right * m_Direction, 3.0f, m_WallMask);

        if (m_Grounded)
        {
            if (!m_Bools[0])
            {
                m_Rigidbody.velocity = new Vector2(m_Direction * m_Speed * m_Acc, m_Rigidbody.velocity.y);
                m_Acc = Mathf.Lerp(m_Acc, 1.0f, m_Acceleration * Time.deltaTime);
            }

            if (m_Hit)
            {
                m_Direction *= -1;
                m_Acc = 0.0f;
                StartCoroutine(WaitForTime(m_WaitTime, 0));
            }
        }
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
