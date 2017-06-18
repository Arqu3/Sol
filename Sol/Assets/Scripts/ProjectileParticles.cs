using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ProjectileParticles : MonoBehaviour
{

    //Component vars
    private ParticleSystem m_System;

    public void SetColor(Color col)
    {
        m_System = GetComponent<ParticleSystem>();
        var module = m_System.main;
        module.startColor = col;
    }
}
