using System;
using System.Collections.Generic;
using System.IO;

namespace HighlightMe.Services
{
    public class DesktopWatcherService : IDisposable
    {
        private readonly FileSystemWatcher _userDesktopWatcher;
        private readonly FileSystemWatcher? _publicDesktopWatcher;
        private readonly HashSet<string> _newItems = new(StringComparer.OrdinalIgnoreCase);
        private readonly object _lock = new();

        public event EventHandler<NewItemEventArgs>? NewItemDetected;

        public class NewItemEventArgs : EventArgs
        {
            public string Name { get; set; } = string.Empty;
            public string FullPath { get; set; } = string.Empty;
            public bool IsDirectory { get; set; }
        }

        public DesktopWatcherService()
        {
            // Watch user's desktop
            var userDesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            _userDesktopWatcher = CreateWatcher(userDesktopPath);

            // Watch public desktop if it exists
            var publicDesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory);
            if (!string.IsNullOrEmpty(publicDesktopPath) && Directory.Exists(publicDesktopPath))
            {
                _publicDesktopWatcher = CreateWatcher(publicDesktopPath);
            }
        }

        private FileSystemWatcher CreateWatcher(string path)
        {
            var watcher = new FileSystemWatcher(path)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName,
                EnableRaisingEvents = true,
                IncludeSubdirectories = false
            };

            watcher.Created += OnItemCreated;

            return watcher;
        }

        private void OnItemCreated(object sender, FileSystemEventArgs e)
        {
            // Skip hidden files and desktop.ini
            if (e.Name?.StartsWith(".") == true || 
                e.Name?.Equals("desktop.ini", StringComparison.OrdinalIgnoreCase) == true)
                return;

            bool isDirectory = Directory.Exists(e.FullPath);

            lock (_lock)
            {
                _newItems.Add(e.FullPath);
            }

            NewItemDetected?.Invoke(this, new NewItemEventArgs
            {
                Name = e.Name ?? Path.GetFileName(e.FullPath),
                FullPath = e.FullPath,
                IsDirectory = isDirectory
            });
        }

        public bool IsNewItem(string fullPath)
        {
            lock (_lock)
            {
                return _newItems.Contains(fullPath);
            }
        }

        public IReadOnlyCollection<string> GetNewItems()
        {
            lock (_lock)
            {
                return new List<string>(_newItems);
            }
        }

        public void ClearNewItem(string fullPath)
        {
            lock (_lock)
            {
                _newItems.Remove(fullPath);
            }
        }

        public void ClearAllNewItems()
        {
            lock (_lock)
            {
                _newItems.Clear();
            }
        }

        public int NewItemCount
        {
            get
            {
                lock (_lock)
                {
                    return _newItems.Count;
                }
            }
        }

        public void Dispose()
        {
            _userDesktopWatcher.EnableRaisingEvents = false;
            _userDesktopWatcher.Dispose();

            if (_publicDesktopWatcher != null)
            {
                _publicDesktopWatcher.EnableRaisingEvents = false;
                _publicDesktopWatcher.Dispose();
            }
        }
    }
}
