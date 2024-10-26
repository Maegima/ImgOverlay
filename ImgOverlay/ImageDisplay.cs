using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImgOverlay {
    internal class ImageDisplay(Window window, Image image, Grid grid, ScrollBar scrollBar) {
        private readonly List<ImageLoader> images = [];
        private readonly Window parent = window;
        private readonly Image current = image;
        private readonly Grid preview = grid;
        private readonly ScrollBar scrool = scrollBar;
        private int currentIndex = -1;
        private readonly int ImageMargin = 4;
        private List<string> folders = [];
        private int currentFolderIndex = -1;

        public bool Load(string path) {
            var parent = Path.GetDirectoryName(path) ?? path;
            folders = Directory.GetDirectories(parent).ToList();
            currentFolderIndex = folders.IndexOf(path);
            
            return LoadImages(path);
        }

        public bool LoadImages(string path) {
            Clear();
            var files = Directory.EnumerateFiles(path).ToList().FindAll(file =>
                file.EndsWith(".png") |
                file.EndsWith(".jpg") |
                file.EndsWith(".jpeg") |
                file.EndsWith(".gif") |
                file.EndsWith(".tiff"));

            if (files.Count == 0) return false;

            int index = 0;
            foreach (string file in files) {
                images.Add(new ImageLoader(file, index++));
            }
            return images.First().Load();
        }

        public void Clear() { 
            images.Clear();
            preview.Children.Clear();
            preview.RowDefinitions.Clear();
            currentIndex = -1;
        }

        public void Display() {
            double height = 0;
            foreach(var image in images) {
                height += AddPreview(image, 2 * parent.ActualHeight > height);   
            }
            current.Source = images.First().Source;
            currentIndex = 1;

            scrool.Value = 0;
            scrool.Maximum = height - parent.ActualHeight;
            scrool.ViewportSize = scrool.Maximum / 15;
        }

        public double AddPreview(ImageLoader imageLoader, bool load) {
            if (load) imageLoader.Load();
            var image = new Image {
                Source = imageLoader.Source,
                Margin = new Thickness(ImageMargin),
                Tag = imageLoader
            };
            image.MouseEnter += (o, ev) => { ((Image)o).Opacity = 0.5; };
            image.MouseLeave += (o, ev) => { ((Image)o).Opacity = 1.0; };
            image.MouseDown += (o, ev) => { SetCurrent((ImageLoader)((Image)o).Tag); };
            Grid.SetRow(image, preview.Children.Count);
            preview.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Auto) });
            preview.Children.Add(image);
            return preview.Width * image.Source.Height / image.Source.Width + 2 * ImageMargin;
        }

        private void SetCurrent(ImageLoader imageLoader) {
            current.Source = imageLoader.Source;
            currentIndex = imageLoader.Index;
        }

        public void Next() {
            if (currentIndex == -1) return;
            do {
                currentIndex = (++currentIndex + images.Count) % images.Count;
            } while (!images[currentIndex].Load());
            current.Source = images[currentIndex].Source;
        }

        public void Previous() {
            if (currentIndex == -1) return;
            do {
                currentIndex = (--currentIndex + images.Count) % images.Count;
            } while (!images[currentIndex].Load());
            current.Source = images[currentIndex].Source;
        }

        public void NextFolder() {
            if(currentFolderIndex == -1) return;
            do {
                currentFolderIndex = ++currentFolderIndex % folders.Count;
            } while (!LoadImages(folders[currentFolderIndex]));
            Display();
        }

        public void PreviousFolder() {
            if (currentFolderIndex == -1) return;
            do {
                currentFolderIndex = (--currentFolderIndex + folders.Count) % folders.Count;
            } while (!LoadImages(folders[currentFolderIndex]));
            Display();
        }
    }
}
