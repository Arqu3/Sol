using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Player : Entity
{
    //Public vars
    [Header("Movement variables")]
    [Range(1.0f, 100.0f)]
    public float m_Speed = 20.0f;

    [Header("Air variables")]
    [Range(0.1f, 100.0f)]
    public float m_JumpForce = 20.0f;

    //Component vars
    private Rigidbody2D m_Rigidbody;
    private Collider2D m_Collider;

    void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
        m_Collider = GetComponent<Collider2D>();
    }
	
	void Update()
    {
        MovementUpdate();
        JumpUpdate();
	}

    void JumpUpdate()
    {
        bool ground = OnGround();
        bool inAir = m_Rigidbody.velocity.y > 0;
        if (Input.GetKey(KeyCode.Space) && ground && !inAir)
            m_Rigidbody.AddForce(Vector2.up * m_JumpForce, ForceMode2D.Impulse);
    }

    bool OnGround()
    {
        bool ground = false;
        Color col = Color.red;
        float dist = 0.6f;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector3.down, dist);
        if (hit)
        {
            col = Color.green;
            ground = true;
        }

        Debug.DrawRay(transform.position, Vector3.down * dist, col);

        return ground;
    }

    public override void MovementUpdate()
    {
        m_Rigidbody.velocity = new Vector2(Input.GetAxis("Horizontal") * m_Speed, m_Rigidbody.velocity.y);
    }
}
