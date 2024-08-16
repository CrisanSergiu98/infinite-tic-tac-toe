using infinite_tic_tac_toe;
using infinite_tic_tac_toe.Enums;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Media;

namespace wpf_ui
{   
    public partial class MainWindow : Window
    {
        private MediaPlayer _soundPlayerAmbientLoop;
        private SoundPlayer _soundPlayerMove;

        private readonly Dictionary<Piece, ImageSource> imageSources = new()
        {
            { Piece.X, new BitmapImage(new Uri("pack://application:,,,/wpf-ui;component/Assets/X15.png")) },
            { Piece.O, new BitmapImage(new Uri("pack://application:,,,/wpf-ui;component/Assets/O15.png")) },
        };

        private readonly Dictionary<Piece, ObjectAnimationUsingKeyFrames> keyFrames = new()
        {
            {Piece.X, new ObjectAnimationUsingKeyFrames() },
            {Piece.O,new ObjectAnimationUsingKeyFrames() }
        };

        private readonly DoubleAnimation fadeInAnimation = new DoubleAnimation
        {
            Duration = TimeSpan.FromSeconds(.5),
            From = 0,
            To = 1,
        };

        private readonly DoubleAnimation fadeOutAnimation = new DoubleAnimation
        {
            Duration = TimeSpan.FromSeconds(.5),
            From = 1,
            To = 0,
        };

        private readonly DoubleAnimation fadeToHalfOpacity = new DoubleAnimation
        {
            Duration = TimeSpan.FromSeconds(.5),
            From = 1.0,
            To = 0.5,
        };

        private readonly DoubleAnimation fadeToFullOpacity = new DoubleAnimation
        {
            Duration = TimeSpan.FromSeconds(.5),
            From = 0.5,
            To = 1,
        };

        private readonly Image[,] imageControls = new Image[3,3];
        private readonly GameState gameState = new GameState();

        public MainWindow()
        {
            InitializeComponent();
            SetupAudio();
            SetupGameGrid();
            SetupAnimations();

            gameState.MoveMade += OnMoveMade;
            gameState.GameEnded += OnGameEnded;
            gameState.GameRestarted += OnGameRestarted;
            gameState.PieceRemoved += OnPieceRemoved;
        }
        private void SetupAudio()
        {
            _soundPlayerAmbientLoop = new MediaPlayer();
            _soundPlayerAmbientLoop.Open(new Uri("Assets/ambient_loop.wav", UriKind.Relative));

            _soundPlayerAmbientLoop.MediaEnded += BackgroundPlayer_MediaEnded;

            _soundPlayerAmbientLoop.Play();


            _soundPlayerMove = new SoundPlayer("Assets/move.wav");
            _soundPlayerMove.Load();
        }

        private void BackgroundPlayer_MediaEnded(object sender, EventArgs e)
        {
            _soundPlayerAmbientLoop.Position = TimeSpan.Zero;
            _soundPlayerAmbientLoop.Play();
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

        private void SetupAnimations()
        {
            keyFrames[Piece.X].Duration = TimeSpan.FromSeconds(.25);
            keyFrames[Piece.O].Duration = TimeSpan.FromSeconds(.25);

            for(int i = 0;i < 16; i++)
            {
                Uri xUri = new Uri($"pack://application:,,,/Assets/X{i}.png");
                BitmapImage xImg = new BitmapImage(xUri);
                DiscreteObjectKeyFrame xKeyFrame = new DiscreteObjectKeyFrame(xImg);
                keyFrames[Piece.X].KeyFrames.Add(xKeyFrame);

                Uri oUri = new Uri($"pack://application:,,,/Assets/O{i}.png");
                BitmapImage oImg = new BitmapImage(oUri);
                DiscreteObjectKeyFrame oKeyFrame = new DiscreteObjectKeyFrame(oImg);
                keyFrames[Piece.O].KeyFrames.Add(oKeyFrame);
            }
        }

        private async Task FadeOut(UIElement uiElement)
        {
            uiElement.BeginAnimation(OpacityProperty,fadeOutAnimation);
            await Task.Delay(fadeOutAnimation.Duration.TimeSpan);
            uiElement.Visibility = Visibility.Hidden;
        }

        private async Task FadeIn(UIElement uiElement)
        {
            uiElement.Visibility = Visibility.Visible;
            uiElement.BeginAnimation(OpacityProperty, fadeInAnimation);
            await Task.Delay(fadeInAnimation.Duration.TimeSpan);            
        }

        private async Task TransitionToEndScreen(string text, ImageSource winnerImage)
        {
            await Task.WhenAll(FadeOut(TurnPanel), FadeOut(GameCanvas));
            ResultText.Text = text;
            WinnerImage.Source=winnerImage;
            await FadeIn(EndScreen);
        }

        private async Task TransitionToGameScreen()
        {
            await FadeOut(EndScreen);
            Line.Visibility = Visibility.Hidden;
            await Task.WhenAll(FadeIn(TurnPanel),FadeIn(GameCanvas));
        }

        private (Point,Point) FindLinePoints(WinInfo winInfo)
        {
            double squareSize = GameGrid.Width / 3;
            double margin = squareSize /2;

            if(winInfo.Type == WinningCondition.Row)
            {
                double y = winInfo.Number * squareSize + margin;
                return (new Point(0, y), new Point(GameGrid.Width, y));
            }
            if (winInfo.Type == WinningCondition.Column)
            {
                double x = winInfo.Number * squareSize + margin;
                return (new Point(x, 0), new Point(x, GameGrid.Height));
            }
            if (winInfo.Type == WinningCondition.MainDiagonal)
            {
                return (new Point(0, 0), new Point(GameGrid.Width, GameGrid.Height));
            }
            return (new Point(GameGrid.Width, 0), new Point(0, GameGrid.Height));
        }

        private async Task ShowLine(WinInfo winInfo) 
        { 
            (Point start,Point end) = FindLinePoints(winInfo);

            Line.X1 = start.X;
            Line.Y1 = start.Y;

            DoubleAnimation x2Animation = new DoubleAnimation
            {
                Duration = TimeSpan.FromSeconds(.25),
                From = start.X,
                To = end.X,
            };

            DoubleAnimation y2Animation = new DoubleAnimation
            {
                Duration = TimeSpan.FromSeconds(.25),
                From = start.Y,
                To = end.Y,
            };            

            Line.Visibility = Visibility.Visible;

            Line.BeginAnimation(System.Windows.Shapes.Line.X2Property,x2Animation);
            Line.BeginAnimation(System.Windows.Shapes.Line.Y2Property, y2Animation);

            await Task.Delay(x2Animation.Duration.TimeSpan);
        }

        private void OnMoveMade(int x, int y)
        {
            Piece piece = gameState.GameGrid[x, y];

            imageControls[x, y].Opacity = 1.0;

            imageControls[x, y].BeginAnimation(OpacityProperty, fadeToFullOpacity);

            imageControls[x, y].BeginAnimation(
                Image.SourceProperty,
                keyFrames[piece]);

            _soundPlayerMove.Play();

            PieceImage.Source = imageSources[gameState.CurrentPlayer];

            if (gameState.RemovalNextTurn())
            {     
                imageControls[
                    gameState.GetNextRemoval.Item1,
                    gameState.GetNextRemoval.Item2
                    ].BeginAnimation(OpacityProperty, fadeToHalfOpacity);
            }
        }
        private void OnPieceRemoved(int x, int y)
        {
            imageControls[x, y].BeginAnimation(Image.SourceProperty, null);
            imageControls[x, y].Source = null;                       
        }

        private async void OnGameEnded(GameResult gameResult) 
        { 
            if(gameResult.Winner == Piece.None)
            {
                await TransitionToEndScreen("It's a tie!", null);
            }
            else
            {
                await ShowLine(gameResult.WinInfo);
                await Task.Delay(1000);
                await TransitionToEndScreen("Winner", imageSources[gameResult.Winner]);
            }
        }

        private async void OnGameRestarted()
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    imageControls[i, j].BeginAnimation(Image.SourceProperty, null);
                    imageControls[i, j].Source = null;
                }
            }
            PieceImage.Source = imageSources[gameState.CurrentPlayer];
            await TransitionToGameScreen();
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
            if (gameState.GameOver)
            {
                gameState.Reset();
            }            
        }
    }
}