using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace AssetsManager
{
    public partial class MainWindow : Window
    {
        private List<string> allAssetPaths = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void AddAsset_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                Title = "Select Asset",
                Filter = "Supported Files|*.png;*.jpg;*.jpeg;*.svg",
                Multiselect = true
            };

            if (dlg.ShowDialog() == true)
            {
                foreach (string file in dlg.FileNames)
                {
                    if (!allAssetPaths.Contains(file))
                        allAssetPaths.Add(file);
                }

                RefreshAssetList();
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshAssetList();
        }

        private void RefreshAssetList()
        {
            string searchText = SearchBox.Text.ToLower();
            var filtered = allAssetPaths
                .Where(path => string.IsNullOrWhiteSpace(searchText) || Path.GetFileName(path).ToLower().Contains(searchText))
                .ToList();

            if (filtered.Count == 0)
            {
                AssetsListBox.ItemsSource = new List<string> { "No assets to show" };
                AssetsListBox.IsEnabled = false;
                ImagePreview.Visibility = Visibility.Collapsed;
                SvgPlaceholder.Visibility = Visibility.Collapsed;
            }
            else
            {
                AssetsListBox.ItemsSource = filtered.Select(p => Path.GetFileName(p)).ToList();
                AssetsListBox.IsEnabled = true;
            }
        }

        private void AssetsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!AssetsListBox.IsEnabled || AssetsListBox.SelectedItem == null)
                return;

            string selectedFileName = AssetsListBox.SelectedItem.ToString();
            string fullPath = allAssetPaths.FirstOrDefault(p => Path.GetFileName(p) == selectedFileName);

            if (string.IsNullOrWhiteSpace(fullPath) || !File.Exists(fullPath))
                return;

            string extension = Path.GetExtension(fullPath).ToLower();

            if (extension == ".png" || extension == ".jpg" || extension == ".jpeg")
            {
                ImagePreview.Source = new BitmapImage(new Uri(fullPath));
                ImagePreview.Visibility = Visibility.Visible;
                SvgPlaceholder.Visibility = Visibility.Collapsed;
            }
            else if (extension == ".svg")
            {
                ImagePreview.Visibility = Visibility.Collapsed;
                SvgPlaceholder.Visibility = Visibility.Visible;
                SvgPlaceholder.Text = $"[Preview SVG: {Path.GetFileName(fullPath)}]";
            }
            else
            {
                ImagePreview.Visibility = Visibility.Collapsed;
                SvgPlaceholder.Visibility = Visibility.Visible;
                SvgPlaceholder.Text = "Unsupported file format.";
            }
        }
    }
}