using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SolLib;

public class EventManager : MonoBehaviour
{
    public void OnEnemyDeath(Vector2 position)
    {
        Debug.Log("Event on enemy death");
        SolGame.SpawnResourceByName("EnemyDeathParticles", position, 1.0f);
    }

    public void OnPlayerDashChargeStart()
    {
        Debug.Log("Event on player dash charge start");
        Player player = Toolbox.Instance.GetPlayer();
        GameObject go = (GameObject)SolGame.SpawnResourceByName("PlayerDashChargeParticles", player.transform.position, 1.0f);
        go.transform.SetParent(player.transform);
    }

    public void OnPlayerDashStart(Player player, Vector2 direction)
    {
        Debug.Log("Event on player dash start");
        ParticleSystem system = player.transform.Find("PlayerDashChargeParticles(Clone)").GetComponent<ParticleSystem>();
        if (system) Destroy(system.gameObject);

        GameObject go = (GameObject)SolGame.SpawnResourceByName("PlayerDashParticles", player.transform.position, 1.0f);
        go.transform.rotation = Quaternion.FromToRotation(Vector3.forward, -direction);
        go.transform.SetParent(player.transform);
    }

    public void OnPlayerJump(Vector2 position)
    {
        Debug.Log("Event on player jump");
        GameObject go = (GameObject)SolGame.SpawnResourceByName("PlayerJumpParticles", position, 0.5f);
        go.transform.Rotate(-90.0f, 0.0f, 0.0f);
    }
}
