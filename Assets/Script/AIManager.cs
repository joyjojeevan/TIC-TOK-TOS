using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public enum AIDifficulty
{
    Easy,
    Medium,
    Hard
}

public class AIManager : MonoBehaviour
{

    private const int Player = 1;
    private const int Player_AI = 2;

    private float delayTime = 0.4f;

    public static AIManager Instance;

    public AIDifficulty difficulty = AIDifficulty.Easy;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }


    public int GetAIMove(int[] board)
    {
        switch (difficulty)
        {
            case AIDifficulty.Easy: return GetMove_Easy(board);
            case AIDifficulty.Medium: return GetMove_Minimax(board ,3);
            case AIDifficulty.Hard: return GetMove_Minimax(board, 9);
        }

        return -1;
    }
    internal IEnumerator WaitAndNextMove()
    {
        // add veriable for time
        yield return new WaitForSeconds(delayTime);

        if (GameManager.Instance.GetCurrentPlayer() != GameManager.Instance.aiPlayer)
            yield break;

        int[] board = GameManager.Instance.ConvertBoardToIntArray();

        bool boardEmpty = true;
        for (int i = 0; i < 9; i++)
        {
            if (board[i] != 0) { boardEmpty = false; break; }
        }

        int moveIndex;
        //if (boardEmpty)
        //{
        //    // pick any empty cell at random
        //    List<int> empty = new List<int>();
        //    for (int i = 0; i < 9; i++) if (board[i] == 0) empty.Add(i);
        //    moveIndex = empty[UnityEngine.Random.Range(0, empty.Count)];

        //    //center
        //    //empty.Add(4);
        //}
        //else
        //{
        //    moveIndex = GetAIMove(board);
        //}

        moveIndex = GetAIMove(board);
        if (moveIndex >= 0)
        {
            int x = moveIndex / 3;
            int y = moveIndex % 3;
            GameManager.Instance.gridCells[x, y].OnClickCell();
        }
    }
    #region AI_Easy
    public int GetMove_Easy(int[] board)
    {
        List<int> empty = new List<int>();

        for (int i = 0; i < 9; i++)
            if (board[i] == 0)
                empty.Add(i);

        if (empty.Count == 0)
            return -1;

        return empty[UnityEngine.Random.Range(0, empty.Count)];
    }
    #endregion
    //get best move from minimax logic
    private int GetMove_Minimax(int[] boardVal, int maxDepth)
    {
        int bestVal = int.MinValue;
        int bestMove = -1;

        for (int i = 0; i < 9; i++)
        {
            if (boardVal[i] == 0)
            {
                boardVal[i] = Player_AI;
                int moveVal = Minimax_AlphaBeta(boardVal, 0, false , maxDepth, int.MinValue, int.MaxValue);
                boardVal[i] = 0;
                Debug.Log("position :" + i + "move val" +moveVal);
                if (moveVal > bestVal)
                {
                    bestVal = moveVal;
                    bestMove = i;
                }
            }
        }

        return bestMove;
    }
    //Minmax recursive logic 
    private int Minimax_AlphaBeta(int[] boardVal, int depth, bool turn, int maxDepth, int alpha, int beta)
    {
        int score = EvaluateBoard(boardVal);

        if (score == 10) return score - depth;
        if (score == -10) return score + depth;
        if (!MovesLeft(boardVal)) return 0;

        if (maxDepth >= 0 && maxDepth <= depth)
            return 0;

        if (turn)
        {
            int best = int.MinValue;

            for (int i = 0; i < 9; i++)
            {
                if (boardVal[i] == 0)
                {
                    boardVal[i] = Player_AI;
                    int val = Minimax_AlphaBeta(boardVal, depth + 1, false , maxDepth,alpha ,beta);
                    boardVal[i] = 0;
                    best = Math.Max(best, val);
                    alpha = Math.Max(alpha, best);

                    if (beta <= alpha) break;
                }
            }

            return best;
        }
        else
        {
            int best = int.MaxValue;

            for (int i = 0; i < 9; i++)
            {
                if (boardVal[i] == 0)
                {
                    boardVal[i] = Player;
                    int val = Minimax_AlphaBeta(boardVal, depth + 1, true, maxDepth, alpha, beta);
                    boardVal[i] = 0;
                    best = Math.Min(best, val);
                    beta = Math.Min(beta, best);

                    if (beta <= alpha) break;
                }
            }

            return best;
        }
    }
    // find left moves
    private bool MovesLeft(int[] boardVal)
    {
        for (int i = 0; i < 9; i++)
            if (boardVal[i] == 0) return true;

        return false;
    }

    //evaluate win
    private int EvaluateBoard(int[] boardVal)
    {
        //todo: same as change game manager 
        int[][] wins =
        {
            new[] {0,1,2}, new[] {3,4,5}, new[] {6,7,8},
            new[] {0,3,6}, new[] {1,4,7}, new[] {2,5,8},
            new[] {0,4,8}, new[] {2,4,6}
        };

        foreach (var win in wins)
        {
            if (boardVal[win[0]] == boardVal[win[1]] && boardVal[win[1]] == boardVal[win[2]])
            {
                if (boardVal[win[0]] == Player_AI) return 10;
                if (boardVal[win[0]] == Player) return -10;
            }
        }

        return 0;
    }
}
//#region AI_Medium
//private int GetMove_Medium(int[] board)
//{
//    int bestVal = int.MinValue;
//    int bestMove = -1;

//    for (int i = 0; i < 9; i++)
//    {
//        if (board[i] == 0)
//        {
//            board[i] = Player_AI;

//            int moveVal = Minimax_Medium(board, 0, false, maxDepth: 2);

//            board[i] = 0;

//            if (moveVal > bestVal)
//            {
//                bestVal = moveVal;
//                bestMove = i;
//            }
//        }
//    }
//    return bestMove;
//}

//private int Minimax_Medium(int[] board, int depth, bool isMaxTurn, int maxDepth)
//{
//    int score = EvaluateBoard(board);

//    if (score == 10) return score - depth;
//    if (score == -10) return score + depth;
//    if (!MovesLeft(board)) return 0;

//    // ⭐ LIMIT DEPTH
//    if (depth >= maxDepth)
//        return 0;

//    if (isMaxTurn)
//    {
//        int best = int.MinValue;

//        for (int i = 0; i < 9; i++)
//        {
//            if (board[i] == 0)
//            {
//                board[i] = Player_AI;

//                best = Math.Max(best,
//                    Minimax_Medium(board, depth + 1, false, maxDepth));

//                board[i] = 0;
//            }
//        }

//        return best;
//    }
//    else
//    {
//        int best = int.MaxValue;

//        for (int i = 0; i < 9; i++)
//        {
//            if (board[i] == 0)
//            {
//                board[i] = Player;

//                best = Math.Min(best,
//                    Minimax_Medium(board, depth + 1, true, maxDepth));

//                board[i] = 0;
//            }
//        }

//        return best;
//    }
//}
//#endregion

// minimax logic
//private IEnumerator AIDelay()
//{
//    yield return new WaitForSeconds(0.2f);

//    int[] board = BoardToArray();
//    int bestMove = GetBestMove(board);

//    ApplyAIMove(bestMove);
//}

//private int[] BoardToArray()
//{
//    int[] b = new int[9];
//    int index = 0;

//    for (int x = 0; x < 3; x++)
//        for (int y = 0; y < 3; y++)
//        {
//            switch (GameManager.Instance.gridCells[x, y].player)
//            {
//                case TicTacPlayer.Player1: b[index] = 1; break;
//                case TicTacPlayer.Player2: b[index] = 2; break;
//                default: b[index] = 0; break;
//            }
//            index++;
//        }
//    return b;
//}



//public class AIManager : MonoBehaviour
//{
//    public static AIManager Instance;

//    private void Awake()
//    {
//        if (Instance == null)
//        {
//            Instance = this;
//            DontDestroyOnLoad(gameObject);
//        }
//        else
//        {
//            Destroy(gameObject);
//        }
//    }

//    public void MakeAIMove(Cell[,] grid)
//    {
//        List<Cell> available = new List<Cell>();

//        foreach (var cell in grid)
//            if (cell.player == TicTacPlayer.None)
//                available.Add(cell);

//        if (available.Count == 0)
//            return;

//        Cell move = available[Random.Range(0, available.Count)];

//        move.AI_Click();
//    }
//    internal IEnumerator AIDelay()
//    {
//        yield return new WaitForSeconds(0.4f);

//        if (AIManager.Instance == null)
//        {
//            yield break;
//        }

//        MakeAIMove(GameManager.Instance.gridCells);

//    }
//}
