using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace HighlightMe.Services
{
    public class PrivacyBlurService
    {
        private readonly string _blurListPath;
        private HashSet<string> _blurredPaths;

        public PrivacyBlurService()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appFolder = Path.Combine(appDataPath, "HighlightMe");
            Directory.CreateDirectory(appFolder);
            _blurListPath = Path.Combine(appFolder, "privacy_blur_list.json");
            
            _blurredPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            LoadBlurList();
        }

        public bool IsBlurred(string path)
        {
            return _blurredPaths.Contains(path);
        }

        public void ToggleBlur(string path)
        {
            if (_blurredPaths.Contains(path))
            {
                _blurredPaths.Remove(path);
            }
            else
            {
                _blurredPaths.Add(path);
            }
            SaveBlurList();
        }

        public void SetBlurred(string path, bool blurred)
        {
            if (blurred)
            {
                _blurredPaths.Add(path);
            }
            else
            {
                _blurredPaths.Remove(path);
            }
            SaveBlurList();
        }

        private void LoadBlurList()
        {
            try
            {
                if (File.Exists(_blurListPath))
                {
                    var json = File.ReadAllText(_blurListPath);
                    var paths = JsonSerializer.Deserialize<List<string>>(json);
                    if (paths != null)
                    {
                        _blurredPaths = new HashSet<string>(paths, StringComparer.OrdinalIgnoreCase);
                    }
                }
            }
            catch
            {
                // Ignore load errors, start with empty list
                _blurredPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
        }

        private void SaveBlurList()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(new List<string>(_blurredPaths), options);
                File.WriteAllText(_blurListPath, json);
            }
            catch
            {
                // Ignore save errors
            }
        }
    }
}
