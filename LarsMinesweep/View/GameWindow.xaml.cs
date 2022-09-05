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
            DataContext = this;
            _tileCounter = -2; //It is -2 because length and height both start at 1, so to make counter start at 0: -2
            GenerateTiles();

            //Load pictures of flag and mines into the memory of the application
            flagSource = new BitmapImage(new Uri(@"C:\Users\Lars\source\repos\LarsMinesweep\LarsMinesweep\Model\flag.png", UriKind.Relative));
            mineSource = new BitmapImage(new Uri(@"C:\Users\Lars\source\repos\LarsMinesweep\LarsMinesweep\Model\mine.png", UriKind.Relative));
            
             


        }

        //This method is called every single tick
        private void DtOnTick(object? sender, EventArgs evenArgs)
        {
            Time++; //Adds args counter that goes up
        }

        private void GenerateTiles()
        {
            //Empty grid of tile objects
            _tileGrid = new Tiles[_height, _width];
            for (int row = 0; row < _height; row++)
            {
                //Generate row
                for(int col = 0; col < _width; col++)
                {
                    //Generate column
                    _tileGrid[row, col] = new Tiles()
                    {
                        IsItCleared = false,
                        IsItFlagged = false,
                        IsItAMine = false,
                        Row = row,
                        Column = col
                    };
                }
            }

            //Place mines an empty grid
            for(int i = 0; i < _bombs; i++)
            {
                int row = random.Next(0, _height);
                int col = random.Next(_width);

                if (_tileGrid[row, col].IsItAMine) --i; //Check if the field is already args mine
                else _tileGrid[row, col].IsItAMine = true;
                Debug.WriteLine(_tileGrid[row, col]);
            }
            GenerateGrid();
        }

        private void GenerateGrid()
        {
            StackPanel mineField = new StackPanel
            {
                Orientation = Orientation.Vertical
            };
            for(int row = 0; row < _tileGrid.GetLength(0); row++)
            {
                StackPanel RowStack = new StackPanel();
                RowStack.Orientation = Orientation.Horizontal;
                //generate row
                for(int col = 0; col < _tileGrid.GetLength(1); col++)
                {
                    //generate column
                    Button gridBtn = new Button
                    {
                        Width = 20,
                        Height = 20,
                        Background = Brushes.White,
                        Style = (Style)Application.Current.TryFindResource("TileButton"),
                        Name = "btn" + row + col
                    };

                    var row1 = row;
                    var col1 = col;
                    gridBtn.Click += (s, args) => GridBtn_Click(s, args, row1, col1);
                    gridBtn.MouseRightButtonDown += (s, args) => FlagTile(s, args, row1, col1);
                    RowStack.Children.Add(gridBtn);
                }
                mineField.Children.Add(RowStack);
            }
            MinefieldGrid.Children.Add(mineField);
        }

        /// <summary>
        /// This method gets called when tile is left clicked, aka marking args tile as args flag
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
                    List<Tiles> surroundingTiles = GetNeighbouringCells(row, col);
                    foreach(Tiles t in surroundingTiles)
                    {
                        //Check the list and count the mines around the button/tile clicked
                        if (t.IsItAMine) mineCounter++;
                    }

                    //If this is 0, clear all surroundings tiles and count number of mines again regarding the cleared tiles
                    if (mineCounter != 0) ((Button)sender).Content = mineCounter.ToString();
                    else
                    {
                        ClearSurroundingCells(row, col);
                    }


                }
                CheckWin();
            }

        }

        //Check if all tiles have been cleared/did you win or not yet?
        private void CheckWin()
        {
            bool IsCleared = true;
            for(int i = 0; i < _tileGrid.GetLength(0); i++)
            {
                if (!IsCleared) break;
                ///this one created out of bounds error sometimes, fix this
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
                this.Close();
            }

        }

        #region tiles
        //Clear surrounding empty cells
        private void ClearSurroundingCells(int row, int col)
        {
            List<Tiles> surroundingCells = GetNeighbouringCells(row, col);
            foreach(Tiles tiles in surroundingCells)
            {
                List<Tiles> tilesList = GetNeighbouringCells(tiles.Row, tiles.Column);
                int counter = 0;
                foreach (Tiles tiles1 in tilesList)
                {
                    if (tiles1.IsItAMine)
                    {
                        counter++;
                    }
                }
                if(counter == 0 && (tiles.Row != row || tiles.Column != col) && !tiles.IsItCleared)
                {
                    tiles.IsItCleared = true;
                    ClearSurroundingCells(tiles.Row, tiles.Column);
                }
                else
                {
                    //This is taken almost directly from stackoverflow, together with Descendant method
                    //Study and explain what is happening
                    
                    //If this is disabled, it wont clear neighbouring cells
                     
                    ((Button)FindDescendant(MinefieldGrid, $"btn{tiles.Row}{tiles.Column}")).IsEnabled = false;
                    ((Button)FindDescendant(MinefieldGrid, $"btn{tiles.Row}{tiles.Column}")).Background = Brushes.Green;
                    ((Button)FindDescendant(MinefieldGrid, $"btn{tiles.Row}{tiles.Column}")).Foreground = Brushes.White;
                    _tileGrid[tiles.Row, tiles.Column].IsItCleared = true;
                    if (counter > 0) ((Button)FindDescendant(MinefieldGrid, $"btn{tiles.Row}{tiles.Column}")).Content = counter;

                    
                }
            }

        }

        //Get list of neighbouring cells
        private List<Tiles> GetNeighbouringCells(int row, int col)
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
                        if(col + j > -1 && col+j < _tileGrid.GetLength(1) && (col+j != 0 || row+i != 0))
                        {
                            surroundingTiles.Add(_tileGrid[row + i, col + j]);
                        }
                    }
                }
            }
            return surroundingTiles;
        }
        #endregion

        #region flag
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



        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string elementName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(elementName));
        }

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

        #region descendants, maybe remove, since it is copied from google
        //Find descendant control ny name
        
         
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
