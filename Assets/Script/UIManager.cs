using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Panels")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private TMP_Text winText;

    [Header("Player Panels")]
    [SerializeField] private Image player1Panel;
    [SerializeField] private Image player2Panel;

    [Header("Buttons")]
    [SerializeField] private Button restartButton;

    private Color activeColor = new Color(1f, 1f, 0.5f);
    private Color normalColor = Color.white;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    private void Start()
    {
        restartButton.onClick.AddListener(() =>
        {
            GameManager.Instance.RestartGame();
        });
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
}
