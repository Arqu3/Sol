using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ProjectileParticles : MonoBehaviour
{

    //Component vars
    private ParticleSystem m_System;

	void Awake()
    {
        m_System = GetComponent<ParticleSystem>();
        Destroy(gameObject, 1.0f);
    }

    public void SetColor(Color col)
    {
        var module = m_System.main;
        module.startColor = col;
    }
}
