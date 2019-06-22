using System;
using System.Collections.Generic;
using System.Windows;
using Puzzle.HelperClasses;

namespace Puzzle
{
    /// <summary>
    /// Interaction logic for EndWindow.xaml
    /// </summary>
    public partial class EndWindow : Window
    {
        public int max = 1;

        public EndWindow()
        {
            InitializeComponent();
            WindowStyle = WindowStyle.ToolWindow;
            ResizeMode = ResizeMode.NoResize;
        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            MainWindow mw = this.Owner as MainWindow;
            timerLabel.Content = String.Format("{0}:{1}:{2}", (mw.seconds / 3600).ToString("00"), (mw.seconds / 60).ToString("00"), (mw.seconds % 60).ToString("00"));

            for (int i = 0; i < mw.unionList.Count; i++)
                if (mw.unionList[i].Count > max)
                    max = mw.unionList[i].Count;

            if (mw.gd.currentGameCounter == mw.maxCount)
                wonLabel.Content = String.Format("You won! You connected all {0} puzzles. Your time:", mw.gd.currentGameCounter);
            else
                wonLabel.Content = String.Format("You connected only {0} puzzle(s). Your time:", max);
        }


        public int timeCompare(listItems x, listItems y)
        {
            if (x.time > y.time) return 1;
            if (x.time == y.time) return 0;
            return -1;
        }

        public int counterCompare(listItems x, listItems y)
        {
            if (x.counter > y.counter) return 1;
            if (x.counter == y.counter) return 0;
            return -1;
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mw = this.Owner as MainWindow;
            string textboxName = nameTextBox.Text;

            if (textboxName == "")
                textboxName = "John Doe";

            List<listItems> liList = new List<listItems>();

            switch (mw.gd.gameLevel)
            {
                case 'h':
                    mw.hardList.Add(new listItems { name = textboxName, counter = max, time = mw.seconds });
                    break;
                case 'm':
                    mw.mediumList.Add(new listItems { name = textboxName, counter = max, time = mw.seconds });
                    break;
                case 'e':
                    mw.easyList.Add(new listItems { name = textboxName, counter = max, time = mw.seconds });
                    break;
            }

            setNewGame(mw);
            this.Close();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mw = this.Owner as MainWindow;

            if (mw.gd.currentGameCounter == mw.maxCount)
                setNewGame(mw);
            else
                mw.timer.Start();

            this.Close();
        }

        private void setNewGame(MainWindow mw)
        {
            mw.start = true;
            mw.startButton.Content = "Start Game";
            mw.pauseButton.IsEnabled = false;
            mw.stream.Close();
            mw.image.Background = null;
            mw.timer.Stop();
            mw.timerLabel.Visibility = Visibility.Hidden;
            mw.unionList.Clear();
            mw.image.Children.Clear();
        }
    }
}
