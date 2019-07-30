using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public delegate void JoinedLobby();
public delegate void JoiningLobby();
public delegate void CanceledJoiningLobby();
public delegate void AddLobbyPlayer(string username);
public delegate void MapLoadingStarted(string sceneName);
public delegate void MapLoaded();
public delegate void MainMenuLoaded();
public delegate void CreatePlayer(uint uid, string username, int spawnPoint);
public delegate void PlayerHit(NetworkPlayer source, NetworkPlayer target, Vector3 position);
public delegate void PlayerFailedHit(NetworkPlayer source);
public delegate void NewQuestion(string question, string[] answers);
public delegate void ConnectionToMasterFailed(string reason);
public delegate void ConnectionToMasterSuccessfull();

public delegate void PlayerDisconnected(uint uid);

public class LobbyPlayer
{
    public uint UID { get; private set; }
    public string Username { get; private set; }

    public LobbyPlayer(uint uid, string username)
    {
        UID = uid;
        Username = username;
    }
}

[RequireComponent(typeof(UnityClient))]
public class NetworkManager : MonoBehaviour
{
    [Header("Network Interface - Debug")]
    public Text userUID;
    [Space]
    public bool debugMode = true;

    public static string Username = "Guest";
    public static uint UID = 0;
    public static float LoadingProgress { get; private set; } = 0f;
    public static bool OfflineMode { get; private set; } = true;
    public static Dictionary<uint, LobbyPlayer>.ValueCollection LobbyPlayers
    {
        get
        {
            return Instance.lPlayers.Values;
        }
    }
    public static NetworkPlayer LocalPlayer
    {
        get
        {
            Instance.players.TryGetValue(UID, out NetworkPlayer localPlayer);
            if (localPlayer != null)
                return localPlayer;
            else
                return null;
        }
    }

    public static event CanceledJoiningLobby CanceledJoiningLobby;
    public static event JoinedLobby JoinedLobby;
    public static event JoiningLobby JoiningLobby;
    public static event AddLobbyPlayer AddLobbyPlayer;

    public static event MapLoadingStarted MapLoadingStarted;
    public static event MapLoaded MapLoaded;
    public static event MainMenuLoaded MainMenuLoaded;

    public static event CreatePlayer CreatePlayer;
    public static event PlayerHit PlayerHit;
    public static event PlayerFailedHit PlayerFailedHit;
    public static event NewQuestion NewQuestion;

    public static event ConnectionToMasterFailed ConnectionToMasterFailed;
    public static event ConnectionToMasterSuccessfull ConnectionToMasterSuccessfull;

    private static NetworkManager Instance;

    private UnityClient client;

    private Dictionary<uint, LobbyPlayer> lPlayers = new Dictionary<uint, LobbyPlayer>();
    private Dictionary<uint, NetworkPlayer> players = new Dictionary<uint, NetworkPlayer>();

    public static string Question { get; private set; }
    public static string[] Answers { get; private set; }

    void Awake()
    {
        client = GetComponent<UnityClient>();
        Instance = this;

        if (client == null)
        {
            Debug.LogError("Client is not assigned.");
            return;
        }

        client.MessageReceived += Client_MessageReceived;

        StartCoroutine(LoadSceneAsync("MainMenu", true));

        ConnectToMaster();
    }

    void Client_MessageReceived(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage() as Message)
        {
            if (message.Tag == NetworkTags.S_Introduced)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    UID = reader.ReadUInt32();
                    userUID.text = UID.ToString();
                    Debug.Log($"Introduced to master, received client UID: {UID}");
                }
            }
            if (message.Tag == NetworkTags.S_FoundLobby)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    if (JoinedLobby != null)
                        JoinedLobby.Invoke();

                    Debug.Log($"[MATCHMAKER] Found lobby.");
                }
            }
            if (message.Tag == NetworkTags.S_UpdateLobbyPlayers)
            {
                lPlayers = new Dictionary<uint, LobbyPlayer>();
                using (DarkRiftReader reader = message.GetReader())
                {
                    int playersListLength = reader.ReadInt32();
                    for (int i = 0; i < playersListLength; i++)
                    {
                        uint uid = reader.ReadUInt32();
                        string username = reader.ReadString();

                        lPlayers.Add(uid, new LobbyPlayer(uid, username));

                        if (AddLobbyPlayer != null)
                            AddLobbyPlayer.Invoke(username);

                        if (debugMode)
                            Debug.Log($"[MATCHMAKER] [DEBUG] LobbyPlayer '{username}'");
                    }
                }
            }
            if (message.Tag == NetworkTags.S_LoadGameScene)
            {
                StartCoroutine(LoadSceneAsync("TestArtScene"));
            }
            if (message.Tag == NetworkTags.S_CharacterFailedHitData)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    uint suid = reader.ReadUInt32();
                    players.TryGetValue(suid, out NetworkPlayer source);
                    if (source != null)
                        if (PlayerFailedHit != null)
                            PlayerFailedHit.Invoke(source);
                }
            }
            if (message.Tag == NetworkTags.S_CharacterHitData)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    Vector3 position = Vector3.zero;

                    uint suid = reader.ReadUInt32();
                    uint tuid = reader.ReadUInt32();
                    players.TryGetValue(suid, out NetworkPlayer source);
                    players.TryGetValue(tuid, out NetworkPlayer target);
                    if (source != null && target != null)
                    {
                        position.x = reader.ReadSingle();
                        position.y = reader.ReadSingle();
                        position.z = reader.ReadSingle();

                        if (PlayerHit != null)
                            PlayerHit.Invoke(source, target, position);
                    } else
                        if (debugMode)
                            Debug.LogError($"[MATCHMAKER] [DEBUG] S_CharacterHitData source or target player not found!");

                }
            }
            if (message.Tag == NetworkTags.S_SpawnPlayers)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    int playersListLength = reader.ReadInt32();
                    for (int i = 0; i < playersListLength; i++)
                    {
                        uint uid = reader.ReadUInt32();
                        string username = reader.ReadString();
                        int spawnPoint = reader.ReadInt32();

                        if (CreatePlayer != null)
                            CreatePlayer.Invoke(uid, username, spawnPoint);

                        if (debugMode)
                            Debug.Log($"[MATCHMAKER] [DEBUG] Create player '{username}'");
                    }
                }
            }
            if (message.Tag == NetworkTags.S_CharacterPositionUpdate)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    uint uid = reader.ReadUInt32();
                    players.TryGetValue(uid, out NetworkPlayer player);
                    if (player != null)
                    {
                        if (player.uid != UID)
                        {
                            Vector3 position = Vector3.zero;
                            Vector3 rotation = Vector3.zero;
                            position.x = reader.ReadSingle();
                            position.y = reader.ReadSingle();
                            position.z = reader.ReadSingle();
                            rotation.x = reader.ReadSingle();
                            rotation.y = reader.ReadSingle();
                            rotation.z = reader.ReadSingle();
                            player.UpdateData(position, rotation);
                            player.UpdateAnimationSpeed(reader.ReadSingle());
                        }
                    } else {
                        if (debugMode)
                            Debug.Log($"[MATCHMAKER] [DEBUG] Received player update but player doesn't exist #{uid}");
                    }
                }
            }
            if (message.Tag == NetworkTags.S_NewQuestionGenerated)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    string questionMessage = reader.ReadString();
                    int answersLength = reader.ReadInt32();
                    string[] newAnswers = new string[answersLength];
                    for (int i = 0; i < answersLength; i++)
                        newAnswers[i] = reader.ReadString();

                    if (NewQuestion != null)
                        NewQuestion.Invoke(questionMessage, newAnswers);

                    Answers = newAnswers;
                    Question = questionMessage;
                }
            }
            if (message.Tag == NetworkTags.S_PlayerQuit)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    uint uid = reader.ReadUInt32();
                    players.TryGetValue(uid, out NetworkPlayer player);
                    if (player != null)
                    {
                        Debug.Log($"[MATCHMAKER] [DEBUG] Player #{uid} quit.");

                        Destroy(player.gameObject);
                        players.Remove(uid);

                        if (uid == UID)
                        {
                            Debug.Log($"[MATCHMAKER] Loading menu...");
                            StartCoroutine(LoadSceneAsync("MainMenu", true));
                        }
                    }
                    else
                        Debug.Log($"[MATCHMAKER] [DEBUG] Received player quit but player doesn't exist #{uid}");
                }
            }
        }
    }

    IEnumerator LoadSceneAsync(string sceneName, bool isMainMenuScene = false)
    {
        if (debugMode)
            Debug.Log($"[DEBUG] Loading '{sceneName}' ...");

        if (MapLoadingStarted != null)
            MapLoadingStarted.Invoke(sceneName);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        while (!asyncLoad.isDone)
        {
            LoadingProgress = asyncLoad.progress;
            yield return null;
        }

        if (MapLoaded != null && !isMainMenuScene)
            MapLoaded.Invoke();

        if (MainMenuLoaded != null && isMainMenuScene)
            MainMenuLoaded.Invoke();

        if (!isMainMenuScene)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            using (Message message = Message.Create(NetworkTags.GameSceneLoaded, writer))
                client.SendMessage(message, SendMode.Reliable);
        }

        if (debugMode)
            Debug.Log($"[DEBUG] Map '{sceneName}' loaded!");
    }

    public static void RegisterNetworkPlayer(NetworkPlayer player)
    {
        if (Instance.players.ContainsKey(player.uid))
            return;

        Instance.players.Add(player.uid, player);

        if (Instance.debugMode)
            Debug.Log($"[MATCHMAKER] [DEBUG] Registered network player '{player.username}#{player.uid}'");
    }

    public static void SendLocalHitData(NetworkPlayer source, NetworkPlayer target, Vector3 position)
    {
        Debug.Log($"Player '{source.username}#{source.uid}' hit '{target.username}#{target.uid}'");
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write(target.uid);
            writer.Write(position.x);
            writer.Write(position.y);
            writer.Write(position.z);
            using (Message message = Message.Create(NetworkTags.SendHitData, writer))
                Instance.client.SendMessage(message, SendMode.Reliable);
        }
    }

    public static void SendLocalFailedHitData()
    {
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
            using (Message message = Message.Create(NetworkTags.SendFailedHitData, writer))
                Instance.client.SendMessage(message, SendMode.Reliable);
    }

    public static void SendLocalCharacterData(NetworkPlayer player, Vector3 position, Vector3 rotation, float animationSpeed)
    {
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write(position.x);
            writer.Write(position.y);
            writer.Write(position.z);
            writer.Write(rotation.x);
            writer.Write(rotation.y);
            writer.Write(rotation.z);
            writer.Write(animationSpeed);
            using (Message message = Message.Create(NetworkTags.CharacterPositionUpdate, writer))
                Instance.client.SendMessage(message, SendMode.Reliable);
        }
    }

    public void ConnectToMaster()
    {
        if (client.ConnectionState != ConnectionState.Connected)
            client.ConnectInBackground(client.Address, client.Port, client.IPVersion, (System.Exception e) =>
            {
                if (e != null)
                {
                    if (ConnectionToMasterFailed != null)
                        ConnectionToMasterFailed.Invoke(e.Message);

                    OfflineMode = true;
                    userUID.text = "Offline";

                    Debug.LogWarning($"Error occured while connecting to master server.\n'{e.Message}'");
                    return;
                }
                else
                    if (ConnectionToMasterSuccessfull != null)
                        ConnectionToMasterSuccessfull.Invoke();

                OfflineMode = false;
            });

        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write(Username);
            using (Message message = Message.Create(NetworkTags.Introduce, writer))
                client.SendMessage(message, SendMode.Reliable);
        }
    }

    public static void SFindMatch()
    {
        Instance.FindMatch();
    }

    public void FindMatch()
    {
        if (OfflineMode)
        {
            StartCoroutine(LoadSceneAsync("TestArtScene"));
            return;
        }

        if (client.ConnectionState != ConnectionState.Connected)
            ConnectToMaster();

        Debug.Log("[MATCHMAKER] Searching for new match...");

        if (JoiningLobby != null)
            JoiningLobby.Invoke();

        using (DarkRiftWriter writer = DarkRiftWriter.Create())
            using (Message message = Message.Create(NetworkTags.LookingForMatch, writer))
                client.SendMessage(message, SendMode.Reliable);
    }
}
