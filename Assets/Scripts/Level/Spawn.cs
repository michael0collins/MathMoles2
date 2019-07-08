using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawn : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 0, 0.25f);
        Gizmos.DrawCube(transform.position, Vector3.one);
    }
}
