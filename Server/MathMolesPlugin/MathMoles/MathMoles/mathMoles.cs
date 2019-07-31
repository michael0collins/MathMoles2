using DarkRift;
using DarkRift.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace MathMoles
{
    public static class GlobalLobbySettings
    {
        public static int LOBBY_START_TIMER
        {
            get { return 5000; }
        }
    }

    public static class NetworkTags
    {
        public static readonly ushort Introduce = 0;
        public static readonly ushort LookingForMatch = 1;

        public static readonly ushort GameSceneLoaded = 11;
        public static readonly ushort CharacterPositionUpdate = 12;
        public static readonly ushort SendHitData = 13;
        public static readonly ushort SendFailedHitData = 14;
        public static readonly ushort SendGoalHitData = 15;

        public static readonly ushort S_Introduced = 100;
        public static readonly ushort S_FoundLobby = 101;
        public static readonly ushort S_UpdateLobbyPlayers = 102;
        public static readonly ushort S_LobbyTimerStarted = 103;

        public static readonly ushort S_LoadGameScene = 110;
        public static readonly ushort S_SpawnPlayers = 111;
        public static readonly ushort S_CharacterPositionUpdate = 112;
        public static readonly ushort S_CharacterHitData = 113;
        public static readonly ushort S_CharacterFailedHitData = 114;
        public static readonly ushort S_SendGoalHitData = 115;
        public static readonly ushort S_PlayerWon = 116;

        public static readonly ushort S_PlayerQuit = 130;

        public static readonly ushort S_NewQuestionGenerated = 151;
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
        public float animationSpeed = 0f;
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

    public struct Answer
    {
        public string message;
        public int health;

        public Answer(string message)
        {
            this.message = message;
            this.health = 10;
        }
    }

    public class Question
    {
        public string message;
        public int correctAnswer;
        public Answer[] answers = new Answer[4];
        public bool questionGenerated = false;

        public void GenerateNewQuestion()
        {
            Random random = new Random();
            Random random_question_number = new Random();
            Random random_answer_number = new Random();

            int messageType = random.Next(0, 3);
            int firstNumber = random_question_number.Next(1, 10);
            int secondNumber = random_question_number.Next(1, 10);
            int correctAnswer = 0;
            switch (messageType)
            {
                case 0:
                    //ADD
                    correctAnswer = firstNumber + secondNumber;
                    message = $"{firstNumber} + {secondNumber} = ?";
                    answers[0] = new Answer($"{correctAnswer}");
                    answers[1] = new Answer($"{random_answer_number.Next(1, 100)}");
                    answers[2] = new Answer($"{random_answer_number.Next(1, 100)}");
                    answers[3] = new Answer($"{random_answer_number.Next(1, 100)}");
                    break;
                case 1:
                    //SUBTRACT
                    correctAnswer = firstNumber - secondNumber;
                    message = $"{firstNumber} - {secondNumber} = ?";
                    answers[0] = new Answer($"{correctAnswer}");
                    answers[1] = new Answer($"{random_answer_number.Next(1, 100)}");
                    answers[2] = new Answer($"{random_answer_number.Next(1, 100)}");
                    answers[3] = new Answer($"{random_answer_number.Next(1, 100)}");
                    break;
                case 2:
                    //MULTIPLY
                    correctAnswer = firstNumber * secondNumber;
                    message = $"{firstNumber} * {secondNumber} = ?";
                    answers[0] = new Answer($"{correctAnswer}");
                    answers[1] = new Answer($"{random_answer_number.Next(1, 100)}");
                    answers[2] = new Answer($"{random_answer_number.Next(1, 100)}");
                    answers[3] = new Answer($"{random_answer_number.Next(1, 100)}");
                    break;
                case 3:
                    //DIVIDE
                    while (firstNumber % secondNumber > 0)
                    {
                        firstNumber = random_question_number.Next(1, 10);
                        secondNumber = random_question_number.Next(1, 10);
                    }
                    correctAnswer = firstNumber % secondNumber;
                    message = $"{firstNumber} / {secondNumber} = ?";
                    answers[0] = new Answer($"{correctAnswer}");
                    answers[1] = new Answer($"{random_answer_number.Next(1, 100)}");
                    answers[2] = new Answer($"{random_answer_number.Next(1, 100)}");
                    answers[3] = new Answer($"{random_answer_number.Next(1, 100)}");
                    break;
            }
            Random rnd = new Random();
            answers = answers.OrderBy(x => rnd.Next()).ToArray();
            questionGenerated = true;
        }
    }

    public class Room
    {
        private Timer startTimer;

        public uint UID;
        public RoomStatus status = RoomStatus.AWAITING_PLAYERS;
        public Dictionary<uint, Player> players = new Dictionary<uint, Player>();

        public Question currentQuestion;

        public int MaxPlayers = 4;
        public int MinPlayers = 2;
        public bool Open = true;

        public int PlayersAmount
        {
            get
            {
                return players.Count;
            }
        }

        public Room(uint uid, int maxPlayers)
        {
            UID = uid;
            MaxPlayers = maxPlayers;
        }

        public void StartGame()
        {
            foreach (Player p in players.Values)
                using (DarkRiftWriter writer = DarkRiftWriter.Create())
                {
                    writer.Write(GlobalLobbySettings.LOBBY_START_TIMER);
                    using (Message message = Message.Create(NetworkTags.S_LobbyTimerStarted, writer))
                        p.Client.SendMessage(message, SendMode.Reliable);
                }

            StartGameTimer();
        }

        private void StartGameTimer()
        {
            startTimer = new Timer(GlobalLobbySettings.LOBBY_START_TIMER);
            startTimer.Elapsed += OnStartTimerElapsed;
            startTimer.AutoReset = false;
            startTimer.Enabled = true;
        }

        private void OnStartTimerElapsed(Object source, ElapsedEventArgs e)
        {
            MathMoles.Log($"Start game timer elapsed for lobby {UID}#UID");
            lock (players)
            {
                Open = false;
                foreach (Player p in players.Values)
                    using (DarkRiftWriter writer = DarkRiftWriter.Create())
                    using (Message message = Message.Create(NetworkTags.S_LoadGameScene, writer))
                        p.Client.SendMessage(message, SendMode.Reliable);

                GenerateQuestion();
            }
        }

        public void CheckForRoundStart()
        {
            lock (players)
            {
                bool allPlayersLoaded = true;
                foreach (Player p in players.Values)
                    if (p.SceneLoaded == false)
                        allPlayersLoaded = false;

                if (allPlayersLoaded && status == RoomStatus.AWAITING_PLAYERS)
                {
                    status = RoomStatus.PLAYING;
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
        }

        public void UpdateLobbyData()
        {
            foreach (Player p in players.Values)
                SendPlayersList(p);
        }

        public void GenerateQuestion()
        {
            currentQuestion = new Question();
            currentQuestion.GenerateNewQuestion();
            while (!currentQuestion.questionGenerated)
            {

            }
            SendCurrentQuestion();
        }

        public void SendCurrentQuestion()
        {
            if (currentQuestion == null)
                return;

            lock (players)
            {
                foreach (Player p in players.Values)
                    using (DarkRiftWriter writer = DarkRiftWriter.Create())
                    {
                        writer.Write(currentQuestion.message);
                        writer.Write(currentQuestion.answers.Length);
                        for (int i = 0; i < currentQuestion.answers.Length; i++)
                            writer.Write(currentQuestion.answers[i].message);

                        using (Message message = Message.Create(NetworkTags.S_NewQuestionGenerated, writer))
                            p.Client.SendMessage(message, SendMode.Unreliable);
                    }
            }
        }

        public void SendPlayerPosition(Player player)
        {
            lock (players)
            {
                foreach (Player p in players.Values)
                    using (DarkRiftWriter writer = DarkRiftWriter.Create())
                    {
                        writer.Write(player.UID);
                        writer.Write(player.Position.x);
                        writer.Write(player.Position.y);
                        writer.Write(player.Position.z);
                        writer.Write(player.Rotation.x);
                        writer.Write(player.Rotation.y);
                        writer.Write(player.Rotation.z);
                        writer.Write(player.animationSpeed);

                        using (Message message = Message.Create(NetworkTags.S_CharacterPositionUpdate, writer))
                            p.Client.SendMessage(message, SendMode.Unreliable);
                    }
            }
        }

        public void SendPlayersList(Player player)
        {
            lock (players)
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
        }

        public void SendPlayerHitData(Player source, Player target, Vec3 position)
        {
            lock (players)
            {
                foreach (Player p in players.Values)
                    using (DarkRiftWriter writer = DarkRiftWriter.Create())
                    {
                        writer.Write(source.UID);
                        writer.Write(target.UID);
                        writer.Write(position.x);
                        writer.Write(position.y);
                        writer.Write(position.z);

                        using (Message message = Message.Create(NetworkTags.S_CharacterHitData, writer))
                            p.Client.SendMessage(message, SendMode.Reliable);
                    }
            }
        }

        public void SendPlayerFailedHitData(Player source)
        {
            lock (players)
            {
                foreach (Player p in players.Values)
                    using (DarkRiftWriter writer = DarkRiftWriter.Create())
                    {
                        writer.Write(source.UID);
                        using (Message message = Message.Create(NetworkTags.S_CharacterFailedHitData, writer))
                            p.Client.SendMessage(message, SendMode.Reliable);
                    }
            }
        }

        public void SendGoalHitData(Player source, int goal)
        {
            currentQuestion.answers[goal].health--;
            if (currentQuestion.answers[goal].health <= 0)
            {
                if (goal == currentQuestion.correctAnswer)
                    foreach (Player p in players.Values)
                        using (DarkRiftWriter writer = DarkRiftWriter.Create())
                        {
                            writer.Write(source.UID);
                            using (Message message = Message.Create(NetworkTags.S_PlayerWon, writer))
                                p.Client.SendMessage(message, SendMode.Reliable);
                        }
                    return;
            }
            else
                foreach (Player p in players.Values)
                    using (DarkRiftWriter writer = DarkRiftWriter.Create())
                    {
                        writer.Write(source.UID);
                        writer.Write(goal);
                        writer.Write(currentQuestion.answers[goal].health);
                        using (Message message = Message.Create(NetworkTags.S_SendGoalHitData, writer))
                            p.Client.SendMessage(message, SendMode.Reliable);
                    }           
        }

        public void RemovePlayer(Player player)
        {
            if (!players.ContainsKey(player.UID) && player.Room != this)
                return;

            if (status == RoomStatus.PLAYING)
                foreach (Player p in players.Values)
                    using (DarkRiftWriter writer = DarkRiftWriter.Create())
                    {
                        writer.Write(player.UID);
                        using (Message message = Message.Create(NetworkTags.S_PlayerQuit, writer))
                            p.Client.SendMessage(message, SendMode.Reliable);
                    }

            players.Remove(player.UID);
            player.Room = null;

            UpdateLobbyData();

            MathMoles.Log($"Player '{player.Username}#{player.UID}' left room #{UID}");

            if (players.Count <= 1)
                MathMoles.DestroyRoom(this);
        }

        public void AddPlayer(Player player)
        {
            lock (players)
            {
                if (players.ContainsKey(player.UID) && player.Room != null)
                    return;

                player.Room = this;
                player.FoundLobby();

                players.Add(player.UID, player);

                UpdateLobbyData();

                MathMoles.Log($"New player '{player.Username}#{player.UID}' joined room #{UID}");

                if (players.Count >= MinPlayers)
                {
                    MathMoles.Log($"Starting countdown for room #{UID}!");
                    StartGame();
                }
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
        public Player GetPlayer(uint client)
        {
            foreach (Player player in players.Values)
                if (player.UID == client)
                    return player;
            return null;
        }


        public Dictionary<uint, Room> rooms = new Dictionary<uint, Room>();

        public Room GetFreeRoomOrCreate()
        {
            lock (Instance.rooms)
            {
                foreach (Room room in rooms.Values)
                {
                    if (room.Open && room.PlayersAmount < room.MaxPlayers)
                        return room;
                }

                return CreateRoom();
            }
        }

        public static void DestroyRoom(Room room)
        {
            lock (Instance.rooms)
            {
                if (!Instance.rooms.ContainsKey(room.UID))
                    return;

                foreach (Player p in room.players.Values)
                    room.RemovePlayer(p);

                Log($"Room #{room.UID} destroyed.");

                Instance.rooms.Remove(room.UID);
            }
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
            lock (players)
            {
                Player player = GetPlayer(e.Client);
                if (player != null)
                {
                    if (player.Room != null)
                        player.Room.RemovePlayer(player);

                    Log($"Player '{player.Username}#{player.UID}' disconnected.");

                    players.Remove(player.UID);
                }
            }
        }

        void ClientMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                if (message.Tag == NetworkTags.Introduce)
                {
                    using (DarkRiftReader reader = message.GetReader())
                    {
                        lock (players)
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
                    lock (rooms)
                    {
                        Player player = GetPlayer(e.Client);
                        player.Status = PlayerStatus.LOOKING_FOR_MATCH;

                        Room freeRoom = GetFreeRoomOrCreate();
                        freeRoom.AddPlayer(player);
                    }
                    return;
                }
                if (message.Tag == NetworkTags.GameSceneLoaded)
                {
                    Player player = GetPlayer(e.Client);
                    player.Status = PlayerStatus.IN_GAME;
                    player.SceneLoaded = true;

                    if (player.Room != null)
                        player.Room.CheckForRoundStart();

                    return;
                }
                if (message.Tag == NetworkTags.SendHitData)
                {
                    Player player = GetPlayer(e.Client);
                    if (player != null)
                    {
                        using (DarkRiftReader reader = message.GetReader())
                        {
                            Vec3 position = new Vec3();
                            uint hitTargetUID = reader.ReadUInt32();
                            position.x = reader.ReadSingle();
                            position.y = reader.ReadSingle();
                            position.z = reader.ReadSingle();
                            Player target = GetPlayer(hitTargetUID);
                            if (player != null)
                            {
                                player.Room.SendPlayerHitData(player, target, position);
                            }
                        }
                    }
                    return;
                }
                if (message.Tag == NetworkTags.SendFailedHitData)
                {
                    Player player = GetPlayer(e.Client);
                    if (player != null)
                        player.Room.SendPlayerFailedHitData(player);

                    return;
                }
                if (message.Tag == NetworkTags.SendGoalHitData)
                {
                    Player player = GetPlayer(e.Client);
                    if (player != null)
                        using (DarkRiftReader reader = message.GetReader())
                        {
                            int answerIndex = reader.ReadInt32();
                            player.Room.SendGoalHitData(player, answerIndex);
                        }

                    return;
                }
                if (message.Tag == NetworkTags.CharacterPositionUpdate)
                {
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
                            player.animationSpeed = reader.ReadSingle();
                            player.Position = position;
                            player.Rotation = rotation;
                            player.Room.SendPlayerPosition(player);
                        }
                    }
                }
            }
        }

        public Room CreateRoom()
        {
            lock (rooms)
            {
                uint roomUID = rnd32Room();
                Room newRoom = new Room(roomUID, maxPlayers: 4);
                rooms.Add(roomUID, newRoom);

                WriteEvent($"New room created! #{roomUID}", LogType.Info);
                return newRoom;
            }
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
