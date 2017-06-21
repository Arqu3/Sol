using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace SolLib
{
    public class SolPhysics
    {
        public static RaycastHit2D DrawCast(Vector2 position, Vector2 direction, float distance)
        {
            RaycastHit2D hit = Physics2D.Raycast(position, direction.normalized, distance);
            Color col = hit ? Color.green : Color.red;
            Debug.DrawRay(position, direction * distance, col);
            return hit;
        }

        public static RaycastHit2D DrawCast(Vector2 position, Vector2 direction, float distance, int layermask)
        {
            RaycastHit2D hit = Physics2D.Raycast(position, direction.normalized, distance, layermask);
            Color col = hit ? Color.green : Color.red;
            Debug.DrawRay(position, direction * distance, col);
            return hit;
        }
    }

    public class SolGame
    {
        public static UnityEngine.Object SpawnResourceByName(string name, Vector3 pos)
        {
            return UnityEngine.Object.Instantiate(Resources.Load(name, typeof(GameObject)), pos, Quaternion.identity);
        }

        public static UnityEngine.Object SpawnResourceByName(string name, Vector3 pos, float lifetime)
        {
            UnityEngine.Object go = UnityEngine.Object.Instantiate(Resources.Load(name, typeof(GameObject)), pos, Quaternion.identity);
            UnityEngine.Object.Destroy(go, lifetime);
            return go;
        }
    }
}
