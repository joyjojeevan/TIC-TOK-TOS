using TMPro;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    #region Input Fields
    [Header("Panels")]
    public GameObject lobbyPanel;
    public GameObject gamePanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private TMP_Text winText;


    [Header("Player Panels")]
    public Image player1Image;
    public Image player2Image;
    public TMP_Text player1Text;
    public TMP_Text player2Text;
    [SerializeField] private RectTransform player1Panel;
    [SerializeField] private RectTransform player2Panel;

    [Header("Mode Selection Panel")]
    [SerializeField] private GameObject modeSelectionPanel;
    [SerializeField] private GameObject PlayWith;
    [SerializeField] private Button buttonPVP;
    [SerializeField] private Button buttonAI;
    [SerializeField] private Button buttonPVN;

    [Header("Set Name Panel")]
    [SerializeField] private GameObject NamePanel;
    public TMP_InputField playerNameInput;
    [SerializeField] private Button buttonEnter;



    [Header("select difuculty")]
    [SerializeField] private GameObject difficultyPanel;
    [SerializeField] private TMP_Dropdown difficultyDropdown;
    [SerializeField] private Button startGameButton;

    [Header("Buttons")]
    [SerializeField] private Button restartButton;
    [SerializeField] private RectTransform restartButtonAni;
    [SerializeField] private Button changeMode;

    [Header("Back Buttons")]
    [SerializeField] private Button backToPlayWithPanel;
    [SerializeField] private Button backToModePanel;

    [Header("Network Room Setup Panel (Lobby Scene)")]
    [SerializeField] private GameObject networkSetupPanel;
    [SerializeField] private GameObject publicRoomPanel;
    [SerializeField] private GameObject privateRoomPanel;
    [SerializeField] private TMP_Text statusText;
    //[SerializeField] private Button joinRoomButton;

    [Header("Network Room UI")]
    public TMP_Text playerName;
    [SerializeField] private TMP_InputField roomIDInput;
    [SerializeField] private Button quickPlayBtn;
    [SerializeField] private Button createPrivateBtn;
    [SerializeField] private Button joinPrivateBtn;

    [Header("Rematch UI")]
    public Button rematchButton;
    public GameObject rematchPromptPanel;
    public TMP_Text rematchText;

    [Header("Waiting Room UI")]
    public GameObject waitingPanel;
    public TMP_Text roomIDDisplay; 
    public Button cancelSearchBtn;
    public Button masterStartButton;

    [Header("Networking UI")]
    public Button readyButton;
    public TMP_Text p1NameWaiting;
    public TMP_Text p2NameWaiting;
    public TMP_Text p1ReadyStatus; 
    public TMP_Text p2ReadyStatus;

    #endregion
    private Color activeColor = new Color(1f, 1f, 0.5f);
    private Color normalColor = Color.white;
    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        buttonPVP.onClick.AddListener(() => StartLocalGame(GameMode.PlayerVsPlayer));
        buttonAI.onClick.AddListener(ShowAIDifficulty);
        buttonPVN.onClick.AddListener(SetName);
        buttonEnter.onClick.AddListener(ShowNetworkSetup);


        difficultyDropdown.onValueChanged.AddListener(OnDifficultySelected);
        startGameButton.onClick.AddListener(() => StartLocalGame(GameMode.PlayerVsAI));

        quickPlayBtn.onClick.AddListener(() => NetworkManager.Instance.JoinRandomMatch());

        createPrivateBtn.onClick.AddListener(() =>
        {
            NetworkManager.Instance.CreatePrivateRoom(roomIDInput.text);
        });
        joinPrivateBtn.onClick.AddListener(() =>
        {
            NetworkManager.Instance.JoinPrivateRoom(roomIDInput.text);
        });

        backToModePanel.onClick.AddListener(() => OnBackButtonClicked());

        cancelSearchBtn.onClick.AddListener(() => OnCancelButtonClicked());
        rematchButton.onClick.AddListener(() => OnRematchClicked());

        readyButton.onClick.AddListener(() => {
            NetworkManager.Instance.ClickReady();
            readyButton.interactable = false; // Player can't un-ready for now
        });

        masterStartButton.onClick.AddListener(() => {
            NetworkManager.Instance.MasterClickStart();
        });
        // Initially hide the prompt
        if (rematchPromptPanel != null) rematchPromptPanel.SetActive(false);

        ShowLobby();
    }
    private void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Debug.Log("Screen Touched!");
            // If you see this in your mobile log (using Logcat or a screen logger), 
            // the hardware is fine; the UI is just blocking the raycast.
        }
    }
    // Called by NetworkManager when connection and lobby join is complete
    public void OnPhotonLobbyReady()
    {
        //joinRoomButton.interactable = true;
        if (buttonPVN != null)
        {
            buttonPVN.interactable = true;
            ShowStatusMessage("Ready to connect to a room.");
        }
    }
    public void ShowLobby()
    {
        lobbyPanel.SetActive(true);
        gamePanel.SetActive(false);
        modeSelectionPanel.SetActive(true);
        difficultyPanel.SetActive(false);
        networkSetupPanel.SetActive(false);
    }
    #region SetUp PlayerUI
    internal void SetupNetworkPlayerUI()
    {
        string hostName = PhotonNetwork.MasterClient.NickName;

        // Find the guest name safely
        string guestName = "Opponent";
        foreach (Player p in PhotonNetwork.PlayerList)
        {
            if (!p.IsMasterClient) guestName = p.NickName;
        }
        if (NetworkManager.Instance.myPlayer == TicTacPlayer.Player1)
        {
            player1Text.text = "You (X)";
            player2Text.text = "Opponent (O)";
        }
        else
        {
            player1Text.text = "Opponent (X)";
            player2Text.text = "You (O)";
        }
    }
    internal void SetupAIPlayerUI()
    {
        if (GameManager.Instance.aiPlayer == TicTacPlayer.Player1)
        {
            player1Text.text = "AI (X)";
            player2Text.text = "You (O)";
        }
        else
        {
            player1Text.text = "You (X)";
            player2Text.text = "AI (O)";
        }
    }
    #endregion

    #region Menu Navigation
    public void ShowModePanel()
    {
        HideAllMenuPanels();
        modeSelectionPanel.SetActive(true);
    }
    private void ShowAIDifficulty()
    {
        modeSelectionPanel.SetActive(false);
        difficultyPanel.SetActive(true);
        backToPlayWithPanel.onClick.AddListener(ShowModePanel);
    }
    public void ShowWaitingRoom(string roomName)
    {
        HideAllMenuPanels();
        lobbyPanel.SetActive(false);

        waitingPanel.SetActive(true);

        roomIDDisplay.text = "ROOM ID: " + roomName;
        ShowStatusMessage("Waiting for an opponent to join...");
        readyButton.interactable = true;
        masterStartButton.gameObject.SetActive(false);
        UpdateReadyVisuals(false, false);
        //if (PhotonNetwork.IsMasterClient)
        //{
        //    masterStartButton.gameObject.SetActive(true);
        //    statusText.text = "Wait for opponent, then click Start.";
        //}
        //else
        //{
        //    // The Guest cannot see the button
        //    masterStartButton.gameObject.SetActive(false);
        //    statusText.text = "Waiting for Host to start the game...";
        //}
    }
    public void UpdateReadyVisuals(bool p1Ready, bool p2Ready)
    {
        p1ReadyStatus.text = p1Ready ? "P1: READY" : "P1: WAITING...";
        p1ReadyStatus.color = p1Ready ? Color.green : Color.white;

        p2ReadyStatus.text = p2Ready ? "P2: READY" : "P2: WAITING...";
        p2ReadyStatus.color = p2Ready ? Color.green : Color.white;
    }
    private void StartLocalGame(GameMode mode)
    {
        GameManager.Instance.currentMode = mode;
        OpenGamePanel();
        GameManager.Instance.StartNewGame();
    }

    public void OpenGamePanel()
    {
        lobbyPanel.SetActive(false);
        backToModePanel.onClick.AddListener(ShowLobby);
        gamePanel.SetActive(true);
        winPanel.SetActive(false);
    }
    public void SetName()
    {
        NetworkManager.Instance.SetPlayerNickname();
        NamePanel.SetActive(true);
        modeSelectionPanel.SetActive(false);
       // playerName.text = NetworkManager.Instance.SetPlayerNickname();
    }
    public void ShowNetworkSetup()
    {
        NetworkManager.Instance.ConnectToPhoton();
        NamePanel.SetActive(false);
        networkSetupPanel.SetActive(true);
        NetworkManager.Instance.SetPlayerNickname();
    }

    public void ShowStatusMessage(string msg) => statusText.text = msg;
    private void HideAllMenuPanels()
    {
        if (modeSelectionPanel != null) modeSelectionPanel.SetActive(false);
        if (difficultyPanel != null) difficultyPanel.SetActive(false);
        if (networkSetupPanel != null) networkSetupPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
    }
    public void UpdateWaitingRoomNames()
    {
        // Clear the texts first
        p1NameWaiting.text = "Waiting...";
        p2NameWaiting.text = "Waiting...";

        Player[] players = PhotonNetwork.PlayerList;
        // Loop through all players currently in the room
        foreach (Player p in players)
        {
            if (p.IsMasterClient)
            {
                p1NameWaiting.text = p.NickName + " (Host)";
            }
            else
            {
                // If there is a second player, they are the guest
                p2NameWaiting.text = p.NickName + " (Guest)";
            }
        }
    }
    public void OnRematchClicked()
    {
        if (GameManager.Instance.currentMode == GameMode.PlayerVsNetwork)
        {
            NetworkManager.Instance.RequestRematch();
            Animations.Instance.StopGlow();
            rematchButton.interactable = false; // Prevent double clicking
        }
        else
        {
            // Local or AI mode just restarts immediately
            GameManager.Instance.RestartGame();
        }
    }

    public void ShowRematchPrompt(string message)
    {
        if (rematchPromptPanel != null) rematchPromptPanel.SetActive(true);
        rematchText.text = message;
        // Change Rematch button color or text to "Accept?"
        rematchButton.GetComponentInChildren<TMP_Text>().text = "Accept Rematch";
        rematchButton.onClick.RemoveAllListeners();
        rematchButton.onClick.AddListener(() => NetworkManager.Instance.AcceptRematch());
    }

    public void HandleOpponentLeft()
    {
        // Disable the rematch button because there is no one to play with
        rematchButton.interactable = false;
        rematchText.text = "Opponent left the room.";
    }
    public void UpdateRematchStatus(string message)
    {
        if (rematchText != null)
        {
            rematchText.text = message;
            rematchText.color = Color.red; // Optional: make it stand out
        }

        // Disable the rematch button since no one is there to accept
        if (rematchButton != null)
        {
            rematchButton.interactable = false;
        }
    }
    // Inside UIManager.cs
    public void ResetRematchUI()
    {
        rematchButton.interactable = true;
        rematchButton.GetComponentInChildren<TMP_Text>().text = "Rematch";
        if (rematchPromptPanel != null) rematchPromptPanel.SetActive(false);

        // Reset the listener to the original "Request" function
        rematchButton.onClick.RemoveAllListeners();
        rematchButton.onClick.AddListener(() => OnRematchClicked());

        GameManager.Instance.RestartGame();

    }
    private void OnDifficultySelected(int index)
    {
        AIManager.Instance.difficulty = (AIDifficulty)index;
    }
    #endregion

    #region Game State/Update Methods

    public void ShowWaitingForOpponent()
    {
        winPanel.SetActive(true);
        winText.text = "Waiting for Opponent...";
    }
    public void OnBackButtonClicked()
    {
        if (GameManager.Instance.currentMode == GameMode.PlayerVsNetwork)
        {
            NetworkManager.Instance.LeaveRoom();
        }
        else
        {
            ShowLobby();
        }
    }
    private void OnCancelButtonClicked()
    {
            NetworkManager.Instance.LeaveRoom();
            waitingPanel.SetActive(false);
            lobbyPanel.SetActive(true);
        
    }
    public void UpdatePlayerTurn(TicTacPlayer player)
    {
        if (player1Image == null || player2Image == null) return;

        // UI color updates... (existing logic)
        if (player == TicTacPlayer.Player1)
        {
            player1Image.color = activeColor;
            player2Image.color = normalColor;
            Animations.Instance.PlayPopup(player1Panel);
        }
        else
        {
            player2Image.color = activeColor;
            player1Image.color = normalColor;
            Animations.Instance.PlayPopup(player2Panel);
        }
    }

    // ... (ShowWin, ShowDraw, HideWinPanel, RestartGame logic remain the same)
    public void ShowWin(TicTacPlayer player)
    {
        winPanel.SetActive(true);
        string pName = (player == TicTacPlayer.Player1) ? player1Text.text : player2Text.text;
        winText.text = $"{pName} WINS!";
        AudioManager.Instance.PlaySound(SoundType.Win);
        //Animations.Instance.PlayPopup(restartButtonAni);
    }

    public void ShowDraw()
    {
        winPanel.SetActive(true);
        winText.text = "DRAW!";
        AudioManager.Instance.PlaySound(SoundType.Draw);
        //Animations.Instance.PlayPopup(restartButtonAni);
    }

    public void HideWinPanel()
    {
        //GameManager.Instance.RestartGame();
        winPanel.SetActive(false);
        //ResetRematchUI();
    }

    #endregion
}

    //private void OnStartGameClicked()
    //{
    //    difficultyPanel.SetActive(false);
    //    modeSelectionPanel.SetActive(false);
    //    GameManager.Instance.RestartGame();
    //}
    //private void OnDifficultySelected(int index)
    //{
    //    AIManager.Instance.difficulty = (AIDifficulty)index;
    //}
    //private void OnChangeMode()
    //{
    //    winPanel.SetActive(false);
    //    modeSelectionPanel.SetActive(true);
    //    ShowModePanel();
    //}
    //private void OnClickPVP()
    //{
    //    GameManager.Instance.SetGameMode(GameMode.PlayerVsPlayer);
    //    GameManager.Instance.RestartGame();
    //    SceneManager.LoadScene("Game");
    //    modeSelectionPanel.SetActive(false);
    //    startGameButton.gameObject.SetActive(true);
    //}
    //private void OnClickPVAI()
    //{
    //    PlayWith.SetActive(false);
    //    GameManager.Instance.SetGameMode(GameMode.PlayerVsAI);
    //    SceneManager.LoadScene("Game");
    //    difficultyPanel.SetActive(true);
    //    backToPlayWithPanel.onClick.AddListener(ShowPlayWithPanel);
    //    startGameButton.gameObject.SetActive(true);
    //}
    //private void OnClickPVNetwork()
    //{
    //    GameManager.Instance.SetGameMode(GameMode.PlayerVsNetwork);

    //    // Load Photon flow (Lobby / Room scene)
    //    SceneManager.LoadScene("Lobby");
    //}

    //public void UpdatePlayerTurn(TicTacPlayer player)
    //{
    //    // Null checks for UI elements
    //    if (player1Image == null || player2Image == null)
    //    {
    //        Debug.LogWarning("Player images not assigned in UIManager!");
    //        return;
    //    }

    //    if (player == TicTacPlayer.Player1)
    //    {
    //        player1Image.color = activeColor;
    //        player2Image.color = normalColor;
    //        //smoothAnimation.PlayPopup(player1Panel);
    //        //Animations.Instance.PlayPopup(player1Panel);
    //    }
    //    else
    //    {
    //        player2Image.color = activeColor;
    //        player1Image.color = normalColor;
    //        //smoothAnimation.PlayPopup(player2Panel);
    //        //Animations.Instance.PlayPopup(player2Panel);
    //    }

    //    if (modeSelectionPanel != null)
    //    {
    //        modeSelectionPanel.SetActive(false);
    //    }
    //}
    //public void ShowWin(TicTacPlayer player)
    //{
    //    winPanel.SetActive(true);
    //    string pName = (player == TicTacPlayer.Player1) ? player1Text.text: player2Text.text;
    //    winText.text = $"{pName} WINS!";
    //    AudioManager.Instance.PlaySound(SoundType.Win);
    //    Animations.Instance.PlayPopup(restartButtonAni);
    //}

    //public void ShowDraw()
    //{
    //    winPanel.SetActive(true);
    //    winText.text = "DRAW!";
    //    AudioManager.Instance.PlaySound(SoundType.Draw);
    //    Animations.Instance.PlayPopup(restartButtonAni);
    //}

    //public void HideWinPanel()
    //{
    //    winPanel.SetActive(false);
    //}
    //public void ShowModePanel()
    //{
    //    modeSelectionPanel.SetActive(true);
    //    PlayWith.SetActive(true);
    //}
    //public void ShowPlayWithPanel()
    //{
    //    PlayWith.SetActive(true);
    //    difficultyPanel.SetActive(false);

    //}

