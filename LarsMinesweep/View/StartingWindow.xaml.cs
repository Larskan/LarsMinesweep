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
    /// Interaction logic for StartingWindow.xaml
    /// </summary>
    public partial class StartingWindow : Window
    {
        public StartingWindow()
        {
            InitializeComponent();
        }

        private void StartGame(int length, int height, int bombs)
        {
            GameWindow gameWindow = new GameWindow(length, height, bombs);
            gameWindow.Show();
            this.Close(); //Closes whatever we have open, so it doesnt persist during the new game
        }

        private void ButtonEasy_Click(object sender, RoutedEventArgs e)
        {
            StartGame(16, 16, 10); //Choosing 10 as default easy bombs

        }

        private void ButtonHard_Click(object sender, RoutedEventArgs e)
        {
            StartGame(32, 32, 50); //Choosing 50 as default hard bombs

        }

        private void ButtonCustom_Click(object sender, RoutedEventArgs e)
        {
            CustomGameWindow customGameWindow = new CustomGameWindow();
            customGameWindow.Show();
            this.Close();
        }
    }
}
