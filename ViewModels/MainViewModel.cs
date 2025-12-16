using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using HighlightMe.Models;
using HighlightMe.Services;

namespace HighlightMe.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly DesktopScannerService _scannerService;
        private readonly DesktopHighlightService _highlightService;
        private readonly DesktopWatcherService _watcherService;
        private readonly SearchHistoryService _searchHistoryService;
        private readonly FileNotesService _notesService;
        private readonly FileLockService _fileLockService;
        private readonly AppSettingsService _appSettingsService;
        private readonly DispatcherTimer _debounceTimer;
        private readonly DispatcherTimer _saveSearchTimer;
        private string _searchQuery = string.Empty;
        private ObservableCollection<DesktopItem> _desktopItems = new();
        private int _totalItems;
        private int _matchingItems;
        private int _desktopHighlightCount;
        private int _newItemCount;
        private bool _isScanning;
        private bool _showDesktopHighlights = true;
        private bool _showNewItemHighlights = true;
        private bool _showArrows = true;
        private bool _isListView = false;
        private bool _searchInContents = false;
        private string _sortBy = "Name";
        private string _newItemNotification = string.Empty;
        private int _cardWidth = 200;
        private int _cardMargin = 8;
        private bool _showFileDetails = true;

        public MainViewModel()
        {
            _scannerService = new DesktopScannerService();
            _highlightService = new DesktopHighlightService();
            _watcherService = new DesktopWatcherService();
            _searchHistoryService = new SearchHistoryService();
            _notesService = new FileNotesService();
            _fileLockService = new FileLockService();
            _appSettingsService = App.SettingsService ?? new AppSettingsService();
            
            // Subscribe to settings changes for layout updates
            _appSettingsService.SettingsChanged += OnSettingsChanged;
            
            // Show admin warning if not elevated
            if (!FileLockService.IsRunningAsAdmin())
            {
                System.Windows.MessageBox.Show(
                    "⚠️ Running without Administrator privileges.\n\n" +
                    "File locking will work, but for best protection:\n" +
                    "• Right-click the app and 'Run as Administrator'\n" +
                    "• This enables permanent NTFS-level protection",
                    "HighlightMe - Admin Notice",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
            }
            
            // Subscribe to new item events
            _watcherService.NewItemDetected += OnNewItemDetected;
            
            // Debounce timer for desktop highlights (300ms delay)
            _debounceTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(300)
            };
            _debounceTimer.Tick += (s, e) =>
            {
                _debounceTimer.Stop();
                ApplyDesktopHighlights();
            };
            
            // Timer to save search to history after user stops typing (1 second)
            _saveSearchTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(1000)
            };
            _saveSearchTimer.Tick += (s, e) =>
            {
                _saveSearchTimer.Stop();
                SaveCurrentSearch();
            };
            
            RefreshCommand = new RelayCommand(_ => Refresh());
            OpenItemCommand = new RelayCommand(OpenItem);
            OpenLocationCommand = new RelayCommand(OpenLocation);
            OpenColorSettingsCommand = new RelayCommand(_ => OpenColorSettings());
            ToggleDesktopHighlightsCommand = new RelayCommand(_ => ToggleDesktopHighlights());
            ToggleNewItemHighlightsCommand = new RelayCommand(_ => ToggleNewItemHighlights());
            ClearNewItemsCommand = new RelayCommand(_ => ClearNewItems());
            SelectHistoryItemCommand = new RelayCommand(SelectHistoryItem);
            ClearHistoryCommand = new RelayCommand(_ => ClearHistory());
            RemoveHistoryItemCommand = new RelayCommand(RemoveHistoryItem);
            OpenNoteEditorCommand = new RelayCommand(OpenNoteEditor);
            ToggleHiddenCommand = new RelayCommand(ToggleHidden);
            PreviewFileCommand = new RelayCommand(PreviewFile);
            ToggleLockCommand = new RelayCommand(ToggleLock);
            OpenAppSettingsCommand = new RelayCommand(_ => OpenAppSettings());
            OpenHelpCommand = new RelayCommand(_ => OpenHelp());
            
            // Apply initial layout settings
            ApplyLayoutSettings();
            
            // Initial scan
            Refresh();
        }

        private void OnNewItemDetected(object? sender, DesktopWatcherService.NewItemEventArgs e)
        {
            // This is called from a background thread, dispatch to UI thread
            Application.Current?.Dispatcher.Invoke(() =>
            {
                NewItemCount = _watcherService.NewItemCount;
                string type = e.IsDirectory ? "folder" : "file";
                NewItemNotification = $"New {type}: {e.Name}";
                
                // Refresh to include the new item
                Refresh();
                
                // Highlight new items on desktop
                if (ShowNewItemHighlights)
                {
                    ApplyNewItemHighlights();
                }
            });
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (_searchQuery != value)
                {
                    _searchQuery = value;
                    OnPropertyChanged();
                    ApplySearchFilter();
                    
                    // Restart save timer
                    _saveSearchTimer.Stop();
                    _saveSearchTimer.Start();
                }
            }
        }

        public ObservableCollection<DesktopItem> DesktopItems
        {
            get => _desktopItems;
            set { _desktopItems = value; OnPropertyChanged(); }
        }

        public int TotalItems
        {
            get => _totalItems;
            set { _totalItems = value; OnPropertyChanged(); }
        }

        public int MatchingItems
        {
            get => _matchingItems;
            set { _matchingItems = value; OnPropertyChanged(); }
        }

        public int DesktopHighlightCount
        {
            get => _desktopHighlightCount;
            set { _desktopHighlightCount = value; OnPropertyChanged(); }
        }

        public int NewItemCount
        {
            get => _newItemCount;
            set { _newItemCount = value; OnPropertyChanged(); }
        }

        public string NewItemNotification
        {
            get => _newItemNotification;
            set { _newItemNotification = value; OnPropertyChanged(); }
        }

        public bool IsScanning
        {
            get => _isScanning;
            set { _isScanning = value; OnPropertyChanged(); }
        }

        public bool ShowDesktopHighlights
        {
            get => _showDesktopHighlights;
            set 
            { 
                _showDesktopHighlights = value; 
                OnPropertyChanged();
                ApplyDesktopHighlights();
            }
        }

        public bool ShowNewItemHighlights
        {
            get => _showNewItemHighlights;
            set 
            { 
                _showNewItemHighlights = value; 
                OnPropertyChanged();
                if (value)
                    ApplyNewItemHighlights();
                else
                    _highlightService.ClearNewItemHighlights();
            }
        }

        public bool ShowArrows
        {
            get => _showArrows;
            set 
            { 
                _showArrows = value; 
                OnPropertyChanged();
                _highlightService.ShowArrows = value;
                if (ShowDesktopHighlights)
                    ApplyDesktopHighlights();
            }
        }

        public bool IsListView
        {
            get => _isListView;
            set { _isListView = value; OnPropertyChanged(); }
        }
        
        public bool SearchInContents
        {
            get => _searchInContents;
            set 
            { 
                _searchInContents = value; 
                OnPropertyChanged();
                ApplySearchFilter();
            }
        }

        public string SortBy
        {
            get => _sortBy;
            set 
            { 
                _sortBy = value; 
                OnPropertyChanged();
                SortItems();
            }
        }

        public ObservableCollection<string> SortOptions { get; } = new ObservableCollection<string>
        {
            "Name",
            "Size",
            "Date",
            "Type"
        };
        
        public ICommand RefreshCommand { get; }
        public ICommand OpenItemCommand { get; }
        public ICommand OpenLocationCommand { get; }
        public ICommand OpenCategoryManagerCommand { get; }
        public ICommand OpenColorSettingsCommand { get; }
        public ICommand ToggleDesktopHighlightsCommand { get; }
        public ICommand ToggleNewItemHighlightsCommand { get; }
        public ICommand ClearNewItemsCommand { get; }
        public ICommand SelectHistoryItemCommand { get; }
        public ICommand ClearHistoryCommand { get; }
        public ICommand RemoveHistoryItemCommand { get; }
        public ICommand OpenNoteEditorCommand { get; }
        public ICommand ToggleHiddenCommand { get; }
        public ICommand PreviewFileCommand { get; }
        public ICommand ToggleLockCommand { get; }
        public ICommand OpenAppSettingsCommand { get; }
        public ICommand OpenHelpCommand { get; }
        
        public int CardWidth
        {
            get => _cardWidth;
            set { _cardWidth = value; OnPropertyChanged(); }
        }
        
        public int CardMargin
        {
            get => _cardMargin;
            set { _cardMargin = value; OnPropertyChanged(); }
        }
        
        public bool ShowFileDetails
        {
            get => _showFileDetails;
            set { _showFileDetails = value; OnPropertyChanged(); }
        }
        
        public ObservableCollection<string> SearchHistory => _searchHistoryService.History;

        private void ToggleDesktopHighlights()
        {
            ShowDesktopHighlights = !ShowDesktopHighlights;
        }

        private void ToggleHidden(object? parameter)
        {
            if (parameter is DesktopItem item)
            {
                DesktopScannerService.ToggleHidden(item.FullPath);
                item.IsHidden = !item.IsHidden;
            }
        }

        private void PreviewFile(object? parameter)
        {
            if (parameter is DesktopItem item && !item.IsDirectory)
            {
                var window = new Views.FilePreviewWindow(item.FullPath, item.Name);
                window.Owner = Application.Current.MainWindow;
                window.ShowDialog();
            }
        }

        private void ToggleLock(object? parameter)
        {
            if (parameter is DesktopItem item)
            {
                bool success = _fileLockService.ToggleLock(item.FullPath, item.IsLocked);
                if (success)
                {
                    item.IsLocked = !item.IsLocked;
                }
            }
        }

        private void ToggleNewItemHighlights()
        {
            ShowNewItemHighlights = !ShowNewItemHighlights;
        }

        private void OpenColorSettings()
        {
            var window = new Views.ColorSettingsWindow(_highlightService.CategorySettings);
            window.Owner = Application.Current.MainWindow;
            window.ShowDialog();
        }

        private void OpenAppSettings()
        {
            var themeService = App.ThemeService ?? new ThemeService();
            var window = new Views.AppSettingsWindow(_appSettingsService, themeService);
            window.Owner = Application.Current.MainWindow;
            window.ShowDialog();
        }

        private void OpenHelp()
        {
            var window = new Views.HelpGuideWindow();
            window.Owner = Application.Current.MainWindow;
            window.ShowDialog();
        }

        private void OnSettingsChanged()
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                ApplyLayoutSettings();
            });
        }

        private void ApplyLayoutSettings()
        {
            CardWidth = _appSettingsService.Settings.Layout.GetCardWidth();
            CardMargin = _appSettingsService.Settings.Layout.CardSpacing;
            ShowFileDetails = _appSettingsService.Settings.Layout.ShowFileDetails;
        }

        private void OpenNoteEditor(object? parameter)
        {
            if (parameter is DesktopItem item)
            {
                var window = new Views.NoteEditorWindow(_notesService, item.FullPath, item.Name);
                window.Owner = Application.Current.MainWindow;
                if (window.ShowDialog() == true)
                {
                    // Update the item's note properties
                    var note = _notesService.GetNote(item.FullPath);
                    if (note != null)
                    {
                        item.HasNote = true;
                        item.NoteText = note.Note;
                        item.NoteColor = note.NoteColor;
                    }
                    else
                    {
                        item.HasNote = false;
                        item.NoteText = string.Empty;
                    }
                }
            }
        }

        private void ClearNewItems()
        {
            _watcherService.ClearAllNewItems();
            _highlightService.ClearNewItemHighlights();
            NewItemCount = 0;
            NewItemNotification = string.Empty;
            
            // Update items to remove "new" flag
            foreach (var item in DesktopItems)
            {
                item.IsNew = false;
            }
        }

        private void Refresh()
        {
            IsScanning = true;
            
            try
            {
                DesktopItems.Clear();
                var items = _scannerService.ScanDesktop(true);
                
                foreach (var item in items)
                {
                    // Mark new items
                    item.IsNew = _watcherService.IsNewItem(item.FullPath);
                    
                    // Populate note data
                    var note = _notesService.GetNote(item.FullPath);
                    if (note != null)
                    {
                        item.HasNote = true;
                        item.NoteText = note.Note;
                        item.NoteColor = note.NoteColor;
                    }
                    
                    DesktopItems.Add(item);
                }
                
                TotalItems = DesktopItems.Count;
                NewItemCount = _watcherService.NewItemCount;
                
                // Refresh desktop icon positions
                _highlightService.RefreshIconPositions();
                
                ApplySearchFilter();
                
                // Apply new item highlights if enabled
                if (ShowNewItemHighlights && NewItemCount > 0)
                {
                    ApplyNewItemHighlights();
                }
                
                SortItems();
            }
            finally
            {
                IsScanning = false;
            }
        }

        private void ApplySearchFilter()
        {
            var query = SearchQuery.Trim();
            bool hasQuery = !string.IsNullOrEmpty(query);
            int matchCount = 0;

            foreach (var item in DesktopItems)
            {
                if (hasQuery)
                {
                    bool matches = item.Name.Contains(query, StringComparison.OrdinalIgnoreCase);
                    
                    // Search in contents if enabled and not already matched by name
                    if (!matches && SearchInContents && !item.IsDirectory)
                    {
                        matches = SearchItemContent(item, query);
                    }

                    item.IsHighlighted = matches;
                    item.Opacity = matches ? 1.0 : 0.35;
                    
                    if (matches) matchCount++;
                }
                else
                {
                    item.IsHighlighted = false;
                    item.Opacity = 1.0;
                }
            }

            MatchingItems = hasQuery ? matchCount : TotalItems;

            // Debounce desktop highlights
            _debounceTimer.Stop();
            _debounceTimer.Start();
        }

        private bool SearchItemContent(DesktopItem item, string query)
        {
            try
            {
                // Only search text-based files and limit size to 1MB
                string ext = System.IO.Path.GetExtension(item.FullPath).ToLower();
                var textExtensions = new[] { ".txt", ".md", ".cs", ".xml", ".json", ".log", ".ini", ".csv", ".html", ".css", ".js", ".xaml" };
                
                if (textExtensions.Contains(ext))
                {
                    var info = new System.IO.FileInfo(item.FullPath);
                    if (info.Length < 1024 * 1024) // 1MB limit
                    {
                        string content = System.IO.File.ReadAllText(item.FullPath);
                        return content.Contains(query, StringComparison.OrdinalIgnoreCase);
                    }
                }
            }
            catch
            {
                // Ignore read errors
            }
            return false;
        }

        private void SortItems()
        {
            if (DesktopItems.Count == 0) return;

            var sortedList = _sortBy switch
            {
                "Name" => DesktopItems.OrderBy(i => i.IsDirectory ? 0 : 1).ThenBy(i => i.Name).ToList(),
                "Size" => DesktopItems.OrderByDescending(i => i.Size).ToList(),
                "Date" => DesktopItems.OrderByDescending(i => i.DateModified).ToList(),
                "Type" => DesktopItems.OrderBy(i => i.IsDirectory ? "" : System.IO.Path.GetExtension(i.FullPath)).ThenBy(i => i.Name).ToList(),
                _ => DesktopItems.OrderBy(i => i.Name).ToList()
            };

            DesktopItems.Clear();
            foreach (var item in sortedList)
            {
                DesktopItems.Add(item);
            }
        }

        private void ApplyDesktopHighlights()
        {
            var query = SearchQuery.Trim();
            bool hasQuery = !string.IsNullOrEmpty(query);

            if (ShowDesktopHighlights && hasQuery)
            {
                _highlightService.HighlightMatchingIcons(query);
                DesktopHighlightCount = _highlightService.GetMatchingIconCount(query);
            }
            else
            {
                _highlightService.ClearSearchHighlights();
                DesktopHighlightCount = 0;
            }
        }

        private void ApplyNewItemHighlights()
        {
            var newItems = _watcherService.GetNewItems();
            if (newItems.Any())
            {
                _highlightService.HighlightNewItems(newItems);
            }
        }

        private void OpenItem(object? parameter)
        {
            if (parameter is DesktopItem item)
            {
                try
                {
                    // Clear "new" status when opened
                    if (item.IsNew)
                    {
                        _watcherService.ClearNewItem(item.FullPath);
                        item.IsNew = false;
                        NewItemCount = _watcherService.NewItemCount;
                        ApplyNewItemHighlights();
                    }

                    var startInfo = new ProcessStartInfo
                    {
                        FileName = item.FullPath,
                        UseShellExecute = true
                    };
                    Process.Start(startInfo);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Could not open {item.Name}: {ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private void OpenLocation(object? parameter)
        {
            if (parameter is DesktopItem item)
            {
                try
                {
                    // Open Explorer and select the file/folder
                    Process.Start("explorer.exe", $"/select,\"{item.FullPath}\"");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Could not open location for {item.Name}: {ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private void SaveCurrentSearch()
        {
            if (!string.IsNullOrWhiteSpace(SearchQuery) && SearchQuery.Length >= 2)
            {
                _searchHistoryService.AddSearch(SearchQuery);
            }
        }

        private void SelectHistoryItem(object? parameter)
        {
            if (parameter is string historyItem)
            {
                SearchQuery = historyItem;
            }
        }

        private void ClearHistory()
        {
            _searchHistoryService.ClearHistory();
        }

        private void RemoveHistoryItem(object? parameter)
        {
            if (parameter is string historyItem)
            {
                _searchHistoryService.RemoveSearch(historyItem);
            }
        }

        public void Dispose()
        {
            _debounceTimer.Stop();
            _saveSearchTimer.Stop();
            _highlightService.Dispose();
            _watcherService.Dispose();
            _appSettingsService.SettingsChanged -= OnSettingsChanged;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
