using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnGoalObjects : MonoBehaviour
{
    private void Start()
    {
        SpawnObjects();
    }

    void SpawnObjects()
    {
        Spawn[] spawnLocation = GetComponentsInChildren<Spawn>();
        foreach (Spawn s in spawnLocation)
        {
            GameObject spawnObjectClone = Instantiate(FindObjectOfType<GameManager>().spawnObjectPrefab, s.transform.position, Quaternion.identity) as GameObject;
        }   
    }
}
