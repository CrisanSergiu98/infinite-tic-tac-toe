using infinite_tic_tac_toe.Enums;

namespace infinite_tic_tac_toe;

public class GameState
{
    private const int MaxMoves = 7;
    public Piece[,] GameGrid {  get; private set; }
    public Piece CurrentPlayer { get; private set; }
    public (int,int) GetNextRemoval { get => GameHistory.Peek(); }
    public int GetTurnCount { get => GameHistory.Count; }

    private Queue<(int, int)> GameHistory = new Queue<(int, int)>();
    public bool GameOver {  get; private set; }

    public event Action<int, int> MoveMade;
    public event Action<GameResult> GameEnded;
    public event Action GameRestarted;
    public event Action<int, int> PieceRemoved;

    public GameState()
    {
        GameGrid = new Piece[3,3];
        CurrentPlayer = Piece.X;
        GameOver = false;        
    }

    public bool RemovalNextTurn()
    {
        return GetTurnCount >= MaxMoves - 1;
    }

    private bool CanMove(int x, int y)
    {
        return !GameOver && GameGrid[x,y] == Piece.None;
    }
    
    private void SwitchPlayer()
    {
        CurrentPlayer = CurrentPlayer == Piece.X? Piece.O: Piece.X;
    }

    private bool AreSquaresMarked((int, int)[] squares, Piece player)
    {
        foreach ((int x, int y) in squares) 
        {
            if (GameGrid[x, y] != player)
                return false;            
        }
        return true;
    }

    private bool DidMoveWin(int x, int y, out WinInfo winInfo)
    {
        (int, int)[] row = new[] { (x, 0), (x, 1), (x, 2) };
        (int, int)[] col = new[] { (0, y), (1, y), (2, y) };
        (int, int)[] mainDiag = new[] { (0, 0), (1, 1), (2, 2) };
        (int, int)[] antiDiag = new[] { (0, 2), (1, 1), (2, 0) };

        if(AreSquaresMarked(row, CurrentPlayer))
        {
            winInfo = new WinInfo { Type = WinningCondition.Row, Number = x };
            return true;
        }

        if (AreSquaresMarked(col, CurrentPlayer))
        {
            winInfo = new WinInfo { Type = WinningCondition.Column, Number = y };
            return true;
        }

        if (AreSquaresMarked(mainDiag, CurrentPlayer))
        {
            winInfo = new WinInfo { Type = WinningCondition.MainDiagonal };
            return true;
        }

        if (AreSquaresMarked(antiDiag, CurrentPlayer))
        {
            winInfo = new WinInfo { Type = WinningCondition.AntiDiagonal };
            return true;
        }

        winInfo = null;

        return false;
    }

    private bool DidMoveEndGame(int x, int y, out GameResult gameResult) 
    {
        if (DidMoveWin(x, y, out WinInfo winInfo)) 
        {
            gameResult = new GameResult { Winner = CurrentPlayer, WinInfo = winInfo };
            return true;
        }

        gameResult = null;

        return false;
    }

    public void MakeMove(int x, int y)
    {
        if(!CanMove(x, y))
        {
            return;
        }

        GameGrid[x, y] = CurrentPlayer;

        if (DidMoveEndGame(x, y, out GameResult gameResult))
        {
            GameOver = true;
            MoveMade?.Invoke(x, y);
            GameEnded?.Invoke(gameResult);
        }
        else
        {
            SwitchPlayer();
            GameHistory.Enqueue((x,y));
            

            if (GetTurnCount >= MaxMoves)
            {
                int xToRemove = GameHistory.Peek().Item1;
                int yToRemove = GameHistory.Peek().Item2;

                GameGrid[xToRemove, yToRemove] = Piece.None;
                GameHistory.Dequeue();

                PieceRemoved?.Invoke(xToRemove, yToRemove);
            }
            MoveMade?.Invoke(x, y);
        }
    }    

    public void Reset()
    {
        GameGrid = new Piece[3,3];
        CurrentPlayer = Piece.X;
        GameOver = false;
        GameHistory = new Queue<(int, int)>();
        GameRestarted?.Invoke();
    }
}
