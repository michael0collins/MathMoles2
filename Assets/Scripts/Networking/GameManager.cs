using System;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("User Interface")]
    public GameObject ingamePanel;

    [Header("Map Setup")]
    public GameObject cameraPrefab;
    public GameObject playerPrefab;

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
        networkPlayer.UpdateData(transform.position, transform.rotation.eulerAngles);
        networkPlayer.oldPosition = transform.position;
        networkPlayer.oldRotation = transform.rotation.eulerAngles;
        networkPlayer.nametag.text = username;
        networkPlayer.nametag.color = NetworkManager.UID == uid ? Color.green : Color.white;
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

        ingamePanel.SetActive(true);
    }
}
