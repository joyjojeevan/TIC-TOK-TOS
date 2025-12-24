using Photon.Pun;
using Photon.Realtime;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Timeline;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance;
    internal TicTacPlayer myPlayer;

    private bool isP1Ready = false;
    private bool isP2Ready = false;
    public bool IsMyTurn => GameManager.Instance != null && myPlayer == GameManager.Instance.GetCurrentPlayer();

    private void Awake()
    {
        Instance = this;
    }
    // safe disconnect
    private void OnApplicationQuit()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
    }

    //private void Start() => ConnectToPhoton();
    
    public void ConnectToPhoton()
    {
        if (!PhotonNetwork.IsConnected) PhotonNetwork.ConnectUsingSettings();
    }

    /* *Connected to master * */
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }
    /* *Join lobby* */
    public override void OnJoinedLobby() => UIManager.Instance.OnPhotonLobbyReady();
    /* Game Logic */
    #region network logic 
    //broadcasts the move to ALL players
    public void SendNetworkMove(int x, int y)
    {
        // This sends the instruction to EVERYONE in the room
        photonView.RPC("RPC_SyncMove", RpcTarget.All, x, y, (int)GameManager.Instance.currentPlayer);
    }
    //resive the moves
    [PunRPC]
    public void RPC_SyncMove(int x, int y, int playerInt)
    {
        TicTacPlayer Player = (TicTacPlayer)playerInt;
        // Find the cell at X, Y and execute the move visually
        GameManager.Instance.gridCells[x, y].ExecuteMove(Player);
        Debug.Log($"Network Move Received: {x},{y} by Player {Player}");
    }
    #endregion

    // Quick Play
    #region Quickplay
    public void JoinRandomMatch()
    {
        //SetPlayerNickname();

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
    #endregion
    // private rooms
    #region Private room
    public void CreatePrivateRoom(string roomID)
    {
        //SetPlayerNickname();

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
    #endregion
    #region Room Management
    /* waiting room logic */
    public override void OnJoinedRoom()
    { 
        // Get the name of the room we just joined (Random or Private)
        string currentRoomName = PhotonNetwork.CurrentRoom.Name;

        // Tell the UI to show the waiting room with this ID
        UIManager.Instance.ShowWaitingRoom(currentRoomName);

        photonView.RPC("RPC_RefreshWaitingNames", RpcTarget.All);

        if (PhotonNetwork.CurrentRoom.PlayerCount < 2)
            UIManager.Instance.ShowStatusMessage("Joined. Waiting for someone to join...");
    }
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        UIManager.Instance.ShowStatusMessage("Join Failed: Room ID not found.");
    }
    /*Call from Master,A new player joins ,Total players = 2 ,Host says: “OK, start the game” , Sends RPC to everyone*/
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        // Force a refresh for everyone when a new player walks in
        photonView.RPC("RPC_RefreshWaitingNames", RpcTarget.All);
        // When the 2nd player joins, the Master Client tells everyone to START
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            UIManager.Instance.UpdateWaitingRoomNames();
            //photonView.RPC("RPC_StartNetworkGame", RpcTarget.All);
            UIManager.Instance.ShowStatusMessage("Opponent Joined! You can now start.");
        }
    }
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        UIManager.Instance.UpdateWaitingRoomNames();
    }
    /* *** Set player name ****/
    public void SetPlayerNickname()
    {
        string name = UIManager.Instance.playerNameInput.text;
        if (string.IsNullOrEmpty(name))
        {
            // Default name if they leave it empty
            name = "Player_" + Random.Range(100, 999);
        }
        PhotonNetwork.NickName = name;
        
        //PhotonNetwork.LocalPlayer.NickName = name;
        UIManager.Instance.playerName.text = name;
    }
    [PunRPC]
    void RPC_RefreshWaitingNames()
    {
        UIManager.Instance.UpdateWaitingRoomNames();
    }

    /*Assign Player Role (X or O) */
    internal void AssignPlayerRole()
    {
        myPlayer = PhotonNetwork.IsMasterClient ? TicTacPlayer.Player1 : TicTacPlayer.Player2;
    }
    //It runs on both players
    [PunRPC]
    void RPC_StartNetworkGame()
    {
        GameManager.Instance.currentMode = GameMode.PlayerVsNetwork;

        AssignPlayerRole();
        UIManager.Instance.waitingPanel.SetActive(false);
        UIManager.Instance.OpenGamePanel(); // Switch UI panels
        GameManager.Instance.StartNewGame();
        UIManager.Instance.SetupNetworkPlayerUI();
    }
    /* ready state */
    public void ClickReady()
    {
        // Send RPC to everyone telling them I am ready
        bool amIHost = PhotonNetwork.IsMasterClient;
        photonView.RPC("RPC_SetReadyState", RpcTarget.All, amIHost);
    }

    [PunRPC]
    void RPC_SetReadyState(bool isHost)
    {
        if (isHost) isP1Ready = true;
        else isP2Ready = true;

        // Update the labels in the UI
        UIManager.Instance.UpdateReadyVisuals(isP1Ready, isP2Ready);

        // Check if we should show the Start Button to the Master
        if (PhotonNetwork.IsMasterClient)
        {
            // Only show Start button if BOTH are ready
            UIManager.Instance.masterStartButton.gameObject.SetActive(isP1Ready && isP2Ready);
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
    // 1. Call this when the "Rematch" button is clicked
    public void RequestRematch()
    {
        photonView.RPC("RPC_RequestRematch", RpcTarget.Others);
        UIManager.Instance.ShowStatusMessage("Rematch request sent...");
    }

    [PunRPC]
    void RPC_RequestRematch()
    {
        // Show the message to the other player
        UIManager.Instance.ShowRematchPrompt("Opponent wants to play again!");
        Animations.Instance.StopGlow();
    }

    // 2. Call this when the other player accepts the rematch
    public void AcceptRematch()
    {
        photonView.RPC("RPC_SyncRematch", RpcTarget.All);
        Animations.Instance.StopGlow();
    }

    [PunRPC]
    void RPC_SyncRematch()
    {
        // Reset the game for everyone
        GameManager.Instance.StartNewGame();
    }


    // 3. Handle when a player leaves the room
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        isP1Ready = false;
        isP2Ready = false;

        UIManager.Instance.UpdateWaitingRoomNames();
        // If we are in the middle of a game or on the win screen
        if (GameManager.Instance.IsGameOver)
        {
            // Update the specific rematch text you requested
            UIManager.Instance.UpdateRematchStatus("Opponent has left the room.");
        }
        else
        {
            // If they left during the game, you can also show it here
            UIManager.Instance.ShowStatusMessage("Opponent disconnected.");
        }
        UIManager.Instance.HandleOpponentLeft();
    }

    // This callback triggers when you successfully leave
    public override void OnLeftRoom()
    {
        Debug.Log("Left Room Successfully");
        // Show the lobby again when back at the main server
        UIManager.Instance.ShowLobby();
    }

    public void MasterClickStart()
    {
        // Safety check: Only the Master can start, and only if there are 2 players
        if (PhotonNetwork.IsMasterClient)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
            {
                photonView.RPC("RPC_StartNetworkGame", RpcTarget.All);
            }
            else
            {
                UIManager.Instance.ShowStatusMessage("Waiting for an opponent to join...");
            }
        }
    }
    #endregion
}
//RPC = Remote Procedure Call  -> Call this function on ALL players’ games at the same time.
//[PunRPC] -> This function is allowed to be called over the network.
//Master Server ≠ Master Client

//Master Server = Photon backend

//Lobby = where rooms are listed / matched