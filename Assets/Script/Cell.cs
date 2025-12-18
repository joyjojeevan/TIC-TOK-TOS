using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class Cell : MonoBehaviourPun
{
    [Header("Set Cell Pos")]
    public Vector2Int cellPos;   

    [Header("UI")]
    public Image cellImage;
    private Button button;

    internal TicTacPlayer player = TicTacPlayer.none;
    private void Awake()
    {
        button = GetComponent<Button>();
    }
    private void Start()
    {
        GameManager.Instance.InitGridCells(this);
        button.onClick.AddListener(OnClickCell);
        // gridPos = new Vector2Int(index / 3, index % 3);
    }
    internal void OnClickCell()
    {

        if (player != TicTacPlayer.none) return;
        if (GameManager.Instance.IsGameOver) return;

        //TicTacPlayer currentPlayer = GameManager.Instance.GetCurrentPlayer();
        if (GameManager.Instance.currentMode == GameMode.PlayerVsNetwork)
        {
            if (NetworkManager.Instance.IsMyTurn)
            {
                // We tell the GameManager to send the move to everyone
                GameManager.Instance.SendNetworkMove(cellPos.x, cellPos.y);
            }
        }
        else if (GameManager.Instance.currentMode == GameMode.PlayerVsAI)
        {
            // Check if it's the AI's turn (should be handled by GameManager but as a fail-safe)
            if (GameManager.Instance.currentMode == GameMode.PlayerVsAI && GameManager.Instance.currentPlayer == GameManager.Instance.aiPlayer)
                return;

            // Execute move directly for local/AI games
            ExecuteMove(GameManager.Instance.GetCurrentPlayer());
        }
        else 
        {
            ExecuteMove(GameManager.Instance.GetCurrentPlayer());
        }
        //player = GameManager.Instance.GetCurrentPlayer();

        //GameManager.Instance.NextMove();
    }
    internal void ExecuteMove(TicTacPlayer p)
    {
        player = p;

        // Update image
        cellImage.sprite = (player == TicTacPlayer.Player1)
            ? GameManager.Instance.xSprite
            : GameManager.Instance.oSprite;

        AudioManager.Instance.PlaySound(SoundType.CellClick);
        Animations.Instance.PlayBounce(GetComponent<RectTransform>());

        GameManager.Instance.NextMove();
    }

    public void ResetCell()
    {
        player = TicTacPlayer.none;
        cellImage.sprite = null;
        cellImage.color = Color.white;
        cellImage.rectTransform.localScale = Vector3.one;
        //GetComponent<RectTransform>().localScale = Vector3.one;
    }
    [PunRPC]
    // The RPC must take the position and the player role
    void NetworkPlayMove(int x, int y, int playerInt)
    {
        // Use the received coordinates to ensure the correct cell processes the RPC
        // Although the RPC is sent from the Cell itself, this is a good safety measure.
        if (this.cellPos.x != x || this.cellPos.y != y) return;

        // Check if the cell is already occupied (shouldn't happen with correct turn sync)
        if (this.player != TicTacPlayer.none) return;

        TicTacPlayer p = (TicTacPlayer)playerInt;

        // Execute the visual and game state update
        ExecuteMove(p);
    }

}