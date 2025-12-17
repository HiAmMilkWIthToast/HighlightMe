using System;
using System.IO;
using System.Text.Json;
using HighlightMe.Models;

namespace HighlightMe.Services
{
    public class AppSettingsService
    {
        private readonly string _settingsFilePath;
        private AppSettings _settings;
        
        public AppSettings Settings => _settings;
        
        public event Action? SettingsChanged;

        public AppSettingsService()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appFolder = Path.Combine(appDataPath, "HighlightMe");
            Directory.CreateDirectory(appFolder);
            _settingsFilePath = Path.Combine(appFolder, "app_settings.json");
            
            _settings = new AppSettings();
            LoadSettings();
        }

        public void LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    var json = File.ReadAllText(_settingsFilePath);
                    var loaded = JsonSerializer.Deserialize<AppSettings>(json);
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
            
            _settings = new AppSettings();
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

        public void SetTheme(string themeName)
        {
            _settings.Theme.CurrentTheme = themeName;
            SaveSettings();
            SettingsChanged?.Invoke();
        }

        public void SetCardSize(CardSizeOption size)
        {
            _settings.Layout.CardSize = size;
            SaveSettings();
            SettingsChanged?.Invoke();
        }

        public void SetCardSpacing(int spacing)
        {
            _settings.Layout.CardSpacing = spacing;
            SaveSettings();
            SettingsChanged?.Invoke();
        }

        public void SetShowFileDetails(bool show)
        {
            _settings.Layout.ShowFileDetails = show;
            SaveSettings();
            SettingsChanged?.Invoke();
        }

        public void SetIconPack(string packName)
        {
            _settings.IconPack.CurrentPack = packName;
            SaveSettings();
            SettingsChanged?.Invoke();
        }

        public void SetPrivacyMode(bool enabled)
        {
            _settings.Privacy.PrivacyModeEnabled = enabled;
            SaveSettings();
            SettingsChanged?.Invoke();
        }
    }
}
