using TMPro;
using UnityEngine.UI;
using UnityEngine;
using UnityEditor.Rendering.LookDev;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Panels")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private TMP_Text winText;

    [Header("Player Panels")]
    [SerializeField] private Image player1Panel;
    [SerializeField] private Image player2Panel;

    [Header("Mode Selection Panel")]
    [SerializeField] private GameObject modePanel;
    [SerializeField] private GameObject PlayWith;
    [SerializeField] private Button buttonPVP;
    [SerializeField] private Button buttonAI;

    [Header("select difuculty")]
    [SerializeField] private GameObject difficultyPanel;
    [SerializeField] private TMP_Dropdown difficultyDropdown;

    [Header("Buttons")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button changeMode;

    [SerializeField] private Button startGameButton;

    [Header("Back Buttons")]
    [SerializeField] private Button B1PlayWith;
    [SerializeField] private Button B2ModePanel;

    private Color activeColor = new Color(1f, 1f, 0.5f);
    private Color normalColor = Color.white;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
    }
    private void Start()
    {
        modePanel.SetActive(true);
        difficultyPanel.SetActive(false);
        winPanel.SetActive(false);

        restartButton.onClick.AddListener(() => GameManager.Instance.RestartGame());
        changeMode.onClick.AddListener(OnChangeMode);

        buttonPVP.onClick.AddListener(OnClickPVP);
        buttonAI.onClick.AddListener(OnClickPVAI);
        difficultyDropdown.onValueChanged.AddListener(OnDifficultySelected);
        startGameButton.onClick.AddListener(OnStartGameClicked);
    }

    private void OnClickPVP()
    {
        GameManager.Instance.SetGameMode(GameMode.PlayerVsPlayer);
        modePanel.SetActive(false);
        GameManager.Instance.RestartGame();
        startGameButton.gameObject.SetActive(true);
    }

    private void OnStartGameClicked()
    {
        difficultyPanel.SetActive(false);
        modePanel.SetActive(false);
        GameManager.Instance.RestartGame();
        //GameManager.Instance.StartNewGame(); 
    }
    private void OnClickPVAI()
    {
        PlayWith.SetActive(false);
        GameManager.Instance.SetGameMode(GameMode.PlayerVsAI);
        difficultyPanel.SetActive(true);
        B1PlayWith.onClick.AddListener(ShowPlayWithPanel);
        startGameButton.gameObject.SetActive(true);
    }
    private void OnDifficultySelected(int index)
    {
        AIManager.Instance.difficulty = (AIDifficulty)index;
    }
    private void OnChangeMode()
    {
        winPanel.SetActive(false);
        modePanel.SetActive(true);
        ShowModePanel();
    }
    public void UpdatePlayerTurn(TicTacPlayer player)
    {
        if (player == TicTacPlayer.Player1)
        {
            player1Panel.color = activeColor;
            player2Panel.color = normalColor;
        }
        else
        {
            player2Panel.color = activeColor;
            player1Panel.color = normalColor;
        }
        modePanel.SetActive(false);
    }

    public void ShowWin(TicTacPlayer player)
    {
        winPanel.SetActive(true);
        string pName = (player == TicTacPlayer.Player1) ? "Player 1 (X)" : "Player 2 (O)";
        winText.text = pName + " WINS!";
        //B2ModePanel.gameObject.SetActive(false);
    }

    public void ShowDraw()
    {
        winPanel.SetActive(true);
        winText.text = "DRAW!";
        //B2ModePanel.gameObject.SetActive(false);
    }

    public void HideWinPanel()
    {
        winPanel.SetActive(false);
    }
    public void ShowModePanel()
    {
        modePanel.SetActive(true);
        PlayWith.SetActive(true);
    }
    public void ShowPlayWithPanel()
    {
        PlayWith.SetActive(true);
        difficultyPanel.SetActive(false);

    }
}
