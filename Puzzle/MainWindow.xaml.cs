using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using Puzzle.Game;
using Puzzle.HelperClasses;

namespace Puzzle
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region variables

        public List<List<Thumb>> unionList = new List<List<Thumb>>();
        public GameDetails gd = new GameDetails(Difficulty.Hard);
        public bool start = true;
        public ObservableCollection<listItems> _easyList = new ObservableCollection<listItems>(),
            _mediumList = new ObservableCollection<listItems>(),
            _hardList = new ObservableCollection<listItems>();
        int zCoordinate = int.MinValue + 1;
        public Stream stream;
        DropShadowBitmapEffect shadowEffect;
        Thumb[,] thumbTab;
        private const int TOLERATION = 20;
        private Point LEFT = new Point() { X = -1, Y = 0 };
        private Point RIGHT = new Point() { X = 1, Y = 0 };
        private Point UP = new Point() { X = 0, Y = -1 };
        private Point DOWN = new Point() { X = 0, Y = 1 };

        public ObservableCollection<listItems> hardList
        { get { return _hardList; } }

        public ObservableCollection<listItems> mediumList
        { get { return _mediumList; } }

        public ObservableCollection<listItems> easyList
        { get { return _easyList; } }

        #endregion

        public MainWindow()
        {
            InitializeComponent();

            setTimers();
            setSortDescriptions();

            shadowEffect = new DropShadowBitmapEffect()
            {
                Color = Colors.Black,
                Direction = 300,
                ShadowDepth = 25,
                Softness = 1,
                Opacity = 0.5
            };
        }

        private void setSortDescriptions()
        {
            easyListView.Items.SortDescriptions.Add(new SortDescription("counter", ListSortDirection.Descending));
            easyListView.Items.SortDescriptions.Add(new SortDescription("time", ListSortDirection.Ascending));
            mediumListView.Items.SortDescriptions.Add(new SortDescription("counter", ListSortDirection.Descending));
            mediumListView.Items.SortDescriptions.Add(new SortDescription("time", ListSortDirection.Ascending));
            hardListView.Items.SortDescriptions.Add(new SortDescription("counter", ListSortDirection.Descending));
            hardListView.Items.SortDescriptions.Add(new SortDescription("time", ListSortDirection.Ascending));
        }

        #region buttons
        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            if (start)
            {
	            Difficulty difficulty;
	            if (hardRadio.IsChecked.Value) difficulty = Difficulty.Hard;
	            else if (mediumRadio.IsChecked.Value) difficulty = Difficulty.Medium;
	            else difficulty = Difficulty.Easy;
                gd = new GameDetails(difficulty);
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                dlg.Filter = "Image Files (*.bmp, *.jpg, *.png)|*.bmp;*.jpg;*.png";

                if (dlg.ShowDialog() == true)
                {
                    BitmapImage image = openChosenFile(dlg.FileName);
                    if (image == null)
                        return;

                    createPuzzle(image);
                    new_timer();

                    startButton.Content = "End game";
                    pauseButton.IsEnabled = true;
                    start = false;
                }
            }
            else
            {
                timer.Stop();
                EndWindow ew = new EndWindow();
                ew.Owner = this;
                ew.ShowInTaskbar = false;
                ew.ShowDialog();
            }
        }

        private BitmapImage openChosenFile(string file)
        {
            try
            {
                BitmapImage imgsrc = new BitmapImage();
                stream = File.Open(file, FileMode.Open);

                imgsrc.BeginInit();
                imgsrc.StreamSource = stream;
                imgsrc.EndInit();
                imgsrc.Freeze();
                return imgsrc;
            }
            catch (Exception exc)
            {
                stream.Close();
                image.Children.Clear();
                MessageBox.Show("An problem occured while opening the file " + exc.Message);
            }
            return null;
        }

        private void createPuzzle(BitmapImage imgsrc)
        {
            Random rnd = new Random();
            Random angle = new Random();
            int[] angles = new int[] { 0, 90, 180, 270 };
            thumbTab = new Thumb[gd.VerticalPuzzleCount, gd.HorizontalPuzzleCount];

            for (int i = 0; i < gd.VerticalPuzzleCount; i++)
                for (int j = 0; j < gd.HorizontalPuzzleCount; j++)
                {
                    CroppedBitmap cb = new CroppedBitmap(imgsrc
                        , new Int32Rect(j * (int)imgsrc.PixelWidth / gd.HorizontalPuzzleCount, i * (int)imgsrc.PixelHeight / gd.VerticalPuzzleCount
							, (int)imgsrc.PixelWidth / gd.HorizontalPuzzleCount, (int)imgsrc.PixelHeight / gd.VerticalPuzzleCount));
                    ImageBrush imgBrush = new ImageBrush(cb);
                    int rotationAng = angles[angle.Next(3)];

                    createThumb(i, j, imgBrush, angles, rotationAng, rnd);
                }
        }

        private void createThumb(int i, int j, ImageBrush imgBrush, int[] angles, int rotationAng, Random rnd)
        {
            imgBrush.Transform = new RotateTransform(rotationAng, -1 + (gd.PuzzleSize - 1) / 2, -1 + (gd.PuzzleSize - 1) / 2);
            imgBrush.Stretch = Stretch.Fill;

            Thumb thmb = new Thumb() { Width = gd.PuzzleSize, Height = gd.PuzzleSize };
            Canvas.SetLeft(thmb, rnd.NextDouble() * (image.ActualWidth - gd.PuzzleSize));
            Canvas.SetTop(thmb, rnd.NextDouble() * (image.ActualHeight - gd.PuzzleSize));
            RotateTransform rt = new RotateTransform(angles[rnd.Next(3)]);

            Canvas.SetZIndex(thmb, int.MinValue);
            thmb.Background = imgBrush;
            List<Thumb> newList = new List<Thumb>();
            newList.Add(thmb);
            unionList.Add(newList);
            thmb.Tag = new thumbTag { ib = imgBrush, rotationAngle = rotationAng, x = j, y = i, unionNr = i * gd.HorizontalPuzzleCount + j, listName = newList };

            thumbTab[i, j] = thmb;
            setThumbEventHandlers(thmb);
            image.Children.Add(thmb);
        }

        private void setThumbEventHandlers(Thumb thmb)
        {
            thmb.DragStarted += thmb_DragStarted;
            thmb.DragDelta += thmb_DragDelta;
            thmb.DragCompleted += thmb_DragCompleted;
            thmb.MouseRightButtonDown += thmb_MouseRightButtonDown;
        }

        void thmb_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Thumb thumb = (Thumb)sender;
            thumbTag tt = (thumbTag)thumb.Tag;

            if (tt.listName.Count == 1)
            {
                ImageBrush imgBrush = tt.ib;

                RotateTransform tran = imgBrush.Transform as RotateTransform;
                tt.rotationAngle = (tt.rotationAngle + 90) % 360;

                tran.Angle += 90;
                tran.CenterX = -1 + (gd.PuzzleSize - 1) / 2;
                tran.CenterY = -1 + (gd.PuzzleSize - 1) / 2;
            }
        }

        void endGame()
        {
            timer.Stop();
            gd.FinishGame();
            EndWindow ew = new EndWindow();
            ew.Owner = this;
            ew.ShowInTaskbar = false;
            ew.ShowDialog();
        }

        void thmb_DragStarted(object sender, DragStartedEventArgs e)
        {
            Thumb thmb = sender as Thumb;
            thumbTag tt = (thumbTag)thmb.Tag;

            for (int i = 0; i < tt.listName.Count; i++)
            {
                Canvas.SetZIndex(tt.listName[i], zCoordinate);
            }
            zCoordinate++;
        }

        void thmb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Thumb thmb = sender as Thumb;
            thumbTag tt = (thumbTag)thmb.Tag;
            bool moveVertical = true, moveHorizontal = true;

            for (int i = 0; i < tt.listName.Count; i++)
            {
                if (Canvas.GetTop(tt.listName[i]) + e.VerticalChange <= 0
                    || Canvas.GetTop(tt.listName[i]) + e.VerticalChange >= rectangle.ActualHeight - gd.PuzzleSize)
                    moveVertical = false;
                if (Canvas.GetLeft(tt.listName[i]) + e.HorizontalChange > rectangle.ActualWidth - gd.PuzzleSize
				|| Canvas.GetLeft(tt.listName[i]) + e.HorizontalChange <= 0)
                    moveHorizontal = false;
            }

            for (int i = 0; i < tt.listName.Count; i++)
            {
                if (moveVertical == true)
                    Canvas.SetTop(tt.listName[i], Canvas.GetTop(tt.listName[i]) + e.VerticalChange);
                if (moveHorizontal == true)
                    Canvas.SetLeft(tt.listName[i], Canvas.GetLeft(tt.listName[i]) + e.HorizontalChange);
                tt.listName[i].BitmapEffect = shadowEffect;
            }
        }

        void thmb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            Thumb thmb = sender as Thumb;
            thumbTag tTag = (thumbTag)thmb.Tag;
            int counter = tTag.listName.Count;

            if (tTag.rotationAngle == 0)
                connectPuzzles(tTag);

            for (int j = 0; j < tTag.listName.Count; j++)
                tTag.listName[j].BitmapEffect = null;
        }

        private void connectPuzzles(thumbTag tTag)
        {
            for (int i = 0; i < tTag.listName.Count; i++)
            {
                Thumb thmb = tTag.listName[i];
                tTag = (thumbTag)thmb.Tag;
                double thmbLeft = Canvas.GetLeft(thmb);
                double thmbTop = Canvas.GetTop(thmb);
                if (tTag.y < gd.VerticalPuzzleCount - 1)
                    chckThumb(DOWN, thmb, tTag, thmbLeft, thmbTop);

                if (tTag.y > 0)
                    chckThumb(UP, thmb, tTag, thmbLeft, thmbTop);

                if (tTag.x > 0)
                    chckThumb(LEFT, thmb, tTag, thmbLeft, thmbTop);

                if (tTag.x < gd.HorizontalPuzzleCount - 1)
                    chckThumb(RIGHT, thmb, tTag, thmbLeft, thmbTop);

                if (unionList.Count == 1)
                    endGame();
            }
        }

        private void chckThumb(Point direction, Thumb thmb, thumbTag tTag, double thmbLeft, double thmbTop)
        {
            Thumb checkThumb = thumbTab[tTag.y + (int)direction.Y, tTag.x + (int)direction.X];
            thumbTag checkTTag = (thumbTag)checkThumb.Tag;
            if (checkTTag.listName != tTag.listName)
                if (checkTTag.rotationAngle == 0
                    && Canvas.GetTop(checkThumb) < Canvas.GetTop(thmb) + (int)direction.Y * gd.PuzzleSize + TOLERATION
                    && Canvas.GetTop(checkThumb) > Canvas.GetTop(thmb) + (int)direction.Y * gd.PuzzleSize - TOLERATION
                    && Canvas.GetLeft(checkThumb) < Canvas.GetLeft(thmb) + (int)direction.X * gd.PuzzleSize + TOLERATION
                    && Canvas.GetLeft(checkThumb) > Canvas.GetLeft(thmb) + (int)direction.X * gd.PuzzleSize - TOLERATION)
                {
                    List<Thumb> l = checkTTag.listName;
                    while (l.Count != 0)
                    {
                        tTag.listName.Add(l[l.Count - 1]);
                        thumbTag ltt = (thumbTag)l[l.Count - 1].Tag;
                        ltt.listName = tTag.listName;
                        l.Remove(l[l.Count - 1]);
                    }

                    unionList.Remove(l);

                    Canvas.SetLeft(thmb, Canvas.GetLeft(checkThumb) - (int)direction.X * gd.PuzzleSize);
                    Canvas.SetTop(thmb, Canvas.GetTop(checkThumb) - (int)direction.Y * gd.PuzzleSize);

                    double deltaX = Canvas.GetLeft(thmb) - thmbLeft;
                    double deltaY = Canvas.GetTop(thmb) - thmbTop;

                    for (int j = 0; j < tTag.listName.Count && tTag.listName[j] != thmb && tTag.listName[j] != checkThumb; j++)
                    {
                        Canvas.SetLeft(tTag.listName[j], Canvas.GetLeft(tTag.listName[j]) - deltaX);
                        Canvas.SetTop(tTag.listName[j], Canvas.GetTop(tTag.listName[j]) - deltaY);
                    }
                }
        }

        private void pauseButton_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            startButton.IsEnabled = false;
            pauseButton.IsEnabled = false;
            easyRadio.IsEnabled = false;
            mediumRadio.IsEnabled = false;
            hardRadio.IsEnabled = false;
            pauseImage.Opacity = 0.8;
            pauseImage.Visibility = Visibility.Visible;
        }

        #endregion
    }
}
