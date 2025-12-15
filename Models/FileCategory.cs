using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows.Media;

namespace HighlightMe.Models
{
    public class FileCategory
    {
        public string Name { get; set; } = string.Empty;
        public string GlowColor { get; set; } = "#FFD700"; // Default gold
        public List<string> Extensions { get; set; } = new();
        public bool IsEnabled { get; set; } = true;

        // Helper to get WPF Color
        public Color GetColor()
        {
            try
            {
                return (Color)ColorConverter.ConvertFromString(GlowColor);
            }
            catch
            {
                return Colors.Gold;
            }
        }
    }

    public class CategorySettings
    {
        public List<FileCategory> Categories { get; set; } = new();
        public string DefaultColor { get; set; } = "#FFD700";
        public string FolderColor { get; set; } = "#4CAF50"; // Green for folders

        public static CategorySettings CreateDefault()
        {
            return new CategorySettings
            {
                DefaultColor = "#FFD700", // Gold
                FolderColor = "#4CAF50",  // Green
                Categories = new List<FileCategory>
                {
                    new FileCategory
                    {
                        Name = "Images",
                        GlowColor = "#E91E63", // Pink
                        Extensions = new List<string> { ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".ico", ".webp", ".svg" }
                    },
                    new FileCategory
                    {
                        Name = "Documents",
                        GlowColor = "#2196F3", // Blue
                        Extensions = new List<string> { ".doc", ".docx", ".pdf", ".txt", ".rtf", ".odt", ".xls", ".xlsx", ".ppt", ".pptx" }
                    },
                    new FileCategory
                    {
                        Name = "Videos",
                        GlowColor = "#9C27B0", // Purple
                        Extensions = new List<string> { ".mp4", ".avi", ".mkv", ".mov", ".wmv", ".flv", ".webm" }
                    },
                    new FileCategory
                    {
                        Name = "Audio",
                        GlowColor = "#FF9800", // Orange
                        Extensions = new List<string> { ".mp3", ".wav", ".flac", ".aac", ".ogg", ".wma", ".m4a" }
                    },
                    new FileCategory
                    {
                        Name = "Archives",
                        GlowColor = "#795548", // Brown
                        Extensions = new List<string> { ".zip", ".rar", ".7z", ".tar", ".gz", ".bz2" }
                    },
                    new FileCategory
                    {
                        Name = "Code",
                        GlowColor = "#00BCD4", // Cyan
                        Extensions = new List<string> { ".cs", ".js", ".py", ".html", ".css", ".java", ".cpp", ".h", ".json", ".xml" }
                    },
                    new FileCategory
                    {
                        Name = "Executables",
                        GlowColor = "#F44336", // Red
                        Extensions = new List<string> { ".exe", ".msi", ".bat", ".cmd", ".ps1", ".sh" }
                    }
                }
            };
        }
    }
}
