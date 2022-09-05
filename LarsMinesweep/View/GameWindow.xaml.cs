using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using LarsMinesweep.ViewModel;


namespace LarsMinesweep.View //Has to be .View, otherwise MinefieldGrid breaks
{
   
    public partial class GameWindow : Window, INotifyPropertyChanged
    {
        #region the different variables needed
        // The underscore is because I read its good practice to have changable variables have an underscore
        private Random random;
        private readonly int _height;
        private readonly int _width;
        private int _bombs;
        private int _starting_bombs;
        private DispatcherTimer dt;
        private int _time;
        private Tiles[,] _tileGrid;
        readonly ImageSource mineSource;
        readonly ImageSource flagSource;
        private int _tileCounter;
        #endregion

        #region The Gamewindow
        public GameWindow(int w, int h, int b)
        {
            InitializeComponent();

            random = new Random();
            _height = h;
            _width = w;
            _bombs = b;
            _starting_bombs = b;

            // Makes dispatchertimer(dt) with 1sec interval
            dt = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1000) };
            dt.Tick += DtOnTick;
            dt.Start();
            DataContext = this; //Databinding
            _tileCounter = -2; //It is -2 because length and height both start at 1, so to make counter start at 0: -2
            GenerateTiles();

            //Load pictures of flag and mines into the memory of the application, so they can be called later
            //Only absolute path seemed to work, tried with just Model\flag.png, but it kept breaking
            flagSource = new BitmapImage(new Uri(@"C:\Users\Lars\source\repos\LarsMinesweep\LarsMinesweep\Model\flag.png", UriKind.Relative));
            mineSource = new BitmapImage(new Uri(@"C:\Users\Lars\source\repos\LarsMinesweep\LarsMinesweep\Model\mine.png", UriKind.Relative));
            
        }

        //This method is called every single tick
        private void DtOnTick(object? sender, EventArgs evenArgs)
        {
            Time++; //Adds a counter that goes up, so you can see how long you take, great for potential hiscores
        }
        #endregion

        #region Generate Tiles and Generate Grid
        private void GenerateTiles()
        {
            //Empty grid of tile objects
            _tileGrid = new Tiles[_height, _width];

            //Generate row
            for (int row = 0; row < _height; row++) //Wont increase row unless height matches
            {
                //Generate column
                for (int column = 0; column < _width; column++) //Wont increase column unless row matches
                {
                    //Adds the grid
                    _tileGrid[row, column] = new Tiles()
                    {
                        IsItCleared = false,
                        IsItFlagged = false,
                        IsItAMine = false,
                        Row = row,
                        Column = column
                    };
                }
            }

            //Place mines an empty grid
            for(int i = 0; i < _bombs; i++)
            {
                //Places the bombs randomly within the parameters of row and column
                int row = random.Next(0, _height);
                int column = random.Next(_width);

                if (_tileGrid[row, column].IsItAMine) --i; //Check if the field is already a mine
                else _tileGrid[row, column].IsItAMine = true; //Adds a mine
                Debug.WriteLine(_tileGrid[row, column]); //Just for fanciness
            }
            GenerateGrid();
        }

        private void GenerateGrid()
        {
            StackPanel mineField = new StackPanel
            {
                Orientation = Orientation.Vertical
            };
            //generate row
            for (int row = 0; row < _tileGrid.GetLength(0); row++)
            {
                StackPanel RowStack = new StackPanel();
                RowStack.Orientation = Orientation.Horizontal;

                //generate column
                for (int column = 0; column < _tileGrid.GetLength(1); column++)
                {   
                    //Creating the tiles
                    Button gridBtn = new Button
                    {
                        Width = 20,
                        Height = 20,
                        Background = Brushes.White,
                        Style = (Style)Application.Current.TryFindResource("TileButton"),
                        Name = "btn" + row + column
                    };

                    var row1 = row;
                    var column1 = column;
                    gridBtn.Click += (s, args) => GridBtn_Click(s, args, row1, column1); //Checks the tile when clicked
                    gridBtn.MouseRightButtonDown += (s, args) => FlagTile(s, args, row1, column1); //Adds the flag
                    RowStack.Children.Add(gridBtn); //Adds the interaction with tiles to the RowStack
                }
                mineField.Children.Add(RowStack); //Adds the RowStack to the mineField
            }
            MinefieldGrid.Children.Add(mineField); //Adds the mineField to the entire MinefieldGrid
        }
        #endregion

        #region Interaction with Grid tiles
        /// <summary>
        /// This method gets called when tile is left clicked, aka marking tile as flag
        /// </summary>
        private void GridBtn_Click(object sender, RoutedEventArgs args, int row, int col)
        {
            if (!_tileGrid[row, col].IsItFlagged)
            {
                //If it is args mine = gameover
                if(_tileGrid[row, col].IsItAMine)
                {
                    ((Button)sender).Background = new ImageBrush(mineSource);
                    if ((MessageBox.Show("You have hit a mine! \n Restart?", "Game Over", MessageBoxButton.YesNo) ==
                        MessageBoxResult.Yes)) //A box to give option to restart or close game
                        
                        
                        
                    {
                        new GameWindow(_width, _height, _starting_bombs).Show(); //Starts new game
                        this.Close(); //closes current lost game
                    }
                    else new StartingWindow().Show(); //Goes back to first starting screen
                    this.Close(); //closes current lost game
                }
                else
                {
                    ((Button)sender).IsEnabled = false;
                    ((Button)sender).Background = Brushes.Green;
                    ((Button)sender).Foreground = Brushes.White;
                    _tileGrid[row, col].IsItCleared = true;

                    _tileCounter++;

                    //Shows every button around the clicked
                    int mineCounter = 0;
                    List<Tiles> surroundingTiles = GetNeighbouringTiles(row, col);
                    foreach(Tiles t in surroundingTiles)
                    {
                        //Check the list and count the mines around the button/tile clicked
                        if (t.IsItAMine) mineCounter++;
                    }

                    //If this is 0, clear all surroundings tiles and count number of mines again regarding the cleared tiles
                    if (mineCounter != 0) ((Button)sender).Content = mineCounter.ToString();
                    else
                    {
                        ClearSurroundingTiles(row, col);
                    }


                }
                CheckWin();
            }

        }
        #endregion

        #region Check if you won or not
        //Check if all tiles have been cleared/did you win or not yet?
        private void CheckWin()
        {
            bool IsCleared = true;
            for(int i = 0; i < _tileGrid.GetLength(0); i++)
            {
                if (!IsCleared) break;
                
                for(int j = 0; j < _tileGrid.GetLength(1); j++)
                {
                    if (!_tileGrid[i,j].IsItFlagged && !_tileGrid[i, j].IsItCleared)
                    {
                        IsCleared = false;
                        break;
                    }
                }
            }
            if (IsCleared)
            {
                dt.Stop();
                if (MessageBox.Show("You have cleared all mines! \n Restart?", "Well Done!", MessageBoxButton.YesNo) ==
                    MessageBoxResult.Yes)
                {
                    new GameWindow(_width, _height, _starting_bombs).Show();
                    this.Close();
                }
                else new StartingWindow().Show();
                this.Close(); //this.Close() makes sure that our former game doesnt stick around
            }

        }
        #endregion

        #region Clear surrounding tiles
        //Clear surrounding empty tiles
        private void ClearSurroundingTiles(int row, int column)
        {
            List<Tiles> surroundingTiless = GetNeighbouringTiles(row, column);
            foreach(Tiles tiles in surroundingTiless)
            {
                List<Tiles> tilesList = GetNeighbouringTiles(tiles.Row, tiles.Column);
                int counter = 0;
                foreach (Tiles tiles1 in tilesList)
                {
                    if (tiles1.IsItAMine)
                    {
                        counter++;
                    }
                }
                //counter refers to the mine counter
                if(counter == 0 && (tiles.Row != row || tiles.Column != column) && !tiles.IsItCleared)
                {
                    tiles.IsItCleared = true;
                    ClearSurroundingTiles(tiles.Row, tiles.Column);
                }
                else
                {
                    //This Descendant setup is found on Stackoverflow, largely inspired
                    //I had never encounted Descendant before, but it seemed like a very good thing to learn to use

                    //If this is disabled, it wont clear neighbouring tiles
                    //The $ is to seperate the tiles from the string
                    //casting as Button
                    //Descendant means it basically walks all over the MinefieldGrid until it finds a result
                    //The results being the mines, the tiles around the mines will make numbers, but the rest is cleared
                    ((Button)FindDescendant(MinefieldGrid, $"btn{tiles.Row}{tiles.Column}")).IsEnabled = false; //Its not cleared by default
                    ((Button)FindDescendant(MinefieldGrid, $"btn{tiles.Row}{tiles.Column}")).Background = Brushes.Green; //My choice of colours
                    ((Button)FindDescendant(MinefieldGrid, $"btn{tiles.Row}{tiles.Column}")).Foreground = Brushes.White;
                    _tileGrid[tiles.Row, tiles.Column].IsItCleared = true; //Clears the neighbouring tiles
                    if (counter > 0) ((Button)FindDescendant(MinefieldGrid, $"btn{tiles.Row}{tiles.Column}")).Content = counter;

                    
                }
            }

        }

        //Get list of neighbouring cells, adds the 1,2,3 etc, depending on how many mines they touch
        private List<Tiles> GetNeighbouringTiles(int row, int column)
        {
            List<Tiles> surroundingTiles = new List<Tiles>();

            for(int i = -1; i <= 1; i++)
            {
                //check if row exists, incase of corners or edges
                if(row+i > -1 && row+i < _tileGrid.GetLength(0))
                {
                    for(int j = -1; j <= 1; j++)
                    {
                        //check if column exists, incase of corners or edges
                        if(column + j > -1 && column+j < _tileGrid.GetLength(1) && (column+j != 0 || row+i != 0))
                        {
                            surroundingTiles.Add(_tileGrid[row + i, column + j]);
                        }
                    }
                }
            }
            return surroundingTiles;
        }
        #endregion

        #region FlagTile
        //When right clicking tiles to get the flag to show
        private void FlagTile(object sender, RoutedEventArgs a, int row, int col)
        {
            if (_tileGrid[row, col].IsItFlagged)
            {
                ((Button)sender).Foreground = Brushes.Blue;
                ((Button)sender).Background = Brushes.Red;
                _tileGrid[row, col].IsItFlagged = false;
                Bombs++;
                _tileCounter--;
            }
            else
            {
                ((Button)sender).Background = new ImageBrush(flagSource);
                ((Button)sender).Foreground = Brushes.Blue;
                _tileGrid[row, col].IsItFlagged = true;
                --Bombs;
                _tileCounter++;
            }
            CheckWin(); //The moment all flags cover all bombs, you win
        }
        #endregion

        #region Property change
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string elementName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(elementName));
        }
        #endregion

        #region getters and setters
        public int Bombs
        {
            get => _bombs;
            set { _bombs = value; OnPropertyChanged("Bombs"); }
        }
        public int Time
        {
            get => _time;
            set { _time = value; OnPropertyChanged("Time"); }
        }
        #endregion

        #region descendants
        
        
        //The ? is condtional operator, evaluates a bool expression and return result
        //Havent worked with FrameworkElement much, if at all, this is heavily inspired from Stackoverflow in working with Descendants
        private static DependencyObject? FindDescendant( DependencyObject parent, string name)
        {
            //See if this object has the target name
            FrameworkElement element = parent as FrameworkElement;
            if ((element != null) && (element.Name == name)) return parent;

            //Recursively check the children
            int num_children = VisualTreeHelper.GetChildrenCount(parent);
            for(int i = 0; i < num_children; i++)
            {
                //See if this child has the target name
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                DependencyObject descendant = FindDescendant(child, name);
                if (descendant != null) return descendant;
            }
            //No descendant found with target name
            return null;
        }
        

        
        #endregion

    }
}
