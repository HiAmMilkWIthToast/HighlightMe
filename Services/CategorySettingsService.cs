using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Media;
using HighlightMe.Models;

namespace HighlightMe.Services
{
    public class CategorySettingsService
    {
        private readonly string _settingsFilePath;
        private CategorySettings _settings;

        public CategorySettings Settings => _settings;

        public CategorySettingsService()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appFolder = Path.Combine(appDataPath, "HighlightMe");
            Directory.CreateDirectory(appFolder);
            _settingsFilePath = Path.Combine(appFolder, "category_colors.json");

            LoadSettings();
        }

        public void LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    var json = File.ReadAllText(_settingsFilePath);
                    var loaded = JsonSerializer.Deserialize<CategorySettings>(json);
                    if (loaded != null)
                    {
                        _settings = loaded;
                        return;
                    }
                }
            }
            catch
            {
                // Fall through to default
            }

            _settings = CategorySettings.CreateDefault();
        }

        public void SaveSettings()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(_settings, options);
                File.WriteAllText(_settingsFilePath, json);
            }
            catch
            {
                // Ignore save errors
            }
        }

        public string GetColorForFile(string filePath, bool isDirectory)
        {
            if (isDirectory)
            {
                return _settings.FolderColor;
            }

            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            
            foreach (var category in _settings.Categories)
            {
                if (category.IsEnabled && category.Extensions.Contains(extension))
                {
                    return category.GlowColor;
                }
            }

            return _settings.DefaultColor;
        }

        public FileCategory? GetCategoryForFile(string filePath, bool isDirectory)
        {
            if (isDirectory)
            {
                return new FileCategory { Name = "Folders", GlowColor = _settings.FolderColor };
            }

            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            
            return _settings.Categories.FirstOrDefault(c => 
                c.IsEnabled && c.Extensions.Contains(extension));
        }

        public void SetCategoryColor(string categoryName, string color)
        {
            var category = _settings.Categories.FirstOrDefault(c => c.Name == categoryName);
            if (category != null)
            {
                category.GlowColor = color;
                SaveSettings();
            }
        }

        public void SetFolderColor(string color)
        {
            _settings.FolderColor = color;
            SaveSettings();
        }

        public void SetDefaultColor(string color)
        {
            _settings.DefaultColor = color;
            SaveSettings();
        }
    }
}
