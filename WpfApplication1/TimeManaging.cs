using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace WpfApplication1
{
    public partial class MainWindow
    {
        #region variables
        public int seconds = 0;
        public DispatcherTimer timer, pauseTimer;
        #endregion

        #region mathods
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
            timer.Stop();
            seconds = 0;
            timerLabel.Content = String.Format("Time {0}:{1}:{2}", (seconds / 3600).ToString("00")
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
            timerLabel.Content = String.Format("Time {0}:{1}:{2}", (seconds / 3600).ToString("00")
                , (seconds / 60).ToString("00"), (seconds % 60).ToString("00"));
        }

        void pauseTimer_Tick(object sender, EventArgs e)
        {
            pauseImage.Opacity -= 0.05;
            pauseImage.UpdateLayout();

            if (pauseImage.Opacity <= 0.1)
            {
                pauseTimer.Stop();
                timer.Start();
                startButton.IsEnabled = true;
                pauseButton.IsEnabled = true;
                easyRadio.IsEnabled = true;
                mediumRadio.IsEnabled = true;
                hardRadio.IsEnabled = true;
                pauseImage.Visibility = Visibility.Hidden;
            }
        }
        #endregion
    }
}
