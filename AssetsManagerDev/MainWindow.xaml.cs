using AssetsManager.Data;
using AssetsManager.Models;
using AssetsManagerDev.Views;
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
        private string assetStorageBasePath;
        private readonly string configFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AssetsManagerDev", "config.txt");

        private readonly string[] assetTypes = new[] { "animations", "svg", "png", "jpg", "jpeg", "gif", "other" };

        private List<string> selectedCategories = new();
        private readonly AssetDatabaseManager dbManager;

        public MainWindow()
        {
            InitializeComponent();
            EnsureAssetStorageInitialized();

            dbManager = new AssetDatabaseManager();

            TopControlsPanel.AddAssetClicked += AddAsset_Click;
            TopControlsPanel.SearchBoxTextChanged += SearchBox_TextChanged;
            RefreshAssetList();
        }

        private void EnsureAssetStorageInitialized()
        {
            if (File.Exists(configFilePath))
            {
                assetStorageBasePath = File.ReadAllText(configFilePath);
                if (!Directory.Exists(assetStorageBasePath))
                {
                    AskUserToSelectAssetFolder();
                }
            }
            else
            {
                AskUserToSelectAssetFolder();
            }

            foreach (var type in assetTypes)
            {
                Directory.CreateDirectory(Path.Combine(assetStorageBasePath, type));
            }
        }

        private void AskUserToSelectAssetFolder()
        {
            var dlg = new OpenFileDialog
            {
                Title = "Select a folder to store your assets",
                Filter = "Folders|\n",
                CheckFileExists = false,
                FileName = "Select this folder"
            };

            if (dlg.ShowDialog() == true)
            {
                string selectedFolder = Path.GetDirectoryName(dlg.FileName);
                assetStorageBasePath = Path.Combine(selectedFolder, "AssetsManager");
                Directory.CreateDirectory(assetStorageBasePath);

                string configDirectory = Path.GetDirectoryName(configFilePath);
                if (!Directory.Exists(configDirectory))
                {
                    Directory.CreateDirectory(configDirectory);
                }

                File.WriteAllText(configFilePath, assetStorageBasePath);
            }
            else
            {
                MessageBox.Show("You must select a folder to use the app.", "Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                Application.Current.Shutdown();
            }
        }

        private void AddAsset_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title = "Select Asset",
                Filter = "Supported Files|*.png;*.jpg;*.jpeg;*.svg;*.gif;*.json",
                Multiselect = true
            };

            if (dlg.ShowDialog() == true)
            {
                foreach (string originalFilePath in dlg.FileNames)
                {
                    string extension = Path.GetExtension(originalFilePath).ToLower();
                    string category = GetCategoryFromExtension(extension);
                    string destinationFolder = Path.Combine(assetStorageBasePath, category);
                    Directory.CreateDirectory(destinationFolder);

                    string displayName = Path.GetFileName(originalFilePath);
                    string storedFileName = displayName;
                    string storedFilePath = Path.Combine(destinationFolder, storedFileName);

                    int counter = 1;
                    string baseName = Path.GetFileNameWithoutExtension(displayName);
                    string ext = Path.GetExtension(displayName);

                    while (File.Exists(storedFilePath))
                    {
                        storedFileName = $"{baseName}_{counter++}{ext}";
                        storedFilePath = Path.Combine(destinationFolder, storedFileName);
                    }

                    File.Copy(originalFilePath, storedFilePath);

                    if (!dbManager.IsAssetAlreadyAdded(storedFilePath))
                    {
                        dbManager.InsertAsset(displayName, storedFilePath, category);
                    }
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
            string searchText = TopControlsPanel.SearchText?.ToLower() ?? "";
            var filteredAssetTuples = dbManager.GetFilteredAssets(searchText, selectedCategories);

            var filteredAssets = filteredAssetTuples.Select(t => new Asset
            {
                DisplayName = t.DisplayName,
                FilePath = t.FilePath,
                Category = "" // Category not returned by current method, so leave empty or modify your DB query.
            }).ToList();

            AssetsWrapPanel.Children.Clear();

            if (filteredAssets.Count == 0)
            {
                // Show no assets message
                var noAssetsText = new TextBlock
                {
                    Text = "No assets to show",
                    Foreground = System.Windows.Media.Brushes.Gray,
                    FontStyle = FontStyles.Italic,
                    FontSize = 16,
                    Margin = new Thickness(10)
                };
                AssetsWrapPanel.Children.Add(noAssetsText);
                ImagePreview.Visibility = Visibility.Collapsed;
                SvgPlaceholder.Visibility = Visibility.Collapsed;
                return;
            }

            ImagePreview.Visibility = Visibility.Collapsed;
            SvgPlaceholder.Visibility = Visibility.Collapsed;

            foreach (var asset in filteredAssets)
            {
                var card = CreateAssetCard(asset);
                AssetsWrapPanel.Children.Add(card);
            }
        }

        private string GetCategoryFromExtension(string extension)
        {
            return extension switch
            {
                ".json" => "animations",
                ".svg" => "svg",
                ".png" => "png",
                ".jpg" or ".jpeg" => "jpg",
                ".gif" => "gif",
                _ => "other"
            };
        }

        private void CategoryListControl_SelectedCategoriesChanged(object sender, RoutedEventArgs e)
        {
            if (sender is AssetCategoryList control)
            {
                selectedCategories = control.SelectedCategories;
                RefreshAssetList();
            }
        }

        private UIElement CreateAssetCard(Asset asset)
        {
            var stackPanel = new StackPanel
            {
                Width = 120,
                Height = 150,
                Margin = new Thickness(5),
                Cursor = System.Windows.Input.Cursors.Hand
            };

            var image = new Image
            {
                Width = 100,
                Height = 100,
                Margin = new Thickness(10, 10, 10, 5),
                Stretch = System.Windows.Media.Stretch.UniformToFill
            };

            string ext = Path.GetExtension(asset.FilePath).ToLower();

            if (ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".gif")
            {
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(asset.FilePath, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    image.Source = bitmap;
                }
                catch
                {
                    // Ignore load errors or set a placeholder image
                }
            }
            else if (ext == ".svg")
            {
                // For SVG you could show a placeholder or handle SVG rendering if you want
                image.Source = null; // or set a placeholder image
            }
            else
            {
                image.Source = null;
            }

            var textBlock = new TextBlock
            {
                Text = asset.DisplayName,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 12,
                Margin = new Thickness(5, 0, 5, 0)
            };

            stackPanel.Children.Add(image);
            stackPanel.Children.Add(textBlock);

            // Add click event to select the asset (like your current ListBox selection)
            stackPanel.MouseLeftButtonUp += (s, e) =>
            {
                ShowAssetPreview(asset);
            };

            return stackPanel;
        }

        private void ShowAssetPreview(Asset asset)
        {
            if (asset == null || string.IsNullOrEmpty(asset.FilePath) || !File.Exists(asset.FilePath))
                return;

            string extension = Path.GetExtension(asset.FilePath).ToLower();

            if (extension == ".png" || extension == ".jpg" || extension == ".jpeg")
            {
                ImagePreview.Source = new BitmapImage(new Uri(asset.FilePath));
                ImagePreview.Visibility = Visibility.Visible;
                SvgPlaceholder.Visibility = Visibility.Collapsed;
            }
            else if (extension == ".svg")
            {
                ImagePreview.Visibility = Visibility.Collapsed;
                SvgPlaceholder.Visibility = Visibility.Visible;
                SvgPlaceholder.Text = $"[Preview SVG: {Path.GetFileName(asset.FilePath)}]";
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
