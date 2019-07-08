using DarkRift;
using DarkRift.Server;
using System;
using System.Collections.Generic;

namespace MathMoles
{
    public static class NetworkTags
    {
        public static readonly ushort Introduce = 0;
        public static readonly ushort LookingForMatch = 1;

        public static readonly ushort GameSceneLoaded = 11;
        public static readonly ushort CharacterPositionUpdate = 12;

        public static readonly ushort S_Introduced = 100;
        public static readonly ushort S_FoundLobby = 101;
        public static readonly ushort S_UpdateLobbyPlayers = 102;

        public static readonly ushort S_LoadGameScene = 110;
        public static readonly ushort S_SpawnPlayers = 111;
        public static readonly ushort S_CharacterPositionUpdate = 112;
    }

    public enum PlayerStatus
    {
        IN_MAINMENU,
        LOOKING_FOR_MATCH,
        IN_LOBBY,
        IN_GAME
    }

    public class Player
    {
        public IClient Client;
        public uint UID;
        public string Username;
        public PlayerStatus Status = PlayerStatus.IN_MAINMENU;
        public Room Room = null;

        public Vec3 Position = new Vec3(0, 0, 0);
        public Vec3 Rotation = new Vec3(0, 0, 0);
        public bool SceneLoaded = false;

        public Player(IClient client, uint uid, string username)
        {
            Client = client;
            UID = uid;
            Username = username;
        }

        public void Introduced()
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(UID);
                using (Message message = Message.Create(NetworkTags.S_Introduced, writer))
                    Client.SendMessage(message, SendMode.Reliable);
            }
        }

        public void FoundLobby()
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            using (Message message = Message.Create(NetworkTags.S_FoundLobby, writer))
                Client.SendMessage(message, SendMode.Reliable);
        }
    }

    public enum RoomStatus
    {
        AWAITING_PLAYERS,
        PLAYING
    }

    public class Room
    {
        public uint UID;
        public RoomStatus status = RoomStatus.AWAITING_PLAYERS;
        public Dictionary<uint, Player> players = new Dictionary<uint, Player>();

        public int MaxPlayers = 4;
        public int PlayersAmount
        {
            get
            {
                return players.Count;
            }
        }
        public bool Open = true;

        public Room(uint uid, int maxPlayers)
        {
            UID = uid;
            MaxPlayers = maxPlayers;
        }

        public void StartGame()
        {
            Open = false;
            foreach (Player p in players.Values)
                using (DarkRiftWriter writer = DarkRiftWriter.Create())
                    using (Message message = Message.Create(NetworkTags.S_LoadGameScene, writer))
                        p.Client.SendMessage(message, SendMode.Reliable);
        }

        public void CheckForRoundStart()
        {
            bool allPlayersLoaded = true;
            foreach (Player p in players.Values)
                if (p.SceneLoaded == false)
                    allPlayersLoaded = false;

            if (allPlayersLoaded)
            {
                foreach (Player p in players.Values)
                    using (DarkRiftWriter writer = DarkRiftWriter.Create())
                    {
                        int spawnPoint = 0;
                        writer.Write(players.Values.Count);
                        foreach (Player item in players.Values)
                        {
                            writer.Write(item.UID);
                            writer.Write(item.Username);
                            writer.Write(spawnPoint);
                            spawnPoint++;
                        }
                        using (Message message = Message.Create(NetworkTags.S_SpawnPlayers, writer))
                            p.Client.SendMessage(message, SendMode.Reliable);
                    }

                MathMoles.Log("All players loaded!");
            }
        }

        public void SendPlayerPosition(Player player)
        {
            foreach (Player p in players.Values)
                if (p != player)
                    using (DarkRiftWriter writer = DarkRiftWriter.Create())
                    {
                        writer.Write(player.UID);
                        writer.Write(player.Position.x);
                        writer.Write(player.Position.y);
                        writer.Write(player.Position.z);
                        writer.Write(player.Rotation.x);
                        writer.Write(player.Rotation.y);
                        writer.Write(player.Rotation.z);

                        using (Message message = Message.Create(NetworkTags.S_CharacterPositionUpdate, writer))
                            player.Client.SendMessage(message, SendMode.Reliable);
                    }
        }

        public void SendPlayersList(Player player)
        {
            foreach (Player p in players.Values)
                using (DarkRiftWriter writer = DarkRiftWriter.Create())
                {
                    writer.Write(players.Values.Count);
                    foreach (Player item in players.Values)
                    {
                        writer.Write(item.UID);
                        writer.Write(item.Username);
                    }

                    using (Message message = Message.Create(NetworkTags.S_UpdateLobbyPlayers, writer))
                        player.Client.SendMessage(message, SendMode.Reliable);
                }
        }

        public void UpdateLobbyData()
        {
            foreach (Player p in players.Values)
                SendPlayersList(p);
        }

        public void RemovePlayer(Player player)
        {
            if (!players.ContainsKey(player.UID) && player.Room != this)
                return;

            players.Remove(player.UID);
            player.Room = null;

            UpdateLobbyData();

            MathMoles.Log($"Player '{player.Username}#{player.UID}' left room #{UID}");

            if (players.Count <= 0)
                MathMoles.DestroyRoom(this);
        }

        public void AddPlayer(Player player)
        {
            if (players.ContainsKey(player.UID) && player.Room != null)
                return;

            player.Room = this;
            player.FoundLobby();

            players.Add(player.UID, player);

            UpdateLobbyData();

            MathMoles.Log($"New player '{player.Username}#{player.UID}' joined room #{UID}");

            if (players.Count >= 2)
            {
                MathMoles.Log($"Starting countdown for room #{UID}!");
                StartGame();
            }
        }
    }

    public class MathMoles : Plugin
    {
        private static MathMoles Instance;
        public static void Log(string message, LogType logType = LogType.Info)
        {
            Instance.WriteEvent(message, logType);
        }

        public override Version Version => new Version(1, 0, 0);
        public override bool ThreadSafe => true;

        public Dictionary<uint, Player> players = new Dictionary<uint, Player>();

        public Player GetPlayer(IClient client)
        {
            foreach (Player player in players.Values)
                if (player.Client == client)
                    return player;
            return null;
        }

        public Dictionary<uint, Room> rooms = new Dictionary<uint, Room>();

        public Room GetFreeRoomOrCreate()
        {
            foreach (Room room in rooms.Values)
            {
                if (room.Open && room.PlayersAmount < room.MaxPlayers)
                    return room;
            }

            return CreateRoom();
        }

        public static void DestroyRoom(Room room)
        {
            if (!Instance.rooms.ContainsKey(room.UID))
                return;

            Log($"Room #{room.UID} destroyed.");

            Instance.rooms.Remove(room.UID);
        }

        private static Random playersUID = new Random();
        private static Random roomsUID = new Random();

        public MathMoles(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            Instance = this;

            WriteEvent("MathMoles plugin started!", LogType.Info);
            ClientManager.ClientConnected += ClientConnected;
            ClientManager.ClientDisconnected += ClientDisconnected;
        }

        public override Command[] Commands => new Command[]
        {
            new Command("rooms", "Displays all active rooms.", "rooms", RoomsCommandHandler)
        };

        void RoomsCommandHandler(object sender, CommandEventArgs e)
        {

        }

        void ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            e.Client.MessageReceived += ClientMessageReceived;
        }

        private void ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            Player player = GetPlayer(e.Client);
            if (player != null) {
                if (player.Room != null)
                    player.Room.RemovePlayer(player);

                Log($"Player '{player.Username}#{player.UID}' disconnected.");

                players.Remove(player.UID);
            }
        }

        void ClientMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                if (message.Tag == NetworkTags.Introduce)
                {
                    lock(players)
                    {
                        using (DarkRiftReader reader = message.GetReader())
                        {
                            string username = reader.ReadString();
                            uint playerUID = rnd32Player();
                            Player newPlayer = new Player(e.Client, playerUID, username);
                            newPlayer.Introduced();
                            players.Add(playerUID, newPlayer);
                            WriteEvent($"New player '{username} #{playerUID}' connected!", LogType.Info);           
                        }
                    }
                    return;
                }
                if (message.Tag == NetworkTags.LookingForMatch)
                {
                    Player player = GetPlayer(e.Client);
                    player.Status = PlayerStatus.LOOKING_FOR_MATCH;

                    Room freeRoom = GetFreeRoomOrCreate();
                    freeRoom.AddPlayer(player);
                    return;
                }
                if (message.Tag == NetworkTags.GameSceneLoaded)
                {
                    Player player = GetPlayer(e.Client);
                    player.Status = PlayerStatus.IN_GAME;
                    player.SceneLoaded = true;
                    player.Room.CheckForRoundStart();
                    return;
                }
                if (message.Tag == NetworkTags.CharacterPositionUpdate) {
                    Player player = GetPlayer(e.Client);
                    if (player != null)
                    {
                        using (DarkRiftReader reader = message.GetReader())
                        {
                            Vec3 position = new Vec3();
                            Vec3 rotation = new Vec3();
                            position.x = reader.ReadSingle();
                            position.y = reader.ReadSingle();
                            position.z = reader.ReadSingle();
                            rotation.x = reader.ReadSingle();
                            rotation.y = reader.ReadSingle();
                            rotation.z = reader.ReadSingle();
                            player.Position = position;
                            player.Rotation = rotation;
                            player.Room.SendPlayerPosition(player);
                            WriteEvent($"Player position update from {player.Username}", LogType.Info);
                        }
                    }
                }
            }
        }

        public Room CreateRoom()
        {
            uint roomUID = rnd32Room();
            Room newRoom = new Room(roomUID, maxPlayers: 4);
            rooms.Add(roomUID, newRoom);

            WriteEvent($"New room created! #{roomUID}", LogType.Info);
            return newRoom;
        }

        private static uint rnd32Room()
        {
            return (uint)(roomsUID.Next(1 << 30)) << 2 | (uint)(roomsUID.Next(1 << 2));
        }

        private static uint rnd32Player()
        {
            return (uint)(playersUID.Next(1 << 30)) << 2 | (uint)(playersUID.Next(1 << 2));
        }
    }

    public class Vec3
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }

        public Vec3()
        {

        }

        public Vec3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }
}
