using TMPro;
using UnityEngine.UI;
using UnityEngine;
using UnityEditor.Rendering.LookDev;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Panels")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private TMP_Text winText;

    [Header("Player Panels")]
    public Image player1Image;
    public Image player2Image;
    public RectTransform player1Panel;
    public RectTransform player2Panel;
    public TMP_Text player1Text;
    public TMP_Text player2Text;

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
    [SerializeField] private RectTransform restartButtonAni;
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
        GameManager.Instance.RestartGame();

        modePanel.SetActive(false);
        startGameButton.gameObject.SetActive(true);
    }

    private void OnStartGameClicked()
    {
        difficultyPanel.SetActive(false);
        modePanel.SetActive(false);
        GameManager.Instance.RestartGame();
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
            player1Image.color = activeColor;
            player2Image.color = normalColor;
            //smoothAnimation.PlayPopup(player1Panel);
            SmoothAnimation.Instance.PlayPopup(player1Panel);
        }
        else
        {
            player2Image.color = activeColor;
            player1Image.color = normalColor;
            //smoothAnimation.PlayPopup(player2Panel);
            SmoothAnimation.Instance.PlayPopup(player2Panel);
        }
        modePanel.SetActive(false);
    }
    public void ShowWin(TicTacPlayer player)
    {
        winPanel.SetActive(true);
        string pName = (player == TicTacPlayer.Player1) ? player1Text.text: player2Text.text;
        winText.text = $"{pName} WINS!";
        AudioManager.Instance.PlaySound(SoundType.Win);
        SmoothAnimation.Instance.PlayPopup(restartButtonAni);
    }

    public void ShowDraw()
    {
        winPanel.SetActive(true);
        winText.text = "DRAW!";
        AudioManager.Instance.PlaySound(SoundType.Draw);
        SmoothAnimation.Instance.PlayPopup(restartButtonAni);
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
