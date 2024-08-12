using infinite_tic_tac_toe;
using infinite_tic_tac_toe.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace wpf_ui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Dictionary<Player, ImageSource> imageSources = new()
        {
            { Player.X, new BitmapImage(new Uri("pack://application:,,,/wpf-ui;component/Assets/X15.png")) },
            { Player.O, new BitmapImage(new Uri("pack://application:,,,/Assets/O15.png")) },
        };

        private readonly Image[,] imageControls = new Image[3,3];
        private readonly GameState gameState = new GameState();

        public MainWindow()
        {
            InitializeComponent();
            SetupGameGrid();

            gameState.MoveMade += OnMoveMade;
            gameState.GameEnded += OnGameEnded;
            gameState.GameRestarted += OnGameRestarted;
        }

        private void SetupGameGrid()
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Image imageControl = new Image();
                    GameGrid.Children.Add(imageControl);
                    imageControls[i, j] = imageControl;
                }
            }
        }

        private void TransitionToEndScreen(string text, ImageSource winnerImage)
        {
            TurnPanel.Visibility = Visibility.Hidden;
            GameCanvas.Visibility = Visibility.Hidden;
            ResultText.Text = text;
            WinnerImage.Source=winnerImage;
            EndScreen.Visibility = Visibility.Visible;
        }

        private void TransitionToGameScreen()
        {
            EndScreen.Visibility=Visibility.Hidden;
            TurnPanel.Visibility=Visibility.Visible;
            GameCanvas.Visibility=Visibility.Visible;
        }

        private void OnMoveMade(int x, int y)
        {
            Player player = gameState.GameGrid[x,y];
            imageControls[x,y].Source = imageSources[player];
            PlayerImage.Source = imageSources[gameState.CurrentPlayer];
        }

        private void OnGameEnded(GameResult gameResult) 
        { 
            if(gameResult.Winner == Player.None)
            {
                TransitionToEndScreen("It's a tie!", null);
            }
            else
            {
                TransitionToEndScreen("Winner", imageSources[gameResult.Winner]);
            }
        }

        private void OnGameRestarted()
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    imageControls[i, j].Source = null;
                }
            }
            PlayerImage.Source= imageSources[gameState.CurrentPlayer];
            TransitionToGameScreen();
        }

        private void GameGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            double squareSize = GameGrid.Width / 3;
            Point clickPosition = e.GetPosition(GameGrid);
            int row = (int)(clickPosition.Y / squareSize);
            int col = (int)(clickPosition.X / squareSize);
            gameState.MakeMove(row, col);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            gameState.Reset();
        }
    }
}