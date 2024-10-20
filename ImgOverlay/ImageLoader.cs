using System;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ImgOverlay {
    public class ImageLoader {
        public string Path { get; set; }
        public BitmapImage Source { get; set; } = new BitmapImage();
        public bool IsLoaded { get; set; } = false;
        
        public static implicit operator bool(ImageLoader image) => image.IsLoaded;

        public ImageLoader(string path) {
            Path = path;
        }
        
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
