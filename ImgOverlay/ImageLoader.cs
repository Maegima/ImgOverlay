using System;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace ImgOverlay {
    public class ImageLoader(string path, int index = -1) {
        public static readonly BitmapImage Default = new(new Uri("pack://application:,,,/none.png"));
        public string Path { get; set; } = path;
        public BitmapImage Source { get; set; } = Default;
        public bool IsLoaded { get; set; } = false;
        public int Index = index;

        public static implicit operator bool(ImageLoader image) => image.IsLoaded;

        public bool Load() {
            if(IsLoaded) return true;
            try {
                Source = new BitmapImage(new Uri(Path));
                IsLoaded = true;
            } catch (Exception) {
                IsLoaded = false;
            }
            return IsLoaded;
        }
    }
}
