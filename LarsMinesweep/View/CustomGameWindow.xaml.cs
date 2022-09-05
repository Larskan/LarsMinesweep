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

namespace LarsMinesweep.View
{
    /// <summary>
    /// Interaction logic for CustomGameWindow.xaml
    /// </summary>
    public partial class CustomGameWindow : Window
    {

        private int _length;
        private int _height;
        private int _bombs;
        public CustomGameWindow()
        {
            InitializeComponent();
        }

        private void BtnStart_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _length = int.Parse(textWidth.Text);
                _height = int.Parse(textHeight.Text);
                _bombs = int.Parse(textBombs.Text);

                if(_length < 3 || _height < 3 || _bombs >= _length + _height || _height > 50 || _length > 50)
                {
                    throw new Exception();
                }

                GameWindow gameWindow = new GameWindow(_length,_height, _bombs);
                gameWindow.Show();
                this.Close();
            }
            catch (Exception exception)
            {
                MessageBox.Show("Make sure all fields are numbers between 3 and 50", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

        }
    }
}
