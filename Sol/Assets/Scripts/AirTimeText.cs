using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class AirTimeText : MonoBehaviour
{

    //Component vars
    private Text m_Text;
    private Player m_Player;

    void Awake()
    {
        m_Text = GetComponent<Text>();
    }

    void Start()
    {
        m_Player = Toolbox.Instance.GetPlayer();
    }

    void Update()
    {
        float timer = m_Player.GetAirTime();

        float minutes = Mathf.Floor(timer / 60.0f);
        float seconds = Mathf.RoundToInt(timer % 60);

        string s = minutes.ToString();
        string ss = seconds.ToString("F0");

        if (minutes < 10) s = "0" + minutes.ToString();
        if (seconds < 10) ss = "0" + seconds.ToString("F0");

        m_Text.text = s + ":" + ss;
    }
}
