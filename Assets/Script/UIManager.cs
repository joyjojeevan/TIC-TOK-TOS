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

    [Header("select difuculty")]
    [SerializeField] private GameObject difficultyPanel;
    [SerializeField] private TMP_Dropdown difficultyDropdown;
    [SerializeField] private Button startGameButton;

    [Header("Buttons")]
    [SerializeField] private Button restartButton;
    [SerializeField] private RectTransform restartButtonAni;
    [SerializeField] private Button changeMode;

    [Header("Back Buttons")]
    [SerializeField] private Button B1PlayWith;
    [SerializeField] private Button B2ModePanel;

    [Header("Network Room Setup Panel (Lobby Scene)")]
    [SerializeField] private GameObject networkSetupPanel;
    [SerializeField] private TMP_Text statusText; 
    [SerializeField] private Button joinRoomButton;

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
        buttonPVN.onClick.AddListener(ShowNetworkSetup);
        joinRoomButton.onClick.AddListener(() => NetworkManager.Instance.JoinOrCreateRoom());

        difficultyDropdown.onValueChanged.AddListener(OnDifficultySelected);
        startGameButton.onClick.AddListener(() => StartLocalGame(GameMode.PlayerVsAI));

        joinRoomButton.interactable = false; // Disable until we are ready
        joinRoomButton.onClick.AddListener(OnJoinRoomClicked);

        restartButton.onClick.AddListener(GameManager.Instance.RestartGame);
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
        joinRoomButton.interactable = true;
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
        if (NetworkManager.Instance.myPlayer == TicTacPlayer.Player1)
        {
            UIManager.Instance.player1Text.text = "You (X)";
            UIManager.Instance.player2Text.text = "Opponent (O)";
        }
        else
        {
            UIManager.Instance.player1Text.text = "Opponent (X)";
            UIManager.Instance.player2Text.text = "You (O)";
        }
    }
    internal void SetupAIPlayerUI()
    {
        if (GameManager.Instance.aiPlayer == TicTacPlayer.Player1)
        {
            UIManager.Instance.player1Text.text = "AI (X)";
            UIManager.Instance.player2Text.text = "You (O)";
        }
        else
        {
            UIManager.Instance.player1Text.text = "You (X)";
            UIManager.Instance.player2Text.text = "AI (O)";
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
        B1PlayWith.onClick.AddListener(ShowModePanel);
    }

    private void ShowNetworkSetup()
    {
        modeSelectionPanel.SetActive(false);
        networkSetupPanel.SetActive(true);
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
        B2ModePanel.onClick.AddListener(ShowLobby);
        gamePanel.SetActive(true);
        winPanel.SetActive(false);
    }

    public void ShowStatusMessage(string msg) => statusText.text = msg;
    private void HideAllMenuPanels()
    {
        if (modeSelectionPanel != null) modeSelectionPanel.SetActive(false);
        if (difficultyPanel != null) difficultyPanel.SetActive(false);
        if (networkSetupPanel != null) networkSetupPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
    }

    //private void OnClickPVP()
    //{
    //    GameManager.Instance.SetGameMode(GameMode.PlayerVsPlayer);
    //    // We assume GameManager is Persistent, so we just load the scene
    //    SceneManager.LoadScene("Game");
    //}

    //private void OnClickPVAI()
    //{
    //    GameManager.Instance.SetGameMode(GameMode.PlayerVsAI);
    //    SceneManager.LoadScene("Game");
    //    // Difficulty selection is handled in the Game scene UI.
    //    modeSelectionPanel.SetActive(false);
    //    difficultyPanel.SetActive(true);
    //}

    //private void OnClickPVNetwork()
    //{
    //    GameManager.Instance.SetGameMode(GameMode.PlayerVsNetwork);
    //    modeSelectionPanel.SetActive(false);
    //    networkSetupPanel.SetActive(true);
    //    // Show status based on the current Photon state (from OnPhotonLobbyReady)
    //}

    private void OnJoinRoomClicked()
    {
        if (NetworkManager.Instance != null && PhotonNetwork.IsConnectedAndReady)
        {
            NetworkManager.Instance.JoinOrCreateRoom();
            joinRoomButton.interactable = false;
        }
    }

    //private void OnStartGameClicked()
    //{
    //    // Used to start the AI game after difficulty selection
    //    HideAllMenuPanels();
    //    GameManager.Instance.StartNewGame();
    //}

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
        winPanel.SetActive(false);
    }

    #endregion

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
    //    B1PlayWith.onClick.AddListener(ShowPlayWithPanel);
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
}
