using System;
using System.Linq;
using System.Windows;

namespace HighlightMe.Services
{
    public class ThemeService
    {
        private string _currentTheme = "Dark";
        
        public string CurrentTheme => _currentTheme;

        public void ApplyTheme(string themeName)
        {
            try
            {
                var app = Application.Current;
                if (app == null) return;

                var themeUri = new Uri($"pack://application:,,,/Themes/{themeName}Theme.xaml", UriKind.Absolute);
                var newThemeDictionary = new ResourceDictionary { Source = themeUri };
                
                // Find and remove any existing theme dictionary
                var existingThemes = app.Resources.MergedDictionaries
                    .Where(d => d.Source != null && d.Source.OriginalString.Contains("/Themes/") && d.Source.OriginalString.EndsWith("Theme.xaml"))
                    .ToList();
                
                foreach (var theme in existingThemes)
                {
                    app.Resources.MergedDictionaries.Remove(theme);
                }
                
                // Add new theme dictionary at the beginning
                app.Resources.MergedDictionaries.Insert(0, newThemeDictionary);
                _currentTheme = themeName;
                
                System.Diagnostics.Debug.WriteLine($"Applied theme: {themeName}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to apply theme {themeName}: {ex.Message}");
                // Fallback to Dark theme if loading fails
                if (themeName != "Dark")
                {
                    ApplyTheme("Dark");
                }
            }
        }

        public static string GetThemePreviewColor(string themeName)
        {
            return themeName switch
            {
                "Dark" => "#1A1A2E",
                "Light" => "#F5F7FA",
                "Ocean" => "#0A192F",
                "Forest" => "#1A2F1A",
                "Sunset" => "#1F1135",
                _ => "#1A1A2E"
            };
        }

        public static string GetThemeAccentColor(string themeName)
        {
            return themeName switch
            {
                "Dark" => "#E94560",
                "Light" => "#E94560",
                "Ocean" => "#64FFDA",
                "Forest" => "#4ADE80",
                "Sunset" => "#F97316",
                _ => "#E94560"
            };
        }

        public static string GetThemeDescription(string themeName)
        {
            return themeName switch
            {
                "Dark" => "Deep purple-blue with pink accents",
                "Light" => "Clean white with subtle shadows",
                "Ocean" => "Deep blue with teal highlights",
                "Forest" => "Natural greens with gold accents",
                "Sunset" => "Warm purple with orange glow",
                _ => ""
            };
        }
    }
}
