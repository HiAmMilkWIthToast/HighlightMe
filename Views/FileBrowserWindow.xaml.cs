using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HighlightMe.Views
{
    public partial class FileBrowserWindow : Window
    {
        private string _currentPath = "";
        private readonly string _desktopPath;
        private readonly ObservableCollection<FileItem> _fileItems;

        public FileBrowserWindow()
        {
            InitializeComponent();
            _desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            _fileItems = new ObservableCollection<FileItem>();
            FileList.ItemsSource = _fileItems;
            FileList.SelectionChanged += FileList_SelectionChanged;
            
            LoadDrives();
        }

        private void LoadDrives()
        {
            FolderTree.Items.Clear();
            
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (!drive.IsReady) continue;
                
                var item = new TreeViewItem
                {
                    Header = $"💾 {drive.Name} ({drive.VolumeLabel})",
                    Tag = drive.RootDirectory.FullName
                };
                item.Items.Add(new TreeViewItem { Header = "Loading..." });
                item.Expanded += FolderItem_Expanded;
                FolderTree.Items.Add(item);
            }
        }

        private void FolderItem_Expanded(object sender, RoutedEventArgs e)
        {
            if (sender is not TreeViewItem item) return;
            if (item.Items.Count == 1 && item.Items[0] is TreeViewItem placeholder && placeholder.Header?.ToString() == "Loading...")
            {
                item.Items.Clear();
                LoadSubfolders(item);
            }
        }

        private void LoadSubfolders(TreeViewItem parentItem)
        {
            var path = parentItem.Tag?.ToString();
            if (string.IsNullOrEmpty(path)) return;

            try
            {
                var directories = Directory.GetDirectories(path);
                foreach (var dir in directories)
                {
                    try
                    {
                        var dirInfo = new DirectoryInfo(dir);
                        if ((dirInfo.Attributes & FileAttributes.Hidden) != 0 ||
                            (dirInfo.Attributes & FileAttributes.System) != 0)
                            continue;

                        var subItem = new TreeViewItem
                        {
                            Header = $"📁 {dirInfo.Name}",
                            Tag = dir
                        };
                        subItem.Items.Add(new TreeViewItem { Header = "Loading..." });
                        subItem.Expanded += FolderItem_Expanded;
                        parentItem.Items.Add(subItem);
                    }
                    catch { /* Skip inaccessible folders */ }
                }
            }
            catch { /* Skip inaccessible folders */ }
        }

        private void FolderTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is TreeViewItem item && item.Tag is string path)
            {
                LoadFolder(path);
            }
        }

        private void LoadFolder(string path)
        {
            _currentPath = path;
            CurrentPathText.Text = path;
            _fileItems.Clear();

            try
            {
                // Add parent folder entry if not at root
                var parentDir = Directory.GetParent(path);
                if (parentDir != null)
                {
                    _fileItems.Add(new FileItem
                    {
                        Icon = "📂",
                        Name = "..",
                        FullPath = parentDir.FullName,
                        Size = "",
                        Modified = "",
                        IsDirectory = true,
                        IsParent = true
                    });
                }

                // Add directories
                foreach (var dir in Directory.GetDirectories(path))
                {
                    try
                    {
                        var dirInfo = new DirectoryInfo(dir);
                        if ((dirInfo.Attributes & FileAttributes.Hidden) != 0 ||
                            (dirInfo.Attributes & FileAttributes.System) != 0)
                            continue;

                        _fileItems.Add(new FileItem
                        {
                            Icon = "📁",
                            Name = dirInfo.Name,
                            FullPath = dir,
                            Size = "<DIR>",
                            Modified = dirInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm"),
                            IsDirectory = true
                        });
                    }
                    catch { }
                }

                // Add files
                foreach (var file in Directory.GetFiles(path))
                {
                    try
                    {
                        var fileInfo = new FileInfo(file);
                        if ((fileInfo.Attributes & FileAttributes.Hidden) != 0 ||
                            (fileInfo.Attributes & FileAttributes.System) != 0)
                            continue;

                        _fileItems.Add(new FileItem
                        {
                            Icon = GetFileIcon(fileInfo.Extension),
                            Name = fileInfo.Name,
                            FullPath = file,
                            Size = FormatFileSize(fileInfo.Length),
                            Modified = fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm"),
                            IsDirectory = false
                        });
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cannot access folder: {ex.Message}", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private static string GetFileIcon(string extension)
        {
            return extension.ToLower() switch
            {
                ".txt" or ".md" or ".log" => "📄",
                ".doc" or ".docx" or ".pdf" => "📝",
                ".xls" or ".xlsx" or ".csv" => "📊",
                ".ppt" or ".pptx" => "📽️",
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp" => "🖼️",
                ".mp3" or ".wav" or ".flac" or ".m4a" => "🎵",
                ".mp4" or ".avi" or ".mkv" or ".mov" or ".wmv" => "🎬",
                ".zip" or ".rar" or ".7z" or ".tar" or ".gz" => "📦",
                ".exe" or ".msi" => "⚙️",
                ".dll" or ".sys" => "🔧",
                ".html" or ".htm" or ".css" or ".js" => "🌐",
                ".cs" or ".py" or ".java" or ".cpp" or ".c" => "💻",
                _ => "📄"
            };
        }

        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.#} {sizes[order]}";
        }

        private void FileList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItems = FileList.SelectedItems.Cast<FileItem>().Where(x => !x.IsParent).ToList();
            var count = selectedItems.Count;
            
            CopyButton.IsEnabled = count > 0;
            MoveButton.IsEnabled = count > 0;
            
            SelectionInfo.Text = count == 0 
                ? "No items selected" 
                : count == 1 
                    ? $"1 item selected: {selectedItems[0].Name}"
                    : $"{count} items selected";
        }

        private void QuickAccess_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string folderName)
            {
                var path = folderName switch
                {
                    "Documents" => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "Downloads" => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"),
                    "Pictures" => Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                    "Music" => Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
                    "Videos" => Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
                    _ => null
                };

                if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
                {
                    LoadFolder(path);
                }
            }
        }

        private void CopyToDesktop_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = FileList.SelectedItems.Cast<FileItem>().Where(x => !x.IsParent).ToList();
            if (selectedItems.Count == 0) return;

            int successCount = 0;
            var errors = new List<string>();

            foreach (var item in selectedItems)
            {
                try
                {
                    var destPath = Path.Combine(_desktopPath, item.Name);
                    
                    // Handle name conflicts
                    destPath = GetUniqueDestinationPath(destPath, item.IsDirectory);

                    if (item.IsDirectory)
                    {
                        CopyDirectory(item.FullPath, destPath);
                    }
                    else
                    {
                        File.Copy(item.FullPath, destPath);
                    }
                    successCount++;
                }
                catch (Exception ex)
                {
                    errors.Add($"{item.Name}: {ex.Message}");
                }
            }

            ShowResult(successCount, errors, "copied");
        }

        private void MoveToDesktop_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = FileList.SelectedItems.Cast<FileItem>().Where(x => !x.IsParent).ToList();
            if (selectedItems.Count == 0) return;

            var result = MessageBox.Show(
                $"Move {selectedItems.Count} item(s) to desktop?\n\nThis will remove them from the original location.",
                "Confirm Move",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            int successCount = 0;
            var errors = new List<string>();

            foreach (var item in selectedItems)
            {
                try
                {
                    var destPath = Path.Combine(_desktopPath, item.Name);
                    
                    // Handle name conflicts
                    destPath = GetUniqueDestinationPath(destPath, item.IsDirectory);

                    if (item.IsDirectory)
                    {
                        Directory.Move(item.FullPath, destPath);
                    }
                    else
                    {
                        File.Move(item.FullPath, destPath);
                    }
                    successCount++;
                }
                catch (Exception ex)
                {
                    errors.Add($"{item.Name}: {ex.Message}");
                }
            }

            ShowResult(successCount, errors, "moved");
            
            // Refresh current folder
            if (!string.IsNullOrEmpty(_currentPath))
            {
                LoadFolder(_currentPath);
            }
        }

        private static string GetUniqueDestinationPath(string path, bool isDirectory)
        {
            if ((isDirectory && !Directory.Exists(path)) || (!isDirectory && !File.Exists(path)))
                return path;

            var dir = Path.GetDirectoryName(path) ?? "";
            var name = Path.GetFileNameWithoutExtension(path);
            var ext = Path.GetExtension(path);

            int counter = 1;
            string newPath;
            do
            {
                newPath = Path.Combine(dir, $"{name} ({counter}){ext}");
                counter++;
            } while ((isDirectory && Directory.Exists(newPath)) || (!isDirectory && File.Exists(newPath)));

            return newPath;
        }

        private static void CopyDirectory(string sourceDir, string destDir)
        {
            Directory.CreateDirectory(destDir);

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var destFile = Path.Combine(destDir, Path.GetFileName(file));
                File.Copy(file, destFile);
            }

            foreach (var dir in Directory.GetDirectories(sourceDir))
            {
                var destSubDir = Path.Combine(destDir, Path.GetFileName(dir));
                CopyDirectory(dir, destSubDir);
            }
        }

        private void ShowResult(int successCount, List<string> errors, string action)
        {
            if (errors.Count == 0)
            {
                MessageBox.Show(
                    $"Successfully {action} {successCount} item(s) to desktop!",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else
            {
                var errorMsg = string.Join("\n", errors.Take(5));
                if (errors.Count > 5) errorMsg += $"\n... and {errors.Count - 5} more errors";
                
                MessageBox.Show(
                    $"Completed with {successCount} success(es) and {errors.Count} error(s):\n\n{errorMsg}",
                    "Partial Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public class FileItem
    {
        public string Icon { get; set; } = "";
        public string Name { get; set; } = "";
        public string FullPath { get; set; } = "";
        public string Size { get; set; } = "";
        public string Modified { get; set; } = "";
        public bool IsDirectory { get; set; }
        public bool IsParent { get; set; }
    }
}
