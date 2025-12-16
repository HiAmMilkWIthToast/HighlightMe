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

        private void OpenGitHub_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "https://github.com/HiAmMilkWIthToast/HighlightMe",
                    UseShellExecute = true
                });
            }
            catch
            {
                // Ignore errors opening browser
            }
        }

        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.ContextMenu != null)
            {
                button.ContextMenu.PlacementTarget = button;
                button.ContextMenu.IsOpen = true;
            }
        }

        private void CreateNewFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string desktopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
                string fileName = Microsoft.VisualBasic.Interaction.InputBox(
                    "Enter the name for the new text file:",
                    "New Text File",
                    "New File.txt");
                
                if (!string.IsNullOrWhiteSpace(fileName))
                {
                    // Ensure .txt extension
                    if (!fileName.EndsWith(".txt", System.StringComparison.OrdinalIgnoreCase))
                        fileName += ".txt";
                    
                    string filePath = System.IO.Path.Combine(desktopPath, fileName);
                    
                    // Check if file already exists
                    if (System.IO.File.Exists(filePath))
                    {
                        MessageBox.Show($"A file named '{fileName}' already exists on the desktop.", 
                            "File Exists", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    
                    System.IO.File.Create(filePath).Close();
                    
                    // Refresh the view
                    if (DataContext is MainViewModel viewModel)
                    {
                        viewModel.RefreshCommand.Execute(null);
                    }
                    
                    MessageBox.Show($"Created '{fileName}' on your desktop!", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error creating file: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateNewFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string desktopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
                string folderName = Microsoft.VisualBasic.Interaction.InputBox(
                    "Enter the name for the new folder:",
                    "New Folder",
                    "New Folder");
                
                if (!string.IsNullOrWhiteSpace(folderName))
                {
                    string folderPath = System.IO.Path.Combine(desktopPath, folderName);
                    
                    // Check if folder already exists
                    if (System.IO.Directory.Exists(folderPath))
                    {
                        MessageBox.Show($"A folder named '{folderName}' already exists on the desktop.", 
                            "Folder Exists", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    
                    System.IO.Directory.CreateDirectory(folderPath);
                    
                    // Refresh the view
                    if (DataContext is MainViewModel viewModel)
                    {
                        viewModel.RefreshCommand.Execute(null);
                    }
                    
                    MessageBox.Show($"Created folder '{folderName}' on your desktop!", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error creating folder: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
