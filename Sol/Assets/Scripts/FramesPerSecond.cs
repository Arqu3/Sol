﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class FramesPerSecond : MonoBehaviour
{
    //Public vars
    [Range(0.01f, 10.0f)]
    public float m_UpdateInterval = 0.5f;

    [Range(0, 5)]
    public int m_NumberOfDecimals = 0;

    //Frame vars
    private float m_Accum = 0.0f;
    private int m_Frames = 0;
    private float m_TimeLeft = 0.0f;

    //Text vars
    private Text m_Text;

	void Start()
    {
        m_Text = GetComponent<Text>();

        if (!m_Text)
        {
            Debug.Log("Frames per second script needs a text component!");
            enabled = false;
            return;
        }

        m_TimeLeft = m_UpdateInterval;
	}
	
	void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) m_Text.enabled = !m_Text.enabled;

        m_TimeLeft -= Time.deltaTime;
        m_Accum += Time.timeScale / Time.deltaTime;
        ++m_Frames;

        if (m_TimeLeft <= 0.0f)
        {
            float fps = m_Accum / m_Frames;

            if (fps < 30)
                m_Text.color = Color.yellow;
            else if (fps < 10)
                m_Text.color = Color.red;
            else
                m_Text.color = Color.green;

            m_Text.text = fps.ToString("F" + m_NumberOfDecimals.ToString());

            m_TimeLeft = m_UpdateInterval;
            m_Accum = 0.0f;
            m_Frames = 0;
        }
	}
}
