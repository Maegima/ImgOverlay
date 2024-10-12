using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ImgOverlay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ControlPanel cp = new ControlPanel();
        List<String> images;
        int currentIndex = -1;

        public MainWindow()
        {
            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;

            InitializeComponent();
        }

        public void LoadImage(string path) {
            currentIndex = -1;
            if (System.IO.Directory.Exists(path)) {
                MessageBox.Show("Cannot open folders.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!System.IO.File.Exists(path)) {
                MessageBox.Show("The selected image file does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if(!TryLoadImage(path)) {
                MessageBox.Show("Error loading image. Perhaps its format is unsupported?", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private bool TryLoadImage(string filepath) {
            var img = new BitmapImage();
            try {
                img.BeginInit();
                img.UriSource = new Uri(filepath);
                img.EndInit();
            } catch (Exception e) {
                return false;
            }

            DisplayImage.Source = img;
            return true;
        }

        public void LoadFolder(string path) {
            if (!System.IO.Directory.Exists(path)) {
                MessageBox.Show("The selected image folder does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            images = Directory.EnumerateFiles(path).ToList();
            images = images.FindAll(file => 
                file.EndsWith(".png") | 
                file.EndsWith(".jpg") | 
                file.EndsWith(".jpeg") |
                file.EndsWith(".gif") |
                file.EndsWith(".tiff"));
            if(images.Count == 0 || !TryLoadImage(images.First())) {
                MessageBox.Show("The selected folder does not contains a valid image.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            currentIndex = 0;
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
            if (currentIndex < 0 && images.Count > 0) return;
            if (e.Key == Key.Left || e.Key == Key.Right) {
                do {
                    currentIndex = ((e.Key == Key.Left ? --currentIndex : ++currentIndex) + images.Count) % images.Count;
                } while (!TryLoadImage(images[currentIndex]));
            }
        }
    }
}
