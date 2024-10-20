using System.IO;
using System.Windows;
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
            ImageDisplay = new ImageDisplay(DisplayImage, DisplayNext);
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

            var images = Directory.EnumerateFiles(path).ToList();
            images = images.FindAll(file => 
                file.EndsWith(".png") | 
                file.EndsWith(".jpg") | 
                file.EndsWith(".jpeg") |
                file.EndsWith(".gif") |
                file.EndsWith(".tiff"));
            
            if (images.Count == 0 || !ImageDisplay.Load(images)) {
                MessageBox.Show("The selected folder does not contains a valid image.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            ImageDisplay.Display(this.ActualHeight);
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
            if (e.Key == Key.Left) {
                ImageDisplay.Previous();
            } else if (e.Key == Key.Right) {
                ImageDisplay.Next();
            }
        }
    }
}
