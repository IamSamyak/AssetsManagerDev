using AssetsManager.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace AssetsManagerDev.Views
{
    public partial class AssetPreviewControl : UserControl
    {
        public AssetPreviewControl()
        {
            InitializeComponent();
        }

        public void ShowPreview(Asset asset)
        {
            string ext = System.IO.Path.GetExtension(asset.FilePath).ToLower();

            ImageNameTextBlock.Text = asset.DisplayName;

            if (ext == ".svg")
            {
                ImagePreview.Visibility = Visibility.Collapsed;
                SvgPlaceholder.Visibility = Visibility.Visible;
                ImageSizeTextBlock.Text = "SVG format – preview not supported.";
            }
            else
            {
                try
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(asset.FilePath, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    ImagePreview.Source = bitmap;
                    ImagePreview.Visibility = Visibility.Visible;
                    SvgPlaceholder.Visibility = Visibility.Collapsed;
                    ImageSizeTextBlock.Text = $"Dimensions: {bitmap.PixelWidth} x {bitmap.PixelHeight}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to load image: {ex.Message}");
                    return;
                }
            }

            PreviewGrid.Visibility = Visibility.Visible;
        }

        private void ClosePreview_Click(object sender, RoutedEventArgs e)
        {
            PreviewGrid.Visibility = Visibility.Collapsed;
            ImagePreview.Source = null;
            SvgPlaceholder.Visibility = Visibility.Collapsed;
        }
    }
}
