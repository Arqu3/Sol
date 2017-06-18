using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Toolbox singleton-class, use this to set and get any public variables needed,
/// note that no more than 1 instance of this class should be present at any time!
/// </summary>
public class Toolbox : Singleton<Toolbox>
{
    //Make sure constructor cannot be used, true singleton
    protected Toolbox(){}

    private EventManager m_EManager;
    private Player m_Player;

    void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public EventManager GetEventManager()
    {
        if (!m_EManager) return m_EManager = FindObjectOfType<EventManager>();
        else return m_EManager;
    }

    public Player GetPlayer()
    {
        if (!m_Player) return m_Player = FindObjectOfType<Player>();
        else return m_Player;
    }

    static public T RegisterComponent<T>() where T : Component
    {
        return Instance.GetOrAddComponent<T>();
    }
}
