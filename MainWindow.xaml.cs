using System.Windows;
using System.Windows.Controls;
using HighlightMe.Models;
using HighlightMe.ViewModels;

namespace HighlightMe
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void CopyPathMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && 
                menuItem.DataContext is DesktopItem item)
            {
                try
                {
                    Clipboard.SetText(item.FullPath);
                }
                catch
                {
                    // Ignore clipboard errors
                }
            }
        }

        private void AddEditNoteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && 
                menuItem.DataContext is DesktopItem item)
            {
                if (DataContext is MainViewModel viewModel)
                {
                    viewModel.OpenNoteEditorCommand.Execute(item);
                }
            }
        }

        private void ToggleHiddenMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && 
                menuItem.DataContext is DesktopItem item)
            {
                if (DataContext is MainViewModel viewModel)
                {
                    viewModel.ToggleHiddenCommand.Execute(item);
                }
            }
        }

        private void PreviewMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && 
                menuItem.DataContext is DesktopItem item)
            {
                if (DataContext is MainViewModel viewModel)
                {
                    viewModel.PreviewFileCommand.Execute(item);
                }
            }
        }

        private void ToggleLockMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && 
                menuItem.DataContext is DesktopItem item)
            {
                if (DataContext is MainViewModel viewModel)
                {
                    viewModel.ToggleLockCommand.Execute(item);
                }
            }
        }
    }
}
