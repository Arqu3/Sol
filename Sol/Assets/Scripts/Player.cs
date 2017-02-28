using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Player : PhysicsEntity
{
    //Public vars
    [Header("Movement variables")]
    [Range(1.0f, 100.0f)]
    public float m_Speed = 20.0f;

    [Header("Air variables")]
    [Range(0.1f, 100.0f)]
    public float m_JumpForce = 20.0f;

    [Header("Dash variables")]
    [Range(0.1f, 10.0f)]
    public float m_PauseTime = 2.0f;

    [Header("Raycast layers")]
    public LayerMask m_GroundMask;
    public LayerMask m_WallMask;

    //Component vars
    private Rigidbody2D m_Rigidbody;
    private Collider2D m_Collider;

    //Air vars
    private bool m_Grounded = false;
    private bool m_Air = false;

    //Movement vars
    private float m_Horizontal = 0.0f;

    //Dash vars
    private bool m_IsPaused = false;
    private float m_PauseTimer = 0.0f;

    void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
        m_Collider = GetComponent<Collider2D>();
    }
	
	void Update()
    {
        MovementUpdate();
        JumpUpdate();
        DashUpdate();
	}

    void JumpUpdate()
    {
        m_Grounded = GroundCheck();
        m_Air = m_Rigidbody.velocity.y > 0;
        if (Input.GetKey(KeyCode.Space) && m_Grounded && !m_Air)
            m_Rigidbody.AddForce(Vector2.up * m_JumpForce, ForceMode2D.Impulse);
    }

    void DashUpdate()
    {
        m_IsPaused = Input.GetMouseButton(1);
        if (m_IsPaused)
        {
            m_Rigidbody.gravityScale = 0.0f;
        }
        else
            m_Rigidbody.gravityScale = 1.0f;
    }

    public override void MovementUpdate()
    {
        if (!WallCheck())
            m_Horizontal = Input.GetAxis("Horizontal");

        m_Rigidbody.velocity = new Vector2(m_Horizontal * m_Speed, m_Rigidbody.velocity.y);
    }

    bool GroundCheck()
    {
        float dist = 0.6f;
        Vector2 pos = m_Collider.bounds.center;
        pos.y -= m_Collider.bounds.size.y / 1.9f;
        pos.x -= dist / 2.0f;
        RaycastHit2D hit = DrawCast(pos, Vector2.right, dist, m_GroundMask);
        return hit;
    }

    bool WallCheck()
    {
        float x = 0.0f;
        float dist = 0.6f;
        bool hit = false;
        for (int i = 0; i < 2; i++)
        {
            switch(i)
            {
                case 0:
                    x = m_Collider.bounds.size.x;
                    break;

                case 1:
                    x *= -1.0f;
                    break;
            }

            Vector2 pos = m_Collider.bounds.center;
            pos.x -= x / 1.9f;
            pos.y -= dist / 2.0f;

            RaycastHit2D rHit = DrawCast(pos, Vector2.up, dist, m_WallMask);

            if (rHit)
            {
                float horizontal = Input.GetAxis("Horizontal");
                if (i == 0)
                {
                    if (horizontal < 0.0f)
                        m_Horizontal = 0.0f;
                    else if (horizontal > 0.0f)
                        m_Horizontal = horizontal;
                }
                else if (i == 1)
                {
                    if (horizontal > 0.0f)
                        m_Horizontal = 0.0f;
                    else if (horizontal < 0.0f)
                        m_Horizontal = horizontal;
                }

                hit = true;
            }
        }
        return hit;
    }
}
