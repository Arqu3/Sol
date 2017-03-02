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
    [Range(1.0f, 100.0f)]
    public float m_DashForce = 20.0f;
    [Range(0.01f, 10.0f)]
    public float m_DashTime = 0.5f;
    [Range(0.0f, 20.0f)]
    public float m_DashCooldown = 1.0f;

    [Header("Shooting variables")]
    [Range(0.01f, 10.0f)]
    public float m_ShootInterval = 0.5f;
    public float m_SpawnOffset = 3.0f;
    [Range(1.0f, 200.0f)]
    public float m_ProjectileSpeed = 30.0f;
    [Range(0.0f, 90.0f)]
    public float m_ProjectileSpread = 1.0f;

    [Header("Raycast layers")]
    public LayerMask m_GroundMask;
    public LayerMask m_WallMask;

    [Header("Spawnable prefabs")]
    public GameObject m_ProjectilePrefab;

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
    private float m_DashTimer = 0.0f;
    private Vector2 m_DashDir = Vector2.zero;
    private Transform m_PauseCirle;
    private Vector3 m_PauseScale = Vector3.zero;
    private List<DashCooldown> m_Cooldowns = new List<DashCooldown>();
    public int m_CooldownCharges = 0;

    //Rotation vars
    private Transform m_Cursor;
    private Vector2 m_CursorDir = Vector2.zero;

    //Shooting vars
    private float m_ShootTimer = 0.0f;

    void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
        m_Collider = GetComponent<Collider2D>();

        m_Cursor = transform.FindChild("CursorRotation");
        if (!m_Cursor)
            Debug.LogError("Player could not find cursor rotation!");

        m_PauseCirle = transform.FindChild("PauseCircle");
        if (!m_PauseCirle)
            Debug.LogError("Player could not find pause circle!");

        m_PauseScale = m_PauseCirle.localScale;
        m_PauseCirle.localScale = Vector3.zero;

        var cds = FindObjectsOfType<DashCooldown>();
        if (cds.Length > 0)
        {
            m_CooldownCharges = cds.Length;
            for (int i = 0; i < cds.Length; i++)
            {
                m_Cooldowns.Add(cds[i]);
            }

            for (int write = 0; write < m_Cooldowns.Count; write++)
            {
                m_Cooldowns[write].SetTime(m_DashCooldown);
                for (int sort = 0; sort < m_Cooldowns.Count - 1; sort++)
                {
                    if (m_Cooldowns[sort].m_ID > m_Cooldowns[sort + 1].m_ID)
                    {
                        DashCooldown temp = m_Cooldowns[sort];
                        m_Cooldowns[sort] = m_Cooldowns[sort + 1];
                        m_Cooldowns[sort + 1] = temp;
                    }
                }
            }
        }
        else
            Debug.LogError("Player could not find any cooldown sliders!");
    }
	
	void Update()
    {
        MovementUpdate();
        JumpUpdate();
        DashUpdate();
        CursorRotation();
        ShootUpdate();
	}

    void CursorRotation()
    {
        if (m_Cursor)
        {
            Vector2 dir = Input.mousePosition - Camera.main.WorldToScreenPoint(m_Cursor.position);
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            m_Cursor.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            m_CursorDir = ((m_Cursor.position + m_Cursor.right) - transform.position).normalized;
        }
    }

    void JumpUpdate()
    {
        m_Grounded = GroundCheck();
        if (m_Grounded)
            m_Rigidbody.gravityScale = 0.0f;
        else
            m_Rigidbody.gravityScale = 1.0f;

        m_Jump = Mathf.Round(m_Rigidbody.velocity.y) > 0;
        if (Input.GetKey(KeyCode.Space) && m_Grounded && !m_Jump)
            m_Rigidbody.AddForce(Vector2.up * m_JumpForce, ForceMode2D.Impulse);
    }

    void DashUpdate()
    {
        m_IsPaused = Input.GetMouseButton(1);
        if (m_IsPaused && !m_Interrupted && !m_IsDash && HasCharges())
        {
            m_Rigidbody.velocity *= 0.3f;
            m_PauseTimer += Time.deltaTime;

            if (m_PauseTimer >= m_PauseTime)
            {
                m_PauseTimer = 0.0f;
                m_Interrupted = true;
                m_PauseCirle.localScale = Vector3.zero;
            }

            if (Mathf.Round(m_PauseCirle.localScale.magnitude) == 0 && !m_Interrupted)
                m_PauseCirle.localScale = m_PauseScale;

            //m_PauseCirle.localScale -= Vector3.one * (m_PauseScale.x - 1.0f) * Time.deltaTime;
            m_PauseCirle.localScale = Vector3.Lerp(m_PauseCirle.localScale, Vector3.one, (m_PauseScale.x - 1.0f) * Time.deltaTime / m_PauseTime);
        }
        else if (m_Interrupted)
            m_Interrupted = !Input.GetMouseButtonUp(1);
        else if (!m_Interrupted && !m_IsDash && HasCharges())
        {
            if (Input.GetMouseButtonUp(1))
            {
                m_PauseCirle.localScale = Vector3.zero;
                m_DashDir = m_CursorDir;
                m_IsDash = true;
                m_PauseTimer = 0.0f;
                m_Cooldowns[m_CooldownCharges - 1].SetCooldown(true);
                m_CooldownCharges--;
            }
        }

        if (m_IsDash)
        {
            m_DashTimer += Time.deltaTime;
            m_Rigidbody.velocity = m_DashDir * m_DashForce;

            if (m_DashTimer >= m_DashTime)
            {
                m_DashTimer = 0.0f;
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

    void Shoot()
    {
        if (m_ProjectilePrefab)
        {
            GameObject clone = (GameObject)Instantiate(m_ProjectilePrefab, m_Cursor.position + m_Cursor.right * m_SpawnOffset, Quaternion.Euler(0.0f, 0.0f, m_Cursor.rotation.eulerAngles.z - 90));
            if (clone.GetComponent<Projectile>())
            {
                clone.transform.Rotate(new Vector3(0, 0, Random.Range(-m_ProjectileSpread, m_ProjectileSpread)));
                clone.GetComponent<Rigidbody2D>().AddForce(clone.transform.up * m_ProjectileSpeed, ForceMode2D.Impulse);
                //clone.GetComponent<Rigidbody2D>().AddForce(m_CursorDir * m_ProjectileSpeed, ForceMode2D.Impulse);
            }
        }
    }

    void ShootUpdate()
    {
        if (Input.GetMouseButton(0))
        {
            m_ShootTimer += Time.deltaTime;
            if (m_ShootTimer >= m_ShootInterval)
            {
                m_ShootTimer = 0.0f;
                Shoot();
            }
        }
        //else
        //{
        //    if (m_ShootTimer > 0.0f)
        //    {
        //        m_ShootTimer += Time.deltaTime;
        //        if (m_ShootTimer >= m_ShootInterval)
        //            m_ShootTimer = 0.0f;
        //    }
        //}
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

    void InterruptDash()
    {
        m_IsDash = false;
        m_DashTimer = 0.0f;
        m_Rigidbody.velocity *= 0.3f;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (m_IsDash)
            InterruptDash();
    }

    public void IncreaseCDs()
    {
        m_CooldownCharges++;
    }

    bool HasCharges()
    {
        return m_CooldownCharges > 0;
    }
}
