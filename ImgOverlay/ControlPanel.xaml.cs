﻿using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;

namespace ImgOverlay
{
    /// <summary>
    /// Interaction logic for ControlPanel.xaml
    /// </summary>
    public partial class ControlPanel : Window
    {
        public ControlPanel()
        {
            InitializeComponent();
        }

        private void DragButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton? toggleButton = sender as ToggleButton;
            if (Owner == null || toggleButton == null)
                return;
            bool opaque = toggleButton.IsChecked != null && toggleButton.IsChecked.Value;
            Owner.IsHitTestVisible = opaque;

            var hwnd = new WindowInteropHelper(Owner).Handle;
            if (opaque)
            {
                WindowsServices.SetWindowExOpaque(hwnd);
            }
            else
            {
                WindowsServices.SetWindowExTransparent(hwnd);
            }

            e.Handled = true;
        }

        private void LoadFileButton_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog();
            if (openDialog.ShowDialog() == true)
            {
                LoadedFile.Text = openDialog.FileName;
                (Owner as MainWindow)?.LoadImage(openDialog.FileName);
            }
        }

        private void LoadFolderButton_Click(object sender, RoutedEventArgs e) {
            var openDialog = new OpenFolderDialog();
            if (openDialog.ShowDialog() == true) {
                LoadedFile.Text = openDialog.FolderName;
                (Owner as MainWindow)?.LoadFolder(openDialog.FolderName);
            }
        }

        private void OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            (Owner as MainWindow)?.ChangeOpacity((float)e.NewValue);
            if(OpacityValue != null)
                OpacityValue.Content = e.NewValue.ToString();
        }

        private void RotateSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            (Owner as MainWindow)?.ChangeRotation((float)e.NewValue);
            if(RotateValue != null)
                RotateValue.Content = e.NewValue.ToString();
            }

        private void ControlPanel_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] s = (string[]) e.Data.GetData(DataFormats.FileDrop, false);
                if (s.Length == 1)
                {
                    e.Effects = DragDropEffects.Move;
                }
                else
                {
                    e.Effects = DragDropEffects.None;
                }
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void ControlPanel_Drop(object sender, DragEventArgs e)
        {
            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            if (s.Length == 1)
            {
                (Owner as MainWindow)?.LoadImage(s[0]);
            }
        }
    }
}
