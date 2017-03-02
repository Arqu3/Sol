using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class DashCooldown : MonoBehaviour
{
    [Header("Sorting ID")]
    public int m_ID = 0;

    //Component vars
    private Slider m_Slider;
    public Player m_Player;

    //Timer vars
    private float m_Time = 1.0f;
    private float m_Timer = 0.0f;
    private bool m_Cooldown = false;

    void Awake()
    {
        m_Slider = GetComponent<Slider>();
        m_Player = FindObjectOfType<Player>();
    }

    void Update()
    {
        if (m_Cooldown)
        {
            m_Timer += Time.deltaTime;
            m_Slider.value = m_Timer;
            if (m_Timer >= m_Time)
            {
                m_Timer = 0.0f;
                m_Slider.value = m_Time;
                m_Cooldown = false;
                if (m_Player)
                    m_Player.IncreaseCDs();
            }
        }
    }

    public bool GetCD()
    {
        return m_Cooldown;
    }

    public void SetCooldown(bool state)
    {
        m_Cooldown = state;
    }

    public void SetTime(float time)
    {
        m_Time = time;
        m_Slider.maxValue = time;
    }
}
