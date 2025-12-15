using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace HighlightMe.Services
{
    public class SearchHistoryService
    {
        private readonly string _historyFilePath;
        private readonly int _maxHistoryItems;
        private readonly List<string> _history;

        public ObservableCollection<string> History { get; }

        public SearchHistoryService(int maxItems = 10)
        {
            _maxHistoryItems = maxItems;
            _history = new List<string>();
            History = new ObservableCollection<string>();

            // Store history in AppData
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appFolder = Path.Combine(appDataPath, "HighlightMe");
            Directory.CreateDirectory(appFolder);
            _historyFilePath = Path.Combine(appFolder, "search_history.json");

            LoadHistory();
        }

        public void AddSearch(string searchQuery)
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
                return;

            searchQuery = searchQuery.Trim();

            // Remove if already exists (to move it to top)
            _history.Remove(searchQuery);

            // Add to beginning
            _history.Insert(0, searchQuery);

            // Trim to max items
            while (_history.Count > _maxHistoryItems)
            {
                _history.RemoveAt(_history.Count - 1);
            }

            UpdateObservableCollection();
            SaveHistory();
        }

        public void RemoveSearch(string searchQuery)
        {
            _history.Remove(searchQuery);
            UpdateObservableCollection();
            SaveHistory();
        }

        public void ClearHistory()
        {
            _history.Clear();
            UpdateObservableCollection();
            SaveHistory();
        }

        private void UpdateObservableCollection()
        {
            History.Clear();
            foreach (var item in _history)
            {
                History.Add(item);
            }
        }

        private void LoadHistory()
        {
            try
            {
                if (File.Exists(_historyFilePath))
                {
                    var json = File.ReadAllText(_historyFilePath);
                    var items = JsonSerializer.Deserialize<List<string>>(json);
                    if (items != null)
                    {
                        _history.AddRange(items.Take(_maxHistoryItems));
                        UpdateObservableCollection();
                    }
                }
            }
            catch
            {
                // Ignore errors loading history
            }
        }

        private void SaveHistory()
        {
            try
            {
                var json = JsonSerializer.Serialize(_history);
                File.WriteAllText(_historyFilePath, json);
            }
            catch
            {
                // Ignore errors saving history
            }
        }
    }
}
