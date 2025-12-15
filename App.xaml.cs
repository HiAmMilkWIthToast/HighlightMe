using System;
using System.Windows;
using HighlightMe.Services;

namespace HighlightMe
{
    public partial class App : Application
    {
        public static AppSettingsService? SettingsService { get; private set; }
        public static ThemeService? ThemeService { get; private set; }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Initialize services FIRST before any UI is created
            SettingsService = new AppSettingsService();
            ThemeService = new ThemeService();
            
            // Load the theme BEFORE MainWindow is created
            // This is critical - the theme must be in Resources before XAML parsing
            string themeName = SettingsService.Settings.Theme.CurrentTheme;
            LoadThemeDirectly(themeName);
        }

        private void LoadThemeDirectly(string themeName)
        {
            try
            {
                var themeUri = new Uri($"pack://application:,,,/Themes/{themeName}Theme.xaml", UriKind.Absolute);
                var themeDictionary = new ResourceDictionary { Source = themeUri };
                
                // Add theme as the first merged dictionary
                Resources.MergedDictionaries.Insert(0, themeDictionary);
                
                System.Diagnostics.Debug.WriteLine($"Loaded initial theme: {themeName}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load theme {themeName}: {ex.Message}");
                // Load dark theme as fallback
                if (themeName != "Dark")
                {
                    LoadThemeDirectly("Dark");
                }
            }
        }
    }
}
