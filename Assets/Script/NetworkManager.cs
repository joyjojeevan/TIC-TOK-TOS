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
        // In single scene, we don't need AutomaticallySyncScene = true;
    }

    private void Start() => ConnectToPhoton();

    public void ConnectToPhoton()
    {
        if (!PhotonNetwork.IsConnected) PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster() => PhotonNetwork.JoinLobby();

    public override void OnJoinedLobby() => UIManager.Instance.OnPhotonLobbyReady();

    public void JoinOrCreateRoom()
    {
        PhotonNetwork.JoinRandomRoom();
        UIManager.Instance.ShowStatusMessage("Searching for a room...");
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        RoomOptions options = new RoomOptions { MaxPlayers = 2 };
        PhotonNetwork.CreateRoom(null, options);
        UIManager.Instance.ShowStatusMessage("Waiting for opponent...");
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount < 2)
            UIManager.Instance.ShowStatusMessage("Joined. Waiting for someone to join...");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        // When the 2nd player joins, the Master Client tells everyone to START
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            photonView.RPC("RPC_StartNetworkGame", RpcTarget.All);
        }
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