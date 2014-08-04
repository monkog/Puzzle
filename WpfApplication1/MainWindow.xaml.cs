using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Collections.ObjectModel;
using System.Windows.Media.Effects;
using System.ComponentModel;
using System.Windows.Controls.Primitives;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region variables

        public List<List<Thumb>> unionList = new List<List<Thumb>>();
        public gameDetails gd = new gameDetails();
        public bool start = true;
        public ObservableCollection<listItems> _easyList = new ObservableCollection<listItems>(),
            _mediumList = new ObservableCollection<listItems>(),
            _hardList = new ObservableCollection<listItems>();
        public int maxCount = 108, maxWidth = 12, maxHeight = 9;
        public char level = 'h';
        public int puzzleSize = 50;
        int zCoordinate = int.MinValue + 1;
        public Stream stream = null;
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
                gd = new gameDetails { gameLevel = level, gameMaxCounter = maxCount, currentGameCounter = 1 };
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
            thumbTab = new Thumb[maxHeight, maxWidth];

            for (int i = 0; i < maxHeight; i++)
                for (int j = 0; j < maxWidth; j++)
                {
                    CroppedBitmap cb = new CroppedBitmap(imgsrc
                        , new Int32Rect(j * (int)imgsrc.PixelWidth / maxWidth, i * (int)imgsrc.PixelHeight / maxHeight
                            , (int)imgsrc.PixelWidth / maxWidth, (int)imgsrc.PixelHeight / maxHeight));
                    ImageBrush imgBrush = new ImageBrush(cb);
                    int rotationAng = angles[angle.Next(3)];

                    createThumb(i, j, imgBrush, angles, rotationAng, rnd);
                }
        }

        private void createThumb(int i, int j, ImageBrush imgBrush, int[] angles, int rotationAng, Random rnd)
        {
            imgBrush.Transform = new RotateTransform(rotationAng, -1 + (puzzleSize - 1) / 2, -1 + (puzzleSize - 1) / 2);
            imgBrush.Stretch = Stretch.Fill;

            Thumb thmb = new Thumb() { Width = puzzleSize, Height = puzzleSize };
            Canvas.SetLeft(thmb, rnd.NextDouble() * (image.ActualWidth - puzzleSize));
            Canvas.SetTop(thmb, rnd.NextDouble() * (image.ActualHeight - puzzleSize));
            RotateTransform rt = new RotateTransform(angles[rnd.Next(3)]);

            Canvas.SetZIndex(thmb, int.MinValue);
            thmb.Background = imgBrush;
            List<Thumb> newList = new List<Thumb>();
            newList.Add(thmb);
            unionList.Add(newList);
            thmb.Tag = new thumbTag { ib = imgBrush, rotationAngle = rotationAng, x = j, y = i, unionNr = i * maxWidth + j, listName = newList };

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
                tran.CenterX = -1 + (puzzleSize - 1) / 2;
                tran.CenterY = -1 + (puzzleSize - 1) / 2;
            }
        }

        void endGame()
        {
            timer.Stop();
            gd.currentGameCounter = gd.gameMaxCounter;
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
                    || Canvas.GetTop(tt.listName[i]) + e.VerticalChange >= rectangle.ActualHeight - puzzleSize)
                    moveVertical = false;
                if (Canvas.GetLeft(tt.listName[i]) + e.HorizontalChange > rectangle.ActualWidth - puzzleSize
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
                if (tTag.y < maxHeight - 1)
                    chckThumb(DOWN, thmb, tTag, thmbLeft, thmbTop);

                if (tTag.y > 0)
                    chckThumb(UP, thmb, tTag, thmbLeft, thmbTop);

                if (tTag.x > 0)
                    chckThumb(LEFT, thmb, tTag, thmbLeft, thmbTop);

                if (tTag.x < maxWidth - 1)
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
                    && Canvas.GetTop(checkThumb) < Canvas.GetTop(thmb) + (int)direction.Y * puzzleSize + TOLERATION
                    && Canvas.GetTop(checkThumb) > Canvas.GetTop(thmb) + (int)direction.Y * puzzleSize - TOLERATION
                    && Canvas.GetLeft(checkThumb) < Canvas.GetLeft(thmb) + (int)direction.X * puzzleSize + TOLERATION
                    && Canvas.GetLeft(checkThumb) > Canvas.GetLeft(thmb) + (int)direction.X * puzzleSize - TOLERATION)
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

                    Canvas.SetLeft(thmb, Canvas.GetLeft(checkThumb) - (int)direction.X * puzzleSize);
                    Canvas.SetTop(thmb, Canvas.GetTop(checkThumb) - (int)direction.Y * puzzleSize);

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

        #region radioButtons

        private void hardRadio_Checked(object sender, RoutedEventArgs e)
        {
            level = 'h';
            maxCount = 108;
            puzzleSize = 46;
            maxWidth = 12;
            maxHeight = 8;
        }

        private void mediumRadio_Checked(object sender, RoutedEventArgs e)
        {
            level = 'm';
            maxCount = 48;
            puzzleSize = 71;
            maxWidth = 8;
            maxHeight = 6;
        }

        private void easyRadio_Checked(object sender, RoutedEventArgs e)
        {
            level = 'e';
            maxCount = 12;
            puzzleSize = 146;
            maxWidth = 4;
            maxHeight = 3;
        }

        #endregion
    }
}
