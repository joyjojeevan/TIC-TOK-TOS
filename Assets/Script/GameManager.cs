using UnityEngine;
using System.Collections;
public enum TicTacPlayer
{
    None,
    Player1,
    Player2
}
public enum GameMode
{
    PlayerVsPlayer,
    PlayerVsAI
   // PlayerVsNetwork
}
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Cell Basic and Panel)")]
    public Cell[,] gridCells = new Cell[3,3];

    [Header("Sprites")]
    public Sprite xSprite;
    public Sprite oSprite;

    [Header("Mode")]
    public GameMode currentMode = GameMode.PlayerVsPlayer;

    private TicTacPlayer currentPlayer = TicTacPlayer.Player1;
    private int turn = 0;
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
        UIManager.Instance.ShowModePanel();
        UIManager.Instance.UpdatePlayerTurn(currentPlayer);
    }
    public void SetGameMode(GameMode mode)
    {
        currentMode = mode;
    }

    internal void InitGridCells(Cell cell)
    {
        gridCells[cell.cellPos.x, cell.cellPos.y] = cell;
    }
    public TicTacPlayer GetCurrentPlayer()
    {
        return currentPlayer;
    }

    public void NextMove()
    {
        turn++;

        if (turn >= 5)
        {
            if (CheckWinner())
            {
                UIManager.Instance.ShowWin(currentPlayer);
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

        //TO DO : assign AI to players(1/2)
        if (currentMode == GameMode.PlayerVsAI && currentPlayer == TicTacPlayer.Player2)
        {
            StartCoroutine(AIManager.Instance.AIDelay());
        }
    }

    internal int[] BoardToArray()
    {
        int[] board = new int[9];
        int index = 0;

        for (int x = 0; x < 3; x++)
            for (int y = 0; y < 3; y++)
            {
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

    private bool CheckWinner()
    {
        {
            TicTacPlayer p = currentPlayer;

            // Row check
            for (int r = 0; r < 3; r++)
                if (gridCells[r, 0].player == p &&
                    gridCells[r, 1].player == p &&
                    gridCells[r, 2].player == p)
                    return true;

            // Column check
            for (int c = 0; c < 3; c++)
                if (gridCells[0, c].player == p &&
                    gridCells[1, c].player == p &&
                    gridCells[2, c].player == p)
                    return true;

            // Diagonals
            if (gridCells[0, 0].player == p &&
                gridCells[1, 1].player == p &&
                gridCells[2, 2].player == p)
                return true;

            if (gridCells[0, 2].player == p &&
                gridCells[1, 1].player == p &&
                gridCells[2, 0].player == p)
                return true;
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

            return false;
        }

    }

    //TODO :create distroy cell
    internal void RestartGame()
    {

        turn = 0;
        currentPlayer = TicTacPlayer.Player1;

        UIManager.Instance.HideWinPanel();
        UIManager.Instance.UpdatePlayerTurn(currentPlayer);

        foreach (var cell in gridCells)
            {
            Debug.Log("Calling ResetCell on: " + cell.cellPos); 
            cell.ResetCell(); 
        }

        //UIManager.Instance.ShowModePanel();
    }
}
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