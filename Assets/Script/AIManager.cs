using UnityEngine;
using System.Collections.Generic;

public class AIManager : MonoBehaviour
{
    public static AIManager Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    // Simple AI: choose a random empty cell
    public void MakeAIMove(Cell[,] grid)
    {
        List<Cell> availableCells = new List<Cell>();

        // Find all empty cells
        foreach (var cell in grid)
        {
            if (cell.player == TicTacPlayer.None)
                availableCells.Add(cell);
        }

        if (availableCells.Count == 0)
            return;

        // Pick random empty cell
        Cell aiCell = availableCells[Random.Range(0, availableCells.Count)];

        // Simulate clicking that cell
        aiCell.AI_Click();
    }
}
