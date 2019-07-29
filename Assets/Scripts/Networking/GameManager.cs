using System;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager Instance;

    [Header("User Interface")]
    public GameObject ingamePanel;

    public CustomButton jumpBtn;

    [Header("Map Setup")]
    public GameObject cameraPrefab;
    public GameObject playerPrefab;
    public GameObject spawnObjectPrefab;

    public GameObject hitParticle;

    //Helmets
    public Material[] helmetMaterials;

    private Spawn[] spawnPoints;

    void Awake() {

        Instance = this;

        jumpBtn.ButtonPressed += OnJumpButtonPressed;
    }

    void OnJumpButtonPressed() {
        JumpButton();
    }

    public static bool IsJumpButtonDown()
    {
        return Instance.jumpBtn.isPressedDown;
    }

    public void HitButton()
    {
        if (NetworkManager.LocalPlayer != null)
            NetworkManager.LocalPlayer.pc.Attack();
    }

    public void JumpButton()
    {
        if (NetworkManager.LocalPlayer != null)
            NetworkManager.LocalPlayer.pc.Jump();
    }

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
        PlayerController playerController = newPlayer.GetComponent<PlayerController>();
        playerController.helmetObject.GetComponent<MeshRenderer>().material = helmetMaterials[spawnPoint];
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

        if (NetworkManager.OfflineMode)
        {
            GameObject devPlayer = Instantiate(playerPrefab);
            devPlayer.GetComponent<Rigidbody>().isKinematic = false;
            devPlayer.GetComponent<PlayerController>().debugMode = true;
            devPlayer.GetComponent<NetworkPlayer>().nametag.text = "";
            devPlayer.GetComponent<NetworkPlayer>().isLocal = true;
            devPlayer.transform.position = spawnPoints[0].transform.position;

        }
    }
}
