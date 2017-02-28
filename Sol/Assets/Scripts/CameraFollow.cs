using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    //Public vars
    [Header("Transform to follow")]
    public Transform m_FollowTransform;

    [Header("Raycast mask")]
    public LayerMask m_RayMask;

    //Camera borders
    private List<CameraBorder> m_Borders = new List<CameraBorder>();

    //Position vars
    private bool m_LockedX = false;
    private bool m_LockedY = false;
    private float m_PosX = 0.0f;
    private float m_PosY = 0.0f;

	void Awake()
    {
		if (!m_FollowTransform)
        {
            Debug.LogError("CameraFollow is missing its follow transform!");
            enabled = false;
            return;
        }
        var borders = GetComponentsInChildren<CameraBorder>();
        if (borders.Length > 0)
        {
            for (int i = 0; i < borders.Length; i++)
            {
                m_Borders.Add(borders[i]);
            }

            for (int write = 0; write < m_Borders.Count; write++)
            {
                for (int sort = 0; sort < m_Borders.Count - 1; sort++)
                {
                    if (m_Borders[sort].m_ID > m_Borders[sort + 1].m_ID)
                    {
                        CameraBorder temp = m_Borders[sort];
                        m_Borders[sort] = m_Borders[sort + 1];
                        m_Borders[sort + 1] = temp;
                    }
                }
            }
        }
	}
	
	void Update()
    {
        for (int i = 0; i < m_Borders.Count; i++)
        {
            if (!m_Borders[i].GetColliding() && !CheckPosition(i))
            {
                Vector2 dir = m_Borders[i].transform.position - transform.position;
                bool hit = Physics2D.Raycast(transform.position, dir.normalized, dir.magnitude, m_RayMask);
                Debug.DrawRay(transform.position, dir.normalized * dir.magnitude);
                if (hit)
                    m_Borders[i].SetColliding(true);
            }
        }

        m_LockedX = m_Borders[1].GetColliding() || m_Borders[3].GetColliding();
        m_LockedY = m_Borders[0].GetColliding() || m_Borders[2].GetColliding();

        if (m_LockedX)
            m_PosX = transform.position.x;
        else
            m_PosX = m_FollowTransform.position.x;

        if (m_LockedY)
            m_PosY = transform.position.y;
        else
            m_PosY = m_FollowTransform.position.y;

        if (m_Borders[1].GetColliding() && m_FollowTransform.position.x < transform.position.x)
            m_Borders[1].SetColliding(false);
        else if (m_Borders[3].GetColliding() && m_FollowTransform.position.x > transform.position.x)
            m_Borders[3].SetColliding(false);

        if (m_Borders[0].GetColliding() && m_FollowTransform.position.y < transform.position.y)
            m_Borders[0].SetColliding(false);
        else if (m_Borders[2].GetColliding() && m_FollowTransform.position.y > transform.position.y)
            m_Borders[2].SetColliding(false);

        transform.position = new Vector3(m_PosX, m_PosY, transform.position.z);
	}

    bool CheckPosition(int i)
    {
        switch(i)
        {
            case 0:
                return m_FollowTransform.position.y < transform.position.y;

            case 1:
                return m_FollowTransform.position.x < transform.position.x; 

            case 2:
                return m_FollowTransform.position.y > transform.position.y;

            case 3:
                return m_FollowTransform.position.x > transform.position.x;
        }

        return false;
    }
}
