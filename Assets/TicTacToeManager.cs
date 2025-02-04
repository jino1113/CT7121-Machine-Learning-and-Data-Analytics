using UnityEngine;
using UnityEngine.UI;

public class TicTacToeManager : MonoBehaviour
{
    public Button[,] gridButtons = new Button[3, 3]; // Reference to UI buttons (if using UI)
    public int[,] board = new int[3, 3]; // 0 = empty, 1 = player, -1 = AI
    public bool isPlayerTurn = true; // Player starts first

    void Start()
    {
        InitializeGrid();
    }

    void InitializeGrid()
    {
        // Assign the gridButtons based on their names in the scene
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                string buttonName = $"Cell_{i}_{j}";
                gridButtons[i, j] = GameObject.Find(buttonName).GetComponent<Button>();

                int x = i, y = j; // Capture variables for the closure
                gridButtons[i, j].onClick.AddListener(() => PlayerMove(x, y));
            }
        }
    }

    void PlayerMove(int x, int y)
    {
        if (!isPlayerTurn || board[x, y] != 0) return; // Ignore if not player's turn or cell is occupied

        // Update board and button text
        board[x, y] = 1; // Player's move
        gridButtons[x, y].GetComponentInChildren<Text>().text = "X";

        // Check for win or draw
        if (CheckWin(1))
        {
            Debug.Log("Player Wins!");
            EndGame();
            return;
        }
        else if (CheckDraw())
        {
            Debug.Log("It's a Draw!");
            EndGame();
            return;
        }

        // Pass turn to AI
        isPlayerTurn = false;
        Invoke("AIMove", 1f); // Delay to simulate AI thinking
    }

    bool CheckWin(int player)
    {
        for (int i = 0; i < 3; i++)
        {
            if (board[i, 0] == player && board[i, 1] == player && board[i, 2] == player) return true; // Row
            if (board[0, i] == player && board[1, i] == player && board[2, i] == player) return true; // Column
        }
        if (board[0, 0] == player && board[1, 1] == player && board[2, 2] == player) return true; // Diagonal
        if (board[0, 2] == player && board[1, 1] == player && board[2, 0] == player) return true; // Diagonal

        return false;
    }

    bool CheckDraw()
    {
        foreach (int cell in board)
        {
            if (cell == 0) return false; // Empty cell found
        }
        return true;
    }

    void AIMove()
    {
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (board[i, j] == 0) // Empty cell
                {
                    board[i, j] = -1; // AI's move
                    gridButtons[i, j].GetComponentInChildren<Text>().text = "O";

                    // Check for win or draw
                    if (CheckWin(-1))
                    {
                        Debug.Log("AI Wins!");
                        EndGame();
                        return;
                    }
                    else if (CheckDraw())
                    {
                        Debug.Log("It's a Draw!");
                        EndGame();
                        return;
                    }

                    isPlayerTurn = true;
                    return; // End turn
                }
            }
        }
    }

    void EndGame()
    {
        foreach (Button button in gridButtons)
        {
            button.interactable = false; // Disable all buttons
        }
    }

    int Minimax(int[,] boardState, bool isMaximizing)
    {
        // Check terminal states
        if (CheckWin(1)) return -1; // Player wins
        if (CheckWin(-1)) return 1; // AI wins
        if (CheckDraw()) return 0; // Draw

        int bestScore = isMaximizing ? int.MinValue : int.MaxValue;

        // Iterate through all cells
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (boardState[i, j] == 0) // Empty cell
                {
                    boardState[i, j] = isMaximizing ? -1 : 1; // AI or Player move
                    int score = Minimax(boardState, !isMaximizing); // Recursive call
                    boardState[i, j] = 0; // Undo move

                    // Maximize or minimize score
                    bestScore = isMaximizing ? Mathf.Max(score, bestScore) : Mathf.Min(score, bestScore);
                }
            }
        }
        return bestScore;
    }

    void AIMoveWithMinimax()
    {
        int bestScore = int.MinValue;
        Vector2Int bestMove = Vector2Int.zero;

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (board[i, j] == 0) // Empty cell
                {
                    board[i, j] = -1; // AI's move
                    int score = Minimax(board, false); // Player's turn next
                    board[i, j] = 0; // Undo move

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMove = new Vector2Int(i, j);
                    }
                }
            }
        }

        // Make the best move
        board[bestMove.x, bestMove.y] = -1; // AI's move
        gridButtons[bestMove.x, bestMove.y].GetComponentInChildren<Text>().text = "O";

        // Check for win or draw
        if (CheckWin(-1))
        {
            Debug.Log("AI Wins!");
            EndGame();
            return;
        }
        else if (CheckDraw())
        {
            Debug.Log("It's a Draw!");
            EndGame();
            return;
        }

        isPlayerTurn = true; // Pass turn to player
    }
}
