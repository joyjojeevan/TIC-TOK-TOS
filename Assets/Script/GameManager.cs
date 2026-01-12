using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
using System.ComponentModel;
public enum TicTacPlayer
{
    none,
    Player1,
    Player2
}
public enum GameMode
{
    PlayerVsPlayer,
    PlayerVsAI,
    PlayerVsNetwork
}
public class GameManager : MonoBehaviourPun
{
    public static GameManager Instance;

    [Header("Cell Basic and Panel)")]
    public Cell[,] gridCells = new Cell[3, 3];

    [Header("Sprites")]
    public Sprite xSprite;
    public Sprite oSprite;

    [Header("Mode")]
    public GameMode currentMode = GameMode.PlayerVsPlayer;

    internal TicTacPlayer currentPlayer;
    private int turn = 0;

    public TicTacPlayer aiPlayer;

    public bool IsGameOver = false;

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
        if (SceneManager.GetActiveScene().name != "Game")
            return;

        currentPlayer = TicTacPlayer.Player1; // Always start with Player 1

        if (currentMode == GameMode.PlayerVsNetwork)
        {
            StartNetworkGameFlow();
        }
        else
        {
            StartNewGame();
        }
        if (photonView == null)
        {
            Debug.LogError("GameManager needs a PhotonView component!");
        }
    }
    private void StartNetworkGameFlow()
    {
        if (NetworkManager.Instance == null)
        {
            Debug.LogError("NetworkManager not found for network game mode!");
            return;
        }

        // 1. Assign player roles
        NetworkManager.Instance.AssignPlayerRole();

        // 2. Setup player UI texts (You/Opponent)
        UIManager.Instance.SetupNetworkPlayerUI();

        // 3. Start the game if the room is full
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            StartNewGame();
        }
        else
        {
            // This case should be rare if loading is correct, but safe to handle.
            UIManager.Instance.ShowWaitingForOpponent();
        }
    }
    // TODO ;change to net work maneger and call here

    //public void SetGameMode(GameMode mode)
    //{
    //    currentMode = mode;
    //}

    internal void InitGridCells(Cell cell)
    {
        gridCells[cell.cellPos.x, cell.cellPos.y] = cell;
    }
    public TicTacPlayer GetCurrentPlayer()
    {
        return currentPlayer;
    }

    public void StartNewGame(TicTacPlayer startingPlayer = TicTacPlayer.Player1)
    {
        IsGameOver = false;
        turn = 0;
        //currentPlayer = TicTacPlayer.Player1;
        currentPlayer = startingPlayer;
        ResetBoardOnly();
        UIManager.Instance.HideWinPanel();
        if (currentMode == GameMode.PlayerVsAI)
        {
            aiPlayer = (Random.Range(0, 2) == 0) ? TicTacPlayer.Player1 : TicTacPlayer.Player2;
            UIManager.Instance.SetupAIPlayerUI();
        }

        else if(currentMode == GameMode.PlayerVsPlayer)
        {
            aiPlayer = TicTacPlayer.none;
        }
        Debug.Log(aiPlayer);

        //Animations.Instance.PopupActive = true;
        UIManager.Instance.UpdatePlayerTurn(currentPlayer);

        if (currentPlayer == aiPlayer && currentMode == GameMode.PlayerVsAI)
            StartCoroutine(AIManager.Instance.WaitAndNextMove());
    }
    public void NextMove()
    {
        if (IsGameOver) return;
        //PopupCellsOfCurrentPlayer();
        turn++;
        //IsGameOver = true;
        if (turn >= 5)
        {
            List<Cell> winCells = CheckWinner();
            if (winCells != null)
            {
                IsGameOver = true;
                Animations.Instance.PlayWinAnimation(winCells);

                //UIManager.Instance.ShowWin(currentPlayer);
                HandleGameEnd(currentPlayer);
                if (currentMode == GameMode.PlayerVsNetwork)
                {
                 
                    bool iWon = (currentPlayer == NetworkManager.Instance.myPlayer);
                    //UIManager.Instance.UpdateStats(iWon);
                }
                else if (currentMode == GameMode.PlayerVsAI)
                {
                    bool iWon = (currentPlayer != aiPlayer);
                    //UIManager.Instance.UpdateStats(iWon);
                }
                return;
            }
            else if (turn == 9)
            {
                UIManager.Instance.ShowDraw();
                return;
            }
        }

        currentPlayer = (currentPlayer == TicTacPlayer.Player1) ? TicTacPlayer.Player2 : TicTacPlayer.Player1;
        UIManager.Instance.UpdatePlayerTurn(currentPlayer);

        // check curent player ai or not
        if (currentMode == GameMode.PlayerVsAI && aiPlayer != TicTacPlayer.none)
        {
            if (currentPlayer == aiPlayer)
            {
                StartCoroutine(AIManager.Instance.WaitAndNextMove());
            }
        }

    }
    void HandleGameEnd(TicTacPlayer winner)
    {
        IsGameOver = true;
        UIManager.Instance.ShowWin(winner);

        switch (currentMode)
        {
            case GameMode.PlayerVsAI:
                HandleAIScore(winner);
                break;

            case GameMode.PlayerVsNetwork:
                NetworkManager.Instance.HandleNetworkScore(winner);
                break;

            case GameMode.PlayerVsPlayer:
                // optional: no score or local score
                break;
        }
    }
    void HandleAIScore(TicTacPlayer winner)
    {
        if (winner == aiPlayer)
            ScoreManager.Instance.AddLoss();
        else
            ScoreManager.Instance.AddWin();

        UIManager.Instance.RefreshScoreUI();
    }
    internal int[] ConvertBoardToIntArray()
    {
        int[] board = new int[9];
        int index = 0;

        for (int x = 0; x < 3; x++)
            for (int y = 0; y < 3; y++)
            {
                board[index] = (int)gridCells[x, y].player;
                switch (gridCells[x, y].player)
                {
                    case TicTacPlayer.Player1: board[index] = 1; break;
                    case TicTacPlayer.Player2: board[index] = 2; break;
                    default: board[index] = 0; break;
                }
                index++;
            }
        return board;
    }

    private List<Cell> CheckWinner()
    {
        int[] board = ConvertBoardToIntArray();
        int playerVal = (currentPlayer == TicTacPlayer.Player1) ? 1 : 2;

        int[][] wins =
        {
            new[] {0, 1, 2}, new[] {3, 4, 5}, new[] {6, 7, 8},
            new[] {0, 3, 6}, new[] {1, 4, 7}, new[] {2, 5, 8},
            new[] {0, 4, 8}, new[] {2, 4, 6}
        };

        foreach (var line in wins)
        {
            if (board[line[0]] == playerVal &&
                board[line[1]] == playerVal &&
                board[line[2]] == playerVal)
            {
                List<Cell> winCells = new List<Cell>();

                foreach (int index in line)
                {
                    int x = index / 3;
                    int y = index % 3;
                    winCells.Add(gridCells[x, y]);
                }
                //Animations.Instance.PopupActive = false;
                Animations.Instance.StopContinuousPopup(Animations.Instance.uiObject);
                return winCells;
            }
        }
        return null;
    }
    //TODO :create distroy cell
    internal void RestartGame()
    {
        StartNewGame();
        ResetBoardOnly();
        //turn = 0;
        ////currentPlayer = TicTacPlayer.Player1;

        UIManager.Instance.HideWinPanel();

        foreach (var cell in gridCells)
        {
            cell.ResetCell();
        }
        Animations.Instance.StopGlow();
        UIManager.Instance.confetti.Stop();
        UIManager.Instance.UpdatePlayerTurn(currentPlayer);
        //UIManager.Instance.OpenGamePanel();
    }
    public void BackToLobby()
    {
        if (PhotonNetwork.InRoom) PhotonNetwork.LeaveRoom();
        UIManager.Instance.ShowLobby();
    }

    internal void ResetBoardOnly()
    {
        turn = 0;
        for (int x = 0; x < 3; x++)
            for (int y = 0; y < 3; y++)
                if (gridCells[x, y] != null)
                    gridCells[x, y].ResetCell();
    }
}
//// Row check
//for (int r = 0; r < 3; r++)
//    if (gridCells[r, 0].player == currentPlayer &&
//        gridCells[r, 1].player == currentPlayer &&
//        gridCells[r, 2].player == currentPlayer)
//        return true;

//// Column check
//for (int c = 0; c < 3; c++)
//    if (gridCells[0, c].player == currentPlayer &&
//        gridCells[1, c].player == currentPlayer &&
//        gridCells[2, c].player == currentPlayer)
//        return true;

//// Diagonals
//if (gridCells[0, 0].player == currentPlayer &&
//    gridCells[1, 1].player == currentPlayer &&
//    gridCells[2, 2].player == currentPlayer)
//    return true;

//if (gridCells[0, 2].player == currentPlayer &&
//    gridCells[1, 1].player == currentPlayer &&
//    gridCells[2, 0].player == currentPlayer)
//    return true;


//public List<Cell> GetCellsForPlayer(TicTacPlayer player)
//{
//    int playerValue = (player == TicTacPlayer.Player1) ? 1 : 2;
//    int[] board = ConvertBoardToIntArray();

//    List<Cell> cells = new List<Cell>();

//    for (int i = 0; i < board.Length; i++)
//    {
//        if (board[i] == playerValue)
//        {
//            int x = i / 3;
//            int y = i % 3;
//            cells.Add(gridCells[x, y]);
//        }
//    }

//    return cells;
//}
//// Rows
//for (int i = 0; i < gridCells.GetLength(0); i++)
//{
//    int count = 0;
//    for (int j = 0; j < gridCells.GetLength(1); j++)
//    {
//        if (gridCells[i, j].player != currentPlayer)
//        {
//            break;
//        }
//        count++;
//    }
//    if(count == gridCells.GetLength(0))
//        return true;
//}
//// Columns
//for (int j = 0; j < gridCells.GetLength(0); j++)
//{
//    int count = 0;
//    for (int i = 0; i < gridCells.GetLength(0); i++)
//    {
//        if (gridCells[i, j].player != currentPlayer)
//        {
//            break;
//        }
//        count++;
//    }
//    if (count == gridCells.GetLength(0))
//        return true;
//}

//// Diagonals
//int mainDiagCount = 0;
//for (int i = 0; i < gridCells.GetLength(0); i++)
//{
//    if (gridCells[i, i].player != currentPlayer)
//    {
//        break;
//    }
//    mainDiagCount++;
//}
//if (mainDiagCount == gridCells.GetLength(0))
//    return true;

//int antiDiagCount = 0;
//for (int i = 0; i < gridCells.GetLength(0); i++)
//{
//    if (gridCells[i, gridCells.GetLength(0) - 1 - i].player != currentPlayer)
//    {
//        break;
//    }
//    antiDiagCount++;
//}
//if (antiDiagCount == gridCells.GetLength(0))
//    return true;


//private readonly int[,] winLines =
//{
//    {0,1,2},{3,4,5},{6,7,8},
//    {0,3,6},{1,4,7},{2,5,8},
//    {0,4,8},{2,4,6}
//};

//private bool CheckWinner(TicTacPlayer player)
//{
//    for (int i = 0; i < winLines.GetLength(0); i++)
//    {
//        int a = winLines[i, 0];
//        int b = winLines[i, 1];
//        int c = winLines[i, 2];

//        if (cells[a].player == player &&
//            cells[b].player == player &&
//            cells[c].player == player)
//            return true;
//    }
//    return false;
//}