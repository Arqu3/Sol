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
    public float m_DashForce = 20.0f;
    public float m_DashTime = 0.5f;

    [Header("Raycast layers")]
    public LayerMask m_GroundMask;
    public LayerMask m_WallMask;

    //Component vars
    private Rigidbody2D m_Rigidbody;
    private Collider2D m_Collider;

    //Air vars
    private bool m_Grounded = false;
    private bool m_Jump = false;

    //Movement vars
    private float m_Horizontal = 0.0f;

    //Dash vars
    private bool m_IsPaused = false;
    private float m_PauseTimer = 0.0f;
    private bool m_IsDash = false;
    private bool m_Interrupted = false;
    private Vector2 m_DashDir = Vector2.zero;
    private float m_DashTimer = 0.0f;

    //Rotation vars
    private Transform m_CursorRotation;

    void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
        m_Collider = GetComponent<Collider2D>();

        m_CursorRotation = transform.FindChild("CursorRotation");
        if (!m_CursorRotation)
            Debug.LogError("Player could not find cursor rotation!");
    }
	
	void Update()
    {
        MovementUpdate();
        JumpUpdate();
        DashUpdate();
        CursorRotation();
	}

    void CursorRotation()
    {
        if (m_CursorRotation)
        {
            Vector2 dir = Input.mousePosition - Camera.main.WorldToScreenPoint(m_CursorRotation.position);
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            m_CursorRotation.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    void JumpUpdate()
    {
        m_Grounded = GroundCheck();
        m_Jump = m_Rigidbody.velocity.y > 0;
        if (Input.GetKey(KeyCode.Space) && m_Grounded && !m_Jump)
            m_Rigidbody.AddForce(Vector2.up * m_JumpForce, ForceMode2D.Impulse);
    }

    void DashUpdate()
    {
        m_IsPaused = Input.GetMouseButton(1);
        if (m_IsPaused && !m_Interrupted)
        {
            m_Rigidbody.gravityScale = 0.0f;
            m_Rigidbody.velocity = Vector2.zero;
            m_PauseTimer += Time.deltaTime;

            if (m_PauseTimer >= m_PauseTime)
            {
                m_PauseTimer = 0.0f;
                m_Interrupted = true;
                m_Rigidbody.gravityScale = 1.0f;
            }
        }
        else if (m_Interrupted)
            m_Interrupted = !Input.GetMouseButtonUp(1);
        else if (!m_Interrupted)
        {
            if (Input.GetMouseButtonUp(1))
            {
                m_DashDir = ((m_CursorRotation.position + m_CursorRotation.transform.right) - transform.position).normalized;
                m_IsDash = true;
                m_PauseTimer = 0.0f;
                m_Rigidbody.gravityScale = 1.0f;
            }
        }

        if (m_IsDash)
        {
            m_DashTimer += Time.deltaTime;
            m_Rigidbody.velocity = m_DashDir * m_DashForce;

            if (m_DashTimer >= m_DashTime)
            {
                m_DashTimer = 0.0f;
                m_Rigidbody.gravityScale = 1.0f;
                m_IsDash = false;
                m_Rigidbody.velocity *= 0.3f;
            }
        }
    }

    public override void MovementUpdate()
    {
        if (!WallCheck())
            m_Horizontal = Input.GetAxis("Horizontal");

        if (!m_IsDash)
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
                switch(i)
                {
                    case 0:
                        if (horizontal < 0.0f)
                            m_Horizontal = 0.0f;
                        else if (horizontal > 0.0f)
                            m_Horizontal = horizontal;
                        break;

                    case 1:
                        if (horizontal > 0.0f)
                            m_Horizontal = 0.0f;
                        else if (horizontal < 0.0f)
                            m_Horizontal = horizontal;
                        break;
                }

                hit = true;
            }
        }
        return hit;
    }
}
