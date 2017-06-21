using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;
using SolLib;

public class Player : PhysicsEntity
{
    //Public vars
    [Header("Movement variables")]
    [Range(1.0f, 100.0f)]
    public float m_Speed = 20.0f;

    [Header("Air variables")]
    [Range(0.1f, 100.0f)]
    public float m_JumpForce = 20.0f;
    [Range(0.0f, 10.0f)]
    public float m_FallGraceTime = 0.5f;
    [Range(1, 10)]
    public int m_NumberOfJumps = 1;

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
    public Color m_ProjectileColor = Color.blue;
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

    //Air vars
    private bool m_Grounded = false;
    private bool m_InAir = false;
    private float m_FallGraceTimer = 0.0f;
    private bool m_GraceJump = false;
    private bool m_HasLanded = false;
    private int m_CurNumJumps = 0;
    private bool m_IsJumpPressed = false;
    private bool m_WasJumpPressed = false;

    //Movement vars
    private float m_Horizontal = 0.0f;

    //Dash vars
    private bool m_IsPaused = false;
    private bool m_WasPaused = false;
    private float m_PauseTimer = 0.0f;
    private bool m_IsDash = false;
    private bool m_Interrupted = false;
    private float m_DashTimer = 0.0f;
    private Vector2 m_DashDir = Vector2.zero;
    private Transform m_PauseCirle;
    private Vector3 m_PauseScale = Vector3.zero;
    private List<DashCooldown> m_Cooldowns = new List<DashCooldown>();
    private int m_CooldownCharges = 0;
    private bool m_HasStartedCharge = false;

    //Rotation vars
    private Transform m_Cursor;
    private Vector2 m_CursorDir = Vector2.zero;

    //Shooting vars
    private float m_ShootTimer = 0.0f;

    //Controller vars
    private PlayerIndex m_PIndex;
    private GamePadState m_GPadState;

    //Particle system vars
    private ParticleSystem m_DashChargeSystem;
    private ParticleSystem m_ShootSystem;

    protected override void Awake()
    {
        base.Awake();

        m_Cursor = transform.Find("CursorRotation");
        if (!m_Cursor) Debug.LogError("Player could not find cursor rotation!");
        else m_ShootSystem = m_Cursor.GetComponentInChildren<ParticleSystem>();

        m_PauseCirle = transform.Find("PauseCircle");
        if (!m_PauseCirle)
            Debug.LogError("Player could not find pause circle!");

        m_PauseScale = m_PauseCirle.localScale;
        m_PauseCirle.localScale = Vector3.zero;

        m_DashChargeSystem = transform.Find("PlayerDashChargeParticles").GetComponent<ParticleSystem>();

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

    void Start()
    {
        for (int i = 0; i < m_Cooldowns.Count; i++)
        {
            m_Cooldowns[i].SetTime(m_DashCooldown);
        }

        for (int i = 0; i < 4; i++)
        {
            PlayerIndex test = (PlayerIndex)i;
            GamePadState state = GamePad.GetState(test);

            if (state.IsConnected)
            {
                m_PIndex = test;
                Debug.Log("Found controller at index: " + i);
                break;
            }
        }

        SolPhysics.DrawCast(transform.position, Vector2.up, 100.0f);
    }
	
	void Update()
    {
        m_GPadState = GamePad.GetState(m_PIndex);

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
            //Vector2 dir = Input.mousePosition - Camera.main.WorldToScreenPoint(m_Cursor.position);
            Vector2 dir = new Vector2(m_GPadState.ThumbSticks.Right.X, m_GPadState.ThumbSticks.Right.Y);
            if (dir.magnitude > 0.3f)
            {
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

                m_Cursor.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                m_CursorDir = ((m_Cursor.position + m_Cursor.right) - transform.position).normalized;
            }
        }
    }

    void JumpUpdate()
    {
        m_IsJumpPressed = m_GPadState.Buttons.A == ButtonState.Pressed;

        m_Grounded = GroundCheck();

        if (!m_IsDash)
        {
            if (m_Grounded)
                m_Rigidbody.gravityScale = 0.0f;
            else
                m_Rigidbody.gravityScale = 1.0f;
        }

        m_InAir = Mathf.Round(m_Rigidbody.velocity.y) != 0;
        if (m_InAir)
            m_HasLanded = false;

        if ((Input.GetKeyDown(KeyCode.Space) || (m_IsJumpPressed && !m_WasJumpPressed)) && !m_IsDash && (((m_Grounded && !m_InAir) || m_CurNumJumps < m_NumberOfJumps) || m_GraceJump))
            Jump();

        if (!m_Grounded && !m_InAir && m_HasLanded)
            m_GraceJump = true;

        if (m_GraceJump)
        {
            m_FallGraceTimer += Time.deltaTime;
            if (m_FallGraceTimer >= m_FallGraceTime)
            {
                m_FallGraceTimer = 0.0f;
                m_GraceJump = false;
                m_HasLanded = false;
            }
        }

        m_WasJumpPressed = m_IsJumpPressed;
    }

    void Jump()
    {
        Vector2 vel = m_Rigidbody.velocity;
        vel.y = 0.0f;
        m_Rigidbody.velocity = vel;
        ++m_CurNumJumps;
        m_Rigidbody.AddForce(Vector2.up * m_JumpForce, ForceMode2D.Impulse);
        m_GraceJump = m_HasLanded = false;
        m_FallGraceTimer = 0.0f;
        Toolbox.Instance.GetEventManager().OnPlayerJump(transform.position);
    }

    void DecrementNumJumps()
    {
        m_CurNumJumps -= 1;
        if (m_CurNumJumps < 0) m_CurNumJumps = 0;
    }

    void DashUpdate()
    {
        m_IsPaused = Mathf.Round(m_GPadState.Triggers.Left) == 1.0f;  

        if (m_IsPaused && !m_Interrupted && !m_IsDash && HasCharges())
        {
            if (!m_HasStartedCharge)
            {
                m_HasStartedCharge = true;
                Toolbox.Instance.GetEventManager().OnPlayerDashChargeStart(this);
            }

            m_Rigidbody.velocity *= 0.3f;
            m_PauseTimer += Time.deltaTime;

            if (m_PauseTimer >= m_PauseTime)
            {
                m_PauseTimer = 0.0f;
                m_Interrupted = true;
                m_PauseCirle.localScale = Vector3.zero;
                m_HasStartedCharge = false;
                Toolbox.Instance.GetEventManager().OnPlayerDashChargeInterrupted(this);
            }

            if (Mathf.Round(m_PauseCirle.localScale.magnitude) == 0 && !m_Interrupted)
                m_PauseCirle.localScale = m_PauseScale;

            //m_PauseCirle.localScale -= Vector3.one * (m_PauseScale.x - 1.0f) * Time.deltaTime;
            m_PauseCirle.localScale = Vector3.Lerp(m_PauseCirle.localScale, Vector3.one * 0.85f, (m_PauseScale.x - 1.0f) * Time.deltaTime / m_PauseTime);
        }
        else if (m_Interrupted)
            m_Interrupted = Mathf.Round(m_GPadState.Triggers.Left) != 0.0f;
        else if (!m_Interrupted && !m_IsDash && HasCharges() && m_WasPaused)
        {
            if (Mathf.Round(m_GPadState.Triggers.Left) == 0.0f)
            {
                m_PauseCirle.localScale = Vector3.zero;
                m_DashDir = m_CursorDir;
                m_IsDash = true;
                m_HasStartedCharge = false;
                m_PauseTimer = 0.0f;
                GetCooldown().SetCooldown(true);
                m_CooldownCharges--;
                m_Rigidbody.gravityScale = 0.0f;
                Toolbox.Instance.GetEventManager().OnPlayerDashStart(this, m_DashDir);
                DecrementNumJumps();
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
                m_Rigidbody.gravityScale = 1.0f;
                m_Rigidbody.velocity *= 0.3f;
            }
        }

        m_WasPaused = m_IsPaused;
    }

    public override void MovementUpdate()
    {
        if (!WallCheck())
            m_Horizontal = m_GPadState.ThumbSticks.Left.X;

        if (!m_IsDash)
            m_Rigidbody.velocity = new Vector2(m_Horizontal * m_Speed, m_Rigidbody.velocity.y);
    }

    void Shoot()
    {
        if (m_ProjectilePrefab)
        {
            GameObject clone = (GameObject)Instantiate(m_ProjectilePrefab, m_Cursor.position + m_Cursor.right * m_SpawnOffset, Quaternion.Euler(0.0f, 0.0f, m_Cursor.rotation.eulerAngles.z - 90));
            Projectile proj = clone.GetComponent<Projectile>();
            if (proj)
            {
                clone.transform.Rotate(new Vector3(0, 0, Random.Range(-m_ProjectileSpread, m_ProjectileSpread)));
                clone.GetComponent<Rigidbody2D>().AddForce(clone.transform.up * m_ProjectileSpeed, ForceMode2D.Impulse);
                proj.SetColor(m_ProjectileColor);
                proj.SetLayer(true);
            }
            m_ShootSystem.Emit(Random.Range(5, 11));
        }
    }

    void ShootUpdate()
    {
        if (Input.GetMouseButton(0) || Mathf.Round(m_GPadState.Triggers.Right) == 1.0f)
        {
            m_ShootTimer += Time.deltaTime;
            if (m_ShootTimer >= m_ShootInterval)
            {
                m_ShootTimer = 0.0f;
                Shoot();
            }
        }
    }

    bool GroundCheck()
    {
        float dist = 0.6f;
        Vector2 pos = m_Collider.bounds.center;
        pos.y -= m_Collider.bounds.size.y / 1.9f;
        pos.x -= dist / 2.0f;
        RaycastHit2D hit = SolPhysics.DrawCast(pos, Vector2.right, dist, m_GroundMask);

        if (hit)
        {
            if (!m_HasLanded)
            {
                m_HasLanded = true;
                m_CurNumJumps = 0;
            }
        }

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

            RaycastHit2D rHit = SolPhysics.DrawCast(pos, Vector2.up, dist, m_WallMask);

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
        m_Rigidbody.gravityScale = 1.0f;
        m_Rigidbody.velocity *= 0.3f;
    }

    public override void OnHit()
    {
        //Player got hit by something
        Debug.Log("Player got hit");
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (m_IsDash)
        {
            string tag = col.gameObject.tag;
            if (tag != "Enemy" && tag != "Projectile")
                InterruptDash();

            PhysicsEntity pEntity = col.gameObject.GetComponent<PhysicsEntity>();
            if (pEntity)
            {
                pEntity.OnHit();

                DashCooldown cd = GetFirstActiveCooldown();
                if (cd) cd.Reset();
            }
        }
    }

    public void IncreaseCDs()
    {
        ++m_CooldownCharges;
    }

    DashCooldown GetCooldown()
    {
        for (int i = m_Cooldowns.Count - 1; i >= 0; --i)
        {
            if (!m_Cooldowns[i].GetCD()) return m_Cooldowns[i];
        }
        return null;
    }

    DashCooldown GetFirstActiveCooldown()
    {
        Debug.Log("asd");
        for (int i = m_Cooldowns.Count - 1; i >= 0; --i)
        {
            if (m_Cooldowns[i].GetCD()) return m_Cooldowns[i];
        }
        return null;
    }

    bool HasCharges()
    {
        return m_CooldownCharges > 0;
    }

    public ParticleSystem GetDashChargeSystem()
    {
        return m_DashChargeSystem;
    }
}
