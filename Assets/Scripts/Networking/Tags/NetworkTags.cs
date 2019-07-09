public static class NetworkTags
{
    public static readonly ushort Introduce = 0;
    public static readonly ushort LookingForMatch = 1;

    public static readonly ushort GameSceneLoaded = 11;
    public static readonly ushort CharacterPositionUpdate = 12;
    public static readonly ushort SendHitData = 13;
    public static readonly ushort SendFailedHitData = 14;

    public static readonly ushort S_Introduced = 100;
    public static readonly ushort S_FoundLobby = 101;
    public static readonly ushort S_UpdateLobbyPlayers = 102;

    public static readonly ushort S_LoadGameScene = 110;
    public static readonly ushort S_SpawnPlayers = 111;
    public static readonly ushort S_CharacterPositionUpdate = 112;
    public static readonly ushort S_CharacterHitData = 113;
    public static readonly ushort S_CharacterFailedHitData = 114;
}