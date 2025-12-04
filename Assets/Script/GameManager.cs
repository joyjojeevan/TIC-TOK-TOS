using UnityEngine;

public enum TicTacPlayer
{
    None,
    Player1,
    Player2
}
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Cell Basic and Panel)")]
    [SerializeField] private Cell[,] gridCells = new Cell[3,3];

    public Sprite xSprite;
    public Sprite oSprite;

    private TicTacPlayer currentPlayer = TicTacPlayer.Player1;
    private int turn = 0;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    private void Start()
    {
        UIManager.Instance.UpdatePlayerTurn(currentPlayer);
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

        //if (currentPlayer == TicTacPlayer.Player2 && aiModeEnabled)
        //{
        //    //StartCoroutine(AIMoveDelay());
        //}
}

    //private IEnumerator AIMoveDelay()
    //{
    //    yield return new WaitForSeconds(0.2f);
    //    TriggerAIMove();
    //}
    private bool CheckWinner()
    {
        // Rows
        for (int i = 0; i < gridCells.GetLength(0); i++)
        {
            int count = 0;
            for (int j = 0; j < gridCells.GetLength(1); j++)
            {
                if (gridCells[i, j].player != currentPlayer)
                {
                    break;
                }
                count++;
            }
            if(count == gridCells.GetLength(0))
                return true;
        }
        // Columns
        for (int j = 0; j < gridCells.GetLength(0); j++)
        {
            int count = 0;
            for (int i = 0; i < gridCells.GetLength(0); i++)
            {
                if (gridCells[i, j].player != currentPlayer)
                {
                    break;
                }
                count++;
            }
            if (count == gridCells.GetLength(0))
                return true;
        }

        // Diagonals
        int mainDiagCount = 0;
        for (int i = 0; i < gridCells.GetLength(0); i++)
        {
            if (gridCells[i, i].player != currentPlayer)
            {
                break;
            }
            mainDiagCount++;
        }
        if (mainDiagCount == gridCells.GetLength(0))
            return true;

        int antiDiagCount = 0;
        for (int i = 0; i < gridCells.GetLength(0); i++)
        {
            if (gridCells[i, gridCells.GetLength(0) - 1 - i].player != currentPlayer)
            {
                break;
            }
            antiDiagCount++;
        }
        if (antiDiagCount == gridCells.GetLength(0))
            return true;

        return false;

    }

    internal void RestartGame()
    {

        turn = 0;
        currentPlayer = TicTacPlayer.Player1;

        UIManager.Instance.HideWinPanel();

        foreach (var cell in gridCells)
            {
            Debug.Log("Calling ResetCell on: " + cell.cellPos); 
            cell.ResetCell(); 
        }

    
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