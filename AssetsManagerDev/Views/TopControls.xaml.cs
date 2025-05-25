using System;
using System.Windows;
using System.Windows.Controls;

namespace AssetsManagerDev.Views
{
    public partial class TopControls : UserControl
    {
        public event RoutedEventHandler AddAssetClicked;
        public event TextChangedEventHandler SearchBoxTextChanged;

        public TopControls()
        {
            InitializeComponent();
        }

        private void AddAsset_Click(object sender, RoutedEventArgs e)
        {
            AddAssetClicked?.Invoke(this, e);
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchBoxTextChanged?.Invoke(this, e);
        }

        public string SearchText => SearchBox.Text;
    }
}
