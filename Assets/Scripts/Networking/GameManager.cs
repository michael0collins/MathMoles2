using System;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Delay/Timing")]
    public float introDelay = 5.0f;
    public float roundDuration = 30.0f;

    [Header("Challenge Question")]
    ChallengeQuestion challengeQuestion;

    public GameObject cameraPrefab;
    public GameObject playerPrefab;

    private Vector3 spawnLocation;

    public static bool IsLocalPlayerFrozen { get; private set; } = true;

    private Spawn[] spawnPoints;

    private void OnEnable()
    {
        NetworkManager.CreatePlayer += OnCreatePlayer;
        NetworkManager.MapLoaded += PreparePlayArena;
    }

    private void OnCreatePlayer(uint uid, string username, int spawnPoint)
    {
        GameObject newPlayer = Instantiate(playerPrefab);
        newPlayer.transform.position = spawnPoints[spawnPoint].transform.position;
        NetworkPlayer networkPlayer = newPlayer.GetComponent<NetworkPlayer>();
        networkPlayer.uid = uid;
        networkPlayer.username = username;
        networkPlayer.isLocal = NetworkManager.UID == uid ? true : false;
        NetworkManager.RegisterNetworkPlayer(networkPlayer);
    }

    private void OnDisable()
    {
        NetworkManager.CreatePlayer -= OnCreatePlayer;
        NetworkManager.MapLoaded -= PreparePlayArena;
    }

    private void PreparePlayArena()
    {
        GameObject cameraRig = Instantiate(cameraPrefab);
        spawnPoints = FindObjectsOfType<Spawn>();
    }
}
