using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace ImgOverlay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly ControlPanel cp = new ControlPanel();
        readonly ImageDisplay ImageDisplay;
        
        public MainWindow()
        {
            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
            InitializeComponent();
            ImageDisplay = new ImageDisplay(this, DisplayImage, DisplayNext, ScrollPreview);
        }

        public void LoadImage(string path) {
            if (System.IO.Directory.Exists(path)) {
                MessageBox.Show("Cannot open folders.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!System.IO.File.Exists(path)) {
                MessageBox.Show("The selected image file does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var image = new ImageLoader(path);
            if (!image.Load()) {
                MessageBox.Show("Error loading image. Perhaps its format is unsupported?", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            DisplayImage.Source = image.Source;
            ImageDisplay.Clear();
        }
        
        public void LoadFolder(string path) {
            if (!System.IO.Directory.Exists(path)) {
                MessageBox.Show("The selected image folder does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!ImageDisplay.Load(path)) {
                MessageBox.Show("The selected folder does not contains a valid image.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            ImageDisplay.Display();
        }

        public void ChangeOpacity(float opacity)
        {
            DisplayImage.Opacity = opacity;
        }

        public void ChangeRotation(float angle)
        {
            // Create a transform to rotate the button
            RotateTransform myRotateTransform = new RotateTransform();

            // Set the rotation of the transform.
            myRotateTransform.Angle = angle;

            // Create a TransformGroup to contain the transforms
            // and add the transforms to it.
            TransformGroup myTransformGroup = new TransformGroup();
            myTransformGroup.Children.Add(myRotateTransform);

            DisplayImage.RenderTransformOrigin = new Point(0.5, 0.5);
            // Associate the transforms to the button.
            DisplayImage.RenderTransform = myTransformGroup;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
                if(cp.LeftValue != null && cp.TopValue != null)
                {
                    cp.LeftValue.Content = this.Left;
                    cp.TopValue.Content = this.Top;
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cp.Owner = this;
            cp.Show();
            cp.Closed += (o, ev) =>
            {
                this.Close();
            };
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            WindowsServices.SetWindowExTransparent(hwnd);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (cp.HeightValue != null && cp.WidthValue != null)
            {
                cp.WidthValue.Content = e.NewSize.Width;
                cp.HeightValue.Content = e.NewSize.Height;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e) {
            switch(e.Key) {
                case Key.Left:
                    ImageDisplay.Previous(); break;
                case Key.Right:
                    ImageDisplay.Next(); break;
                case Key.Up:
                    ImageDisplay.PreviousFolder(); break;
                case Key.Down:
                    ImageDisplay.NextFolder(); break;
                case Key.M:
                    cp.DragButton.IsChecked = false;
                    cp.DragImageWindow(false);
                    cp.Activate();
                    break;
            }
        }

        private void ScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            var sc = (ScrollBar)sender;
            double percent = (ScrollPreview.Value + this.Height) / ScrollPreview.Maximum;
            int until = percent > 1 ? DisplayNext.Children.Count : (int)(DisplayNext.Children.Count * percent);
            for (int i = 0; i < until; i++) {
                var image = (Image)DisplayNext.Children[i];
                var loader = (ImageLoader)image.Tag;
                if (!loader.IsLoaded) {
                    loader.Load();
                    image.Source = loader.Source;
                }
            }
            double height = 0;
            foreach(var child in DisplayNext.Children) {
                var image = (Image)child;
                height += image.ActualHeight;
            }
            sc.Maximum = height - this.Height  + 250;
            DisplayNext.Margin = new Thickness(0, -sc.Value, 0, 0);
        }

        private void DisplayNext_MouseWheel(object sender, MouseWheelEventArgs e) {
            if (e.Delta != 0) {
                ScrollPreview.Value -= e.Delta;
                if (ScrollPreview.Value < 0) ScrollPreview.Value = 0;
                if (ScrollPreview.Value > ScrollPreview.Maximum) ScrollPreview.Value = ScrollPreview.Maximum;
            }
        }
    }
}
