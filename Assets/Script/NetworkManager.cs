using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance;
    internal TicTacPlayer myPlayer;
    public bool IsMyTurn => GameManager.Instance != null && myPlayer == GameManager.Instance.GetCurrentPlayer();

    private void Awake()
    {
        Instance = this;
    }
    private void OnApplicationQuit()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
    }

    private void Start() => ConnectToPhoton();

    public void ConnectToPhoton()
    {
        if (!PhotonNetwork.IsConnected) PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster() => PhotonNetwork.JoinLobby();

    public override void OnJoinedLobby() => UIManager.Instance.OnPhotonLobbyReady();

    // Quick Play
    public void JoinRandomMatch()
    {
        PhotonNetwork.JoinRandomRoom();
        UIManager.Instance.ShowStatusMessage("Searching for a room...");
        //UIManager.Instance.HidePrivateRoomPanel();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        RoomOptions options = new RoomOptions { MaxPlayers = 2 };
        PhotonNetwork.CreateRoom(null, options);
        UIManager.Instance.ShowStatusMessage("Waiting for opponent...");
    }
    // private rooms
    public void CreatePrivateRoom(string roomID)
    {
        if (string.IsNullOrEmpty(roomID)) return;
        UIManager.Instance.ShowStatusMessage("Creating private room: " + roomID);
        //UIManager.Instance.HidePublicRoomPanel();
        RoomOptions options = new RoomOptions { MaxPlayers = 2, IsVisible = false }; // Hidden from Quick Play
        PhotonNetwork.CreateRoom(roomID, options);
    }

    public void JoinPrivateRoom(string roomID)
    {
        if (string.IsNullOrEmpty(roomID)) return;
        UIManager.Instance.ShowStatusMessage("Joining private room: " + roomID);
        //UIManager.Instance.HidePublicRoomPanel();
        PhotonNetwork.JoinRoom(roomID);
    }
    public override void OnJoinedRoom()
    {
        string roomType = PhotonNetwork.CurrentRoom.IsVisible ? "Public" : "Private";
        UIManager.Instance.ShowStatusMessage($"Joined {roomType} Room: {PhotonNetwork.CurrentRoom.Name}");

        if (PhotonNetwork.CurrentRoom.PlayerCount < 2)
            UIManager.Instance.ShowStatusMessage("Joined. Waiting for someone to join...");
    }
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        UIManager.Instance.ShowStatusMessage("Join Failed: Room ID not found.");
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        // When the 2nd player joins, the Master Client tells everyone to START
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            photonView.RPC("RPC_StartNetworkGame", RpcTarget.All);
        }
    }
    public void LeaveRoom()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
            UIManager.Instance.ShowStatusMessage("Leaving room...");
        }
    }
    public void CancelSearch()
    {
        // If we are currently trying to join or search, just stop.
        // Photon doesn't have a "StopSearch" function, so we simply Leave the Lobby or disconnect.
        if (PhotonNetwork.InLobby)
        {
            // This stops the lobby updates and essentially "cancels" your readiness to join
            PhotonNetwork.LeaveLobby();
        }

        UIManager.Instance.ShowLobby(); // Return to your main menu UI
        //UIManager.Instance.ShowStatusMessage("Search Cancelled.");
    }
    public void StopSearchAndHost()
    {
        // You don't need to call a "Stop" command; calling CreateRoom 
        // will override the previous JoinRandom request.
        string customRoomID = UnityEngine.Random.Range(1000, 9999).ToString();

        RoomOptions options = new RoomOptions { MaxPlayers = 2, IsVisible = true };
        PhotonNetwork.CreateRoom(customRoomID, options);

        //UIManager.Instance.ShowStatusMessage("Stopped search. Room Created: " + customRoomID);
    }

    // This callback triggers when you successfully leave
    public override void OnLeftRoom()
    {
        Debug.Log("Left Room Successfully");
        // Show the lobby again when back at the main server
        UIManager.Instance.ShowLobby();
    }

    // This callback triggers if the OTHER player leaves
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UIManager.Instance.ShowStatusMessage("Opponent left the game.");
        // Optionally: End the game or show a "Win by forfeit" message
    }

    [PunRPC]
    void RPC_StartNetworkGame()
    {
        GameManager.Instance.currentMode = GameMode.PlayerVsNetwork;

        AssignPlayerRole();
        UIManager.Instance.OpenGamePanel(); // Switch UI panels
        GameManager.Instance.StartNewGame();
        UIManager.Instance.SetupNetworkPlayerUI();
    }

    internal void AssignPlayerRole()
    {
        myPlayer = PhotonNetwork.IsMasterClient ? TicTacPlayer.Player1 : TicTacPlayer.Player2;
    }
}