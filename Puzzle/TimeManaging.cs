using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Puzzle
{
    public partial class MainWindow
    {
        #region variables
        public int seconds = 0;
        public DispatcherTimer timer, pauseTimer;
        #endregion

        #region methods
        private void setTimers()
        {
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += timer_Tick;

            pauseTimer = new DispatcherTimer();
            pauseTimer.Interval = new TimeSpan(0, 0, 0, 0, 50);
            pauseTimer.Tick += pauseTimer_Tick;
        }

        void new_timer()
        {
            TimerLabel.Visibility = Visibility.Visible;
            timer.Stop();
            seconds = 0;
            TimerLabel.Content = String.Format("Time {0}:{1}:{2}", (seconds / 3600).ToString("00")
                , (seconds / 60).ToString("00"), (seconds % 60).ToString("00"));
            timer.Start();
        }

        private void pauseImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            pauseTimer.Start();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            seconds++;
            TimerLabel.Content = String.Format("Time {0}:{1}:{2}", (seconds / 3600).ToString("00")
                , (seconds / 60).ToString("00"), (seconds % 60).ToString("00"));
        }

        void pauseTimer_Tick(object sender, EventArgs e)
        {
            PauseImage.Opacity -= 0.05;
            PauseImage.UpdateLayout();

            if (PauseImage.Opacity <= 0.1)
            {
                pauseTimer.Stop();
                timer.Start();
                StartButton.IsEnabled = true;
                PauseButton.IsEnabled = true;
                EasyRadio.IsEnabled = true;
                MediumRadio.IsEnabled = true;
                HardRadio.IsEnabled = true;
                PauseImage.Visibility = Visibility.Hidden;
            }
        }
        #endregion
    }
}
