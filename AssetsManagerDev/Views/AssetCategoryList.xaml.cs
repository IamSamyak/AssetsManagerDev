using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace AssetsManagerDev.Views
{
    public partial class AssetCategoryList : UserControl
    {
        public static readonly RoutedEvent SelectedCategoriesChangedEvent = EventManager.RegisterRoutedEvent(
            nameof(SelectedCategoriesChanged),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(AssetCategoryList));

        public event RoutedEventHandler SelectedCategoriesChanged
        {
            add => AddHandler(SelectedCategoriesChangedEvent, value);
            remove => RemoveHandler(SelectedCategoriesChangedEvent, value);
        }

        public List<string> SelectedCategories { get; private set; } = new();

        public AssetCategoryList()
        {
            InitializeComponent();
        }

        private void CategoryListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedCategories.Clear();

            foreach (ListBoxItem item in CategoryListBox.SelectedItems)
            {
                if (item.Tag is string tag)
                {
                    SelectedCategories.Add(tag);
                }
            }

            RaiseEvent(new RoutedEventArgs(SelectedCategoriesChangedEvent));
        }
    }
}
