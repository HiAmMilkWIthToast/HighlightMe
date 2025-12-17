using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace HighlightMe.Models
{
    public class DesktopItem : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private string _fullPath = string.Empty;
        private bool _isDirectory;
        private ImageSource? _icon;
        private long _size;
        private DateTime _dateModified;
        private bool _isHighlighted;
        private bool _isVisible = true;
        private double _opacity = 1.0;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public string FullPath
        {
            get => _fullPath;
            set { _fullPath = value; OnPropertyChanged(); }
        }

        public bool IsDirectory
        {
            get => _isDirectory;
            set { _isDirectory = value; OnPropertyChanged(); }
        }

        public ImageSource? Icon
        {
            get => _icon;
            set { _icon = value; OnPropertyChanged(); }
        }

        public long Size
        {
            get => _size;
            set { _size = value; OnPropertyChanged(); }
        }

        public DateTime DateModified
        {
            get => _dateModified;
            set { _dateModified = value; OnPropertyChanged(); }
        }

        public bool IsHighlighted
        {
            get => _isHighlighted;
            set { _isHighlighted = value; OnPropertyChanged(); }
        }

        public bool IsVisible
        {
            get => _isVisible;
            set { _isVisible = value; OnPropertyChanged(); }
        }

        public double Opacity
        {
            get => _opacity;
            set { _opacity = value; OnPropertyChanged(); }
        }

        private bool _isNew;
        public bool IsNew
        {
            get => _isNew;
            set { _isNew = value; OnPropertyChanged(); }
        }

        private bool _hasNote;
        public bool HasNote
        {
            get => _hasNote;
            set { _hasNote = value; OnPropertyChanged(); }
        }

        private string _noteText = string.Empty;
        public string NoteText
        {
            get => _noteText;
            set { _noteText = value; OnPropertyChanged(); }
        }

        private string _noteColor = "#FFEB3B";
        public string NoteColor
        {
            get => _noteColor;
            set { _noteColor = value; OnPropertyChanged(); }
        }

        private bool _isHidden;
        public bool IsHidden
        {
            get => _isHidden;
            set { _isHidden = value; OnPropertyChanged(); }
        }

        private bool _isLocked;
        public bool IsLocked
        {
            get => _isLocked;
            set { _isLocked = value; OnPropertyChanged(); }
        }

        private bool _isPrivacyBlurred;
        public bool IsPrivacyBlurred
        {
            get => _isPrivacyBlurred;
            set { _isPrivacyBlurred = value; OnPropertyChanged(); }
        }

        public string FormattedSize
        {
            get
            {
                if (IsDirectory) return "Folder";
                
                string[] sizes = { "B", "KB", "MB", "GB", "TB" };
                double len = Size;
                int order = 0;
                while (len >= 1024 && order < sizes.Length - 1)
                {
                    order++;
                    len = len / 1024;
                }
                return $"{len:0.##} {sizes[order]}";
            }
        }

        public string FormattedDate => DateModified.ToString("MMM dd, yyyy HH:mm");

        public string TypeDescription => IsDirectory ? "File Folder" : GetFileTypeDescription();

        // Properties for identifying openable file types
        public string FileType => IsDirectory ? "Folder" : System.IO.Path.GetExtension(Name)?.ToLowerInvariant() ?? "";

        private static readonly string[] TextExtensions = { ".txt", ".log", ".md", ".json", ".xml", ".csv", ".ini", ".cfg", ".config", ".html", ".css", ".js", ".cs", ".py", ".java" };
        private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".ico", ".webp", ".tiff", ".svg" };

        public bool IsTextFile => !IsDirectory && TextExtensions.Contains(FileType);
        public bool IsImageFile => !IsDirectory && ImageExtensions.Contains(FileType);
        public bool IsOpenable => IsDirectory || IsTextFile || IsImageFile;

        public string OpenActionHint
        {
            get
            {
                if (IsDirectory) return "📂 Double-click to explore!";
                if (IsTextFile) return "📝 Double-click to read!";
                if (IsImageFile) return "🖼️ Double-click to view!";
                return "📄 Double-click to open!";
            }
        }

        public string OpenActionEmoji
        {
            get
            {
                if (IsDirectory) return "📂";
                if (IsTextFile) return "📝";
                if (IsImageFile) return "🖼️";
                return "📄";
            }
        }

        private string GetFileTypeDescription()
        {
            var extension = System.IO.Path.GetExtension(Name);
            if (string.IsNullOrEmpty(extension)) return "File";
            return extension.ToUpperInvariant().TrimStart('.') + " File";
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
