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
    [SerializeField] private Button buttonPVP;
    [SerializeField] private Button buttonAI;

    [Header("select difuculty")]
    [SerializeField] private GameObject difficultyPanel;
    [SerializeField] private TMP_Dropdown difficultyDropdown;

    [Header("Buttons")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button changeMode;

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
        buttonAI.onClick.AddListener(OnClickAI);
        difficultyDropdown.onValueChanged.AddListener(OnDifficultySelected);
    }

    private void OnClickPVP()
    {
        GameManager.Instance.SetGameMode(GameMode.PlayerVsPlayer);
        modePanel.SetActive(false);
    }
    private void OnClickAI()
    {
        GameManager.Instance.SetGameMode(GameMode.PlayerVsAI);
        difficultyPanel.SetActive(true);
    }
    private void OnDifficultySelected(int index)
    {
        AIManager.Instance.difficulty = (AIDifficulty)index;

        difficultyPanel.SetActive(false);
        modePanel.SetActive(false);

        Debug.Log("Difficulty Selected: " + AIManager.Instance.difficulty);
    }
    private void OnChangeMode()
    {
        GameManager.Instance.RestartGame();
        //modePanel.SetActive(true);
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
    }

    public void ShowDraw()
    {
        winPanel.SetActive(true);
        winText.text = "DRAW!";
    }

    public void HideWinPanel()
    {
        winPanel.SetActive(false);
    }
    public void ShowModePanel()
    {
        modePanel.SetActive(true);
    }
}
