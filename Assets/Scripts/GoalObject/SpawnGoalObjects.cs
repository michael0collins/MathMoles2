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
        //Transform[] spawnLocation = GetComponentsInChildren<Transform>();
        //foreach (Transform t in spawnLocation)
        //{
            //GameObject spawnObjectClone = Instantiate(FindObjectOfType<GameManager>().spawnObjectPrefab, t.position, Quaternion.identity) as GameObject;
        //}   
    }
}
