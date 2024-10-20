using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ImgOverlay {
    internal class ImageDisplay(Image image, Grid grid) {
        private readonly List<ImageLoader> images = [];
        private readonly Image current = image;
        private readonly Grid preview = grid;
        private int currentIndex = -1;
        private readonly int ImageMargin = 4;

        public bool Load(List<string> paths) {
            Clear();
            foreach (string path in paths) {
                images.Add(new ImageLoader(path));
            }
            return images.First().Load();
        }

        public void Clear() { 
            images.Clear();
            preview.Children.Clear();
            preview.RowDefinitions.Clear();
            currentIndex = -1;
        }

        public void Display(double maxheight) {
            double height = 0;
            int index = 0;
            while(height < maxheight && index < images.Count) {
                height += AddPreview(images[index++]);
            }
            current.Source = images.First().Source;
            currentIndex = 1;
        }

        public double AddPreview(ImageLoader imageLoader) {
            if (!imageLoader.Load()) return 0;
            var image = new Image {
                Source = imageLoader.Source,
                Margin = new Thickness(ImageMargin)
            };
            image.MouseEnter += (o, ev) => { ((Image)o).Opacity = 0.5; };
            image.MouseLeave += (o, ev) => { ((Image)o).Opacity = 1.0; };
            image.MouseDown += (o, ev) => { current.Source = ((Image)o).Source; };
            Grid.SetRow(image, preview.Children.Count);
            preview.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Auto) });
            preview.Children.Add(image);
            return preview.Width * image.Source.Height / image.Source.Width + 2 * ImageMargin;
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
    }
}
