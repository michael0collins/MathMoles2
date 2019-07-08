using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnedCharacterPlacement : MonoBehaviour
{
    private void Start()
    {
        AssignPlayerSpawnPositions();
    }

    void AssignPlayerSpawnPositions()
    {
        Spawn[] spawnLocation = FindObjectsOfType<Spawn>();
        PlayerController[] players = FindObjectsOfType<PlayerController>();
        for(int i = 0; i < players.Length; i++)
        {
            if(players[i] != null)
                players[i].transform.position = spawnLocation[i].transform.position;
        }
    }
}
