using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace HighlightMe.Views
{
    public partial class FilePreviewWindow : Window
    {
        private readonly string _filePath;
        
        // Supported extensions
        private static readonly string[] TextExtensions = { ".txt", ".log", ".md", ".json", ".xml", ".csv", ".ini", ".cfg", ".config", ".cs", ".js", ".py", ".html", ".css", ".bat", ".ps1", ".sh" };
        private static readonly string[] ImageExtensions = { ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".ico", ".webp", ".tiff", ".tif" };

        public FilePreviewWindow(string filePath, string fileName)
        {
            InitializeComponent();
            _filePath = filePath;
            
            FileNameText.Text = fileName;
            LoadFileInfo();
            LoadPreview();
        }

        private void LoadFileInfo()
        {
            try
            {
                var fileInfo = new FileInfo(_filePath);
                var sizeStr = FormatFileSize(fileInfo.Length);
                var dateStr = fileInfo.LastWriteTime.ToString("MMM dd, yyyy h:mm tt");
                FileInfoText.Text = $"Size: {sizeStr} | Modified: {dateStr}";
            }
            catch
            {
                FileInfoText.Text = "Unable to read file information";
            }
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private void LoadPreview()
        {
            var extension = Path.GetExtension(_filePath).ToLowerInvariant();
            
            if (IsTextFile(extension))
            {
                LoadTextPreview();
            }
            else if (IsImageFile(extension))
            {
                LoadImagePreview();
            }
            else
            {
                ShowUnsupported();
            }
        }

        private bool IsTextFile(string extension)
        {
            return Array.Exists(TextExtensions, ext => ext.Equals(extension, StringComparison.OrdinalIgnoreCase));
        }

        private bool IsImageFile(string extension)
        {
            return Array.Exists(ImageExtensions, ext => ext.Equals(extension, StringComparison.OrdinalIgnoreCase));
        }

        private void LoadTextPreview()
        {
            HeaderText.Text = "📄 Text Preview";
            TextPreviewArea.Visibility = Visibility.Visible;
            
            try
            {
                // Read up to 100KB of text to avoid memory issues with large files
                var fileInfo = new FileInfo(_filePath);
                if (fileInfo.Length > 100 * 1024)
                {
                    using var reader = new StreamReader(_filePath);
                    var buffer = new char[100 * 1024];
                    int charsRead = reader.Read(buffer, 0, buffer.Length);
                    TextContent.Text = new string(buffer, 0, charsRead) + "\n\n... [File truncated - too large to preview completely]";
                }
                else
                {
                    TextContent.Text = File.ReadAllText(_filePath);
                }
            }
            catch (Exception ex)
            {
                TextContent.Text = $"Unable to read file:\n{ex.Message}";
            }
        }

        private void LoadImagePreview()
        {
            HeaderText.Text = "🖼️ Image Preview";
            ImagePreviewArea.Visibility = Visibility.Visible;
            
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(_filePath);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                
                ImageContent.Source = bitmap;
                
                // Add image dimensions to info
                FileInfoText.Text += $" | {bitmap.PixelWidth} × {bitmap.PixelHeight} px";
            }
            catch (Exception ex)
            {
                ImagePreviewArea.Visibility = Visibility.Collapsed;
                UnsupportedArea.Visibility = Visibility.Visible;
            }
        }

        private void ShowUnsupported()
        {
            HeaderText.Text = "📄 File Preview";
            UnsupportedArea.Visibility = Visibility.Visible;
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = _filePath,
                    UseShellExecute = true
                });
            }
            catch
            {
                MessageBox.Show("Unable to open file", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
