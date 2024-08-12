using infinite_tic_tac_toe.Enums;

namespace infinite_tic_tac_toe;

public class GameState
{
    public Player[,] GameGrid {  get; private set; }
    public Player CurrentPlayer { get; private set; }
    public int TurnsPassed {  get; private set; }
    public bool GameOver {  get; private set; }

    public event Action<int, int> MoveMade;
    public event Action<GameResult> GameEnded;
    public event Action GameRestarted;

    public GameState()
    {
        GameGrid = new Player[3,3];
        CurrentPlayer = Player.X;
        TurnsPassed = 0;
        GameOver = false;
    }

    private bool CanMove(int x, int y)
    {
        return !GameOver && GameGrid[x,y] == Player.None;
    }

    private bool IsGridFull()
    {
        return TurnsPassed == 9;
    }

    private void SwitchPlayer()
    {
        CurrentPlayer = CurrentPlayer == Player.X? Player.O: Player.X;
    }

    private bool AreSquaresMarked((int, int)[] squares, Player player)
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
            winInfo = new WinInfo { Type = WinType.Row, Number = x };
            return true;
        }

        if (AreSquaresMarked(col, CurrentPlayer))
        {
            winInfo = new WinInfo { Type = WinType.Column, Number = y };
            return true;
        }

        if (AreSquaresMarked(mainDiag, CurrentPlayer))
        {
            winInfo = new WinInfo { Type = WinType.MainDiagonal };
            return true;
        }

        if (AreSquaresMarked(antiDiag, CurrentPlayer))
        {
            winInfo = new WinInfo { Type = WinType.AntiDiagonal };
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

        if (IsGridFull())
        {
            gameResult = new GameResult { Winner = Player.None };
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
        TurnsPassed++;

        if(DidMoveEndGame(x,y, out GameResult gameResult))
        {
            GameOver = true;
            MoveMade?.Invoke(x,y);
            GameEnded?.Invoke(gameResult);
        }
        else
        {
            SwitchPlayer();
            MoveMade?.Invoke(x,y);
        }
    }

    public void Reset()
    {
        GameGrid = new Player[3,3];
        CurrentPlayer = Player.X;
        TurnsPassed = 0;
        GameOver = false;
        GameRestarted?.Invoke();
    }
}
