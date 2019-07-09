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

    public GameObject hitParticle;

    private Spawn[] spawnPoints;

    private void OnEnable()
    {
        NetworkManager.CreatePlayer += OnCreatePlayer;
        NetworkManager.MapLoaded += PreparePlayArena;
        NetworkManager.PlayerHit += OnPlayerHit;
        NetworkManager.PlayerFailedHit += OnPlayerFailedHit;
    }

    private void OnPlayerFailedHit(NetworkPlayer source)
    {
        if (source.uid != NetworkManager.UID)
            source.pc.Attack();
    }

    private void OnPlayerHit(NetworkPlayer source, NetworkPlayer target, Vector3 position)
    {
        GameObject particle = Instantiate(hitParticle);
        particle.transform.position = position;
        if (source.uid != NetworkManager.UID)
            source.pc.Attack();

        if (target.uid == NetworkManager.UID)
            target.pc.GetKnockedBack(position);
    }

    private void OnCreatePlayer(uint uid, string username, int spawnPoint)
    {
        GameObject newPlayer = Instantiate(playerPrefab);
        newPlayer.GetComponent<Rigidbody>().isKinematic = NetworkManager.UID == uid ? false : true;
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
        NetworkManager.PlayerHit -= OnPlayerHit;
        NetworkManager.PlayerFailedHit -= OnPlayerFailedHit;
    }

    private void PreparePlayArena()
    {
        GameObject cameraRig = Instantiate(cameraPrefab);
        spawnPoints = FindObjectsOfType<Spawn>();

        ingamePanel.SetActive(true);
    }
}
