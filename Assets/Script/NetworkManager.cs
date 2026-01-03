using Photon.Pun;
using Photon.Realtime;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
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
    // safe disconnect / prevent player
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

    // Quick Play
    #region Quickplay
    //public void JoinRandomMatch()
    //{
    //    //SetPlayerNickname();

    //    PhotonNetwork.JoinRandomRoom();
    //    UIManager.Instance.ShowStatusMessage("Searching for a room...");
    //    //UIManager.Instance.HidePrivateRoomPanel();
    //}

    //public override void OnJoinRandomFailed(short returnCode, string message)
    //{
    //    RoomOptions options = new RoomOptions { MaxPlayers = 2 };
    //    PhotonNetwork.CreateRoom(null, options);
    //    UIManager.Instance.ShowStatusMessage("Waiting for opponent...");
    //}
    #endregion
    // private rooms
    #region Private room
    public void CreatePrivateRoom(string roomID)
    {
        //SetPlayerNickname();

        if (string.IsNullOrEmpty(roomID)) return;
        UIManager.Instance.ShowStatusMessage("Creating private room: " + roomID);
        //UIManager.Instance.HidePublicRoomPanel();
        if (PhotonNetwork.NetworkClientState == ClientState.Joining ||
        PhotonNetwork.NetworkClientState == ClientState.PeerCreated)
        {
            return;
        }
        if (PhotonNetwork.NetworkClientState == ClientState.Joining ||
        PhotonNetwork.InRoom)
        {
            UIManager.Instance.ShowAlert("You are already joining a room!");
            return;
        }
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
        string currentRoomName = PhotonNetwork.CurrentRoom.Name;

        // show the waiting room with this ID
        UIManager.Instance.ShowWaitingRoom(currentRoomName);

        bool isRoomFull = PhotonNetwork.CurrentRoom.PlayerCount == 2;
        UIManager.Instance.SetReadyButtonVisibility(isRoomFull);

        photonView.RPC("RPC_RefreshWaitingNames", RpcTarget.All);

        if (PhotonNetwork.IsMasterClient)
        {
            ResetRoomReadyProperties();
        }

        if (PhotonNetwork.CurrentRoom.PlayerCount < 2)
            UIManager.Instance.ShowStatusMessage("Joined. Waiting for someone to join...");
    }
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        //UIManager.Instance.ShowStatusMessage("Join Failed: Room ID not found.");
        // Common Photon ReturnCodes:
        // GameFull = 32767
        // GameClosed = 32766
        // GameDoesNotExist = 32758

        Debug.Log($"Join Room Failed: {message} (Code: {returnCode})");

        if (returnCode == ErrorCode.GameFull)
        {
            UIManager.Instance.ShowAlert("This room is already full!");
        }
        else if (returnCode == ErrorCode.GameDoesNotExist)
        {
            UIManager.Instance.ShowAlert("Room ID not found. Check the ID and try again.");
        }
        else
        {
            UIManager.Instance.ShowAlert("Could not join room: " + message);
        }
    }
    /*Call from Master,A new player joins ,Total players = 2 ,Host says: “OK, start the game” , Sends RPC to everyone*/
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // When a guest joins, reset both to NOT READY so they start fresh
            ResetRoomReadyProperties();
        }
        // Force a refresh for everyone when a new player walks in
        photonView.RPC("RPC_RefreshWaitingNames", RpcTarget.All);
        // When the 2nd player joins, the Master Client tells everyone to START
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            UIManager.Instance.SetReadyButtonVisibility(true);
            //UIManager.Instance.readyButton.SetActive(true);
            UIManager.Instance.UpdateWaitingRoomNames();
            //photonView.RPC("RPC_StartNetworkGame", RpcTarget.All);
            UIManager.Instance.ShowStatusMessage("Opponent Joined! You can now start.");
        }
    }
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        UIManager.Instance.UpdateWaitingRoomNames();
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        // ErrorCode.ServerFull (32766) or common creation errors
        Debug.Log($"Create Room Failed: {message}");

        if (returnCode == ErrorCode.GameIdAlreadyExists)
        {
            UIManager.Instance.ShowAlert("Room ID already exists! Please choose a different ID or Join this one.");
        }
        else
        {
            UIManager.Instance.ShowAlert("Room Creation Failed: " + message);
        }
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
        name = name.ToUpper();
        PhotonNetwork.NickName = name;
        
        //PhotonNetwork.LocalPlayer.NickName = name;
        UIManager.Instance.playerName.text =" Hi " + name;
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
    void RPC_StartNetworkGame(int startingPlayerIndex)
    {
        GameManager.Instance.currentMode = GameMode.PlayerVsNetwork;

        AssignPlayerRole();
        //random starter
        TicTacPlayer starter = (startingPlayerIndex == 0) ? TicTacPlayer.Player1 : TicTacPlayer.Player2;

        UIManager.Instance.waitingPanel.SetActive(false);
        UIManager.Instance.OpenGamePanel(); // Switch UI panels
        GameManager.Instance.StartNewGame(starter);
        UIManager.Instance.SetupNetworkPlayerUI();
    }
    /* ready state */
    public void ClickReady()
    {
        // Send RPC to everyone telling them I am ready
        //bool amIHost = PhotonNetwork.IsMasterClient;

        //bool currentStatus = amIHost ? isP1Ready : isP2Ready;
        //bool nextStatus = !currentStatus;

        //photonView.RPC("RPC_SetReadyState", RpcTarget.All, amIHost, nextStatus);
        string key = PhotonNetwork.IsMasterClient ? "P1Ready" : "P2Ready";
        bool currentState = false;

        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(key))
        {
            currentState = (bool)PhotonNetwork.CurrentRoom.CustomProperties[key];
        }

        bool nextState = !currentState;

        // Save to Room Properties so everyone (including new joiners) sees it
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
        props.Add(key, nextState);
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
    }
    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        bool p1 = false;
        bool p2 = false;

        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("P1Ready"))
            p1 = (bool)PhotonNetwork.CurrentRoom.CustomProperties["P1Ready"];

        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("P2Ready"))
            p2 = (bool)PhotonNetwork.CurrentRoom.CustomProperties["P2Ready"];

        // Update UI Visuals for everyone
        UIManager.Instance.UpdateReadyVisuals(p1, p2);

        // Update the button text for the LOCAL player only
        bool localReady = PhotonNetwork.IsMasterClient ? p1 : p2;
        UIManager.Instance.UpdateReadyButtonText(localReady);

        // Master Client: Show start button ONLY if both are true
        if (PhotonNetwork.IsMasterClient)
        {
            UIManager.Instance.masterStartButton.gameObject.SetActive(p1 && p2 && PhotonNetwork.CurrentRoom.PlayerCount == 2);
        }
    }

    [PunRPC]
    void RPC_SetReadyState(bool isHost, bool newState)
    {
        if (isHost) isP1Ready = newState;
        else isP2Ready = newState;

        // Update the labels in the UI
        UIManager.Instance.UpdateReadyVisuals(isP1Ready, isP2Ready);
        if (isHost == PhotonNetwork.IsMasterClient)
        {
            UIManager.Instance.UpdateReadyButtonText(newState);
        }
        // Check if we should show the Start Button to the Master
        if (PhotonNetwork.IsMasterClient)
        {
            // Only show Start button if BOTH are ready
            bool bothReady = isP1Ready && isP2Ready;
            bool roomFull = PhotonNetwork.CurrentRoom.PlayerCount == 2;

            UIManager.Instance.masterStartButton.gameObject.SetActive(bothReady && roomFull);
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

    //Call this when the "Rematch" button is clicked
    public void RequestRematch()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount < 2)
        {
            UIManager.Instance.UpdateRematchStatus("Opponent has left. Cannot rematch.");
            return;
        }
        photonView.RPC("RPC_RequestRematch", RpcTarget.Others, PhotonNetwork.NickName);
        UIManager.Instance.ShowStatusMessage("Rematch request sent...");
    }

    [PunRPC]
    void RPC_RequestRematch(string senderName)
    {
        // Show the message to the other player
        UIManager.Instance.ShowRematchPrompt($"{senderName} wants a rematch!");
        Animations.Instance.StopGlow();
    }

    //Call this when the other player accepts the rematch
    public void AcceptRematch()
    {
        photonView.RPC("RPC_SyncRematch", RpcTarget.All);
        Animations.Instance.StopGlow();
    }

    [PunRPC]
    void RPC_SyncRematch()
    {
        UIManager.Instance.rematchPromptPanel.SetActive(false);
        UIManager.Instance.winPanel.SetActive(false);
        // Reset the game for everyone
        GameManager.Instance.StartNewGame();
        UIManager.Instance.ResetRematchUI();
        if (PhotonNetwork.IsMasterClient)
        {
            int nextStarter = UnityEngine.Random.Range(0, 2);
            photonView.RPC("RPC_FinalizeRematch", RpcTarget.All, nextStarter);
        }

        UIManager.Instance.ShowStatusMessage("Rematch Started!");
    }
    [PunRPC]
    void RPC_FinalizeRematch(int starterIndex)
    {
        TicTacPlayer starter = (starterIndex == 0) ? TicTacPlayer.Player1 : TicTacPlayer.Player2;
        GameManager.Instance.StartNewGame(starter);
        UIManager.Instance.ResetRematchUI();
    }

    // Handle when a player leaves the room
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            ResetRoomReadyProperties();
        }
        photonView.RPC("RPC_RefreshWaitingNames", RpcTarget.All);

        UIManager.Instance.SetReadyButtonVisibility(false);

        isP1Ready = false;
        isP2Ready = false;
        //TODO: check curent player  == Other player or not
        UIManager.Instance.ShowStatusMessage(otherPlayer.NickName + " left the game.");
        UIManager.Instance.ShowOpponentLeftPanel(otherPlayer.NickName);

        UIManager.Instance.UpdateWaitingRoomNames();

        //GameManager.Instance.IsGameOver = true;
        //  middle of a game or on the win screen
        if (GameManager.Instance.IsGameOver)
        {
            UIManager.Instance.UpdateRematchStatus("Opponent has left the room.");
            UIManager.Instance.rematchButton.interactable = false;
        }
        else
        {
            UIManager.Instance.ShowStatusMessage("Opponent disconnected.");
        }
        UIManager.Instance.HandleOpponentLeft();
        UIManager.Instance.ResetReadyState();
    }

    // This callback triggers when you successfully leave
    public override void OnLeftRoom()
    {
        isP1Ready = false;
        isP2Ready = false;
        Debug.Log("Left Room Successfully And Reset inputs");

        UIManager.Instance.ResetReadyState();

        UIManager.Instance.ResetInputFields();
        // Show the lobby again when back at the main server
        UIManager.Instance.ShowLobby();
        //UIManager.Instance.OpenGamePanel();
        UIManager.Instance.ResetReadyState();
    }
    private void ResetRoomReadyProperties()
    {
        if (PhotonNetwork.CurrentRoom == null) return;

        ExitGames.Client.Photon.Hashtable resetProps = new ExitGames.Client.Photon.Hashtable
        {
            { "P1Ready", false },
            { "P2Ready", false }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(resetProps);
    }
    public void MasterClickStart()
    {
        // Safety check: Only the Master can start, and only if there are 2 players
        if (PhotonNetwork.IsMasterClient)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
            {
                int startingPlayerIndex = UnityEngine.Random.Range(0, 2);
                photonView.RPC("RPC_StartNetworkGame", RpcTarget.All, startingPlayerIndex);
            }
            else
            {
                UIManager.Instance.ShowStatusMessage("Waiting for an opponent to join...");
            }
        }
    }
    #endregion
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
    //public void CancelSearch()
    //{
    //    // If we are currently trying to join or search, just stop.
    //    // Photon doesn't have a "StopSearch" function, so we simply Leave the Lobby or disconnect.
    //    if (PhotonNetwork.InLobby)
    //    {
    //        // This stops the lobby updates and essentially "cancels" your readiness to join
    //        PhotonNetwork.LeaveLobby();
    //    }

    //    UIManager.Instance.ShowLobby(); // Return to your main menu UI
    //    //UIManager.Instance.ShowStatusMessage("Search Cancelled.");
    //}
    //public void StopSearchAndHost()
    //{
    //    // You don't need to call a "Stop" command; calling CreateRoom 
    //    // will override the previous JoinRandom request.
    //    string customRoomID = UnityEngine.Random.Range(1000, 9999).ToString();

    //    RoomOptions options = new RoomOptions { MaxPlayers = 2, IsVisible = true };
    //    PhotonNetwork.CreateRoom(customRoomID, options);

    //    //UIManager.Instance.ShowStatusMessage("Stopped search. Room Created: " + customRoomID);
    //}

}
//RPC = Remote Procedure Call  -> Call this function on ALL players’ games at the same time.
    // If something must happen on both screens → RPC
    //If local only → normal method
//[PunRPC] -> This function is allowed to be called over the network.
//Master Server ≠ Master Client

//Master Server = Photon backend

//Lobby = where rooms are listed / matched
/*
 * MonoBehaviourPunCallbacks → allows Photon callbacks (OnJoinedRoom, etc.)
 * 
 */