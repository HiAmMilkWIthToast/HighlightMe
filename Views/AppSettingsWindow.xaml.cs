using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using HighlightMe.Models;
using HighlightMe.Services;

namespace HighlightMe.Views
{
    public partial class AppSettingsWindow : Window
    {
        private readonly AppSettingsService _settingsService;
        private readonly ThemeService _themeService;
        private string _selectedTheme;
        private CardSizeOption _selectedCardSize;
        private string _selectedIconPack;

        public AppSettingsWindow(AppSettingsService settingsService, ThemeService themeService)
        {
            InitializeComponent();
            _settingsService = settingsService;
            _themeService = themeService;
            
            _selectedTheme = _settingsService.Settings.Theme.CurrentTheme;
            _selectedCardSize = _settingsService.Settings.Layout.CardSize;
            _selectedIconPack = _settingsService.Settings.IconPack.CurrentPack;
            
            BuildThemesUI();
            BuildLayoutUI();
            BuildIconPacksUI();
        }

        private void BuildThemesUI()
        {
            ThemesList.Children.Clear();
            
            foreach (var theme in ThemeSettings.AvailableThemes)
            {
                var isSelected = theme == _selectedTheme;
                
                var card = new Border
                {
                    Width = 180,
                    Height = 120,
                    Margin = new Thickness(0, 0, 15, 15),
                    CornerRadius = new CornerRadius(12),
                    Cursor = System.Windows.Input.Cursors.Hand,
                    BorderThickness = new Thickness(isSelected ? 3 : 1),
                    BorderBrush = isSelected 
                        ? new SolidColorBrush((Color)ColorConverter.ConvertFromString(ThemeService.GetThemeAccentColor(theme)))
                        : (Brush)FindResource("InputBorderBrush"),
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ThemeService.GetThemePreviewColor(theme)))
                };
                
                var stack = new StackPanel
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                
                // Accent color preview circle
                var accentCircle = new Ellipse
                {
                    Width = 30,
                    Height = 30,
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ThemeService.GetThemeAccentColor(theme))),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 10)
                };
                stack.Children.Add(accentCircle);
                
                // Theme name
                var nameText = new TextBlock
                {
                    Text = theme,
                    FontSize = 15,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = theme == "Light" 
                        ? new SolidColorBrush(Colors.Black) 
                        : new SolidColorBrush(Colors.White),
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                stack.Children.Add(nameText);
                
                // Description
                var descText = new TextBlock
                {
                    Text = ThemeService.GetThemeDescription(theme),
                    FontSize = 10,
                    Foreground = theme == "Light" 
                        ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#667085"))
                        : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A0A0A0")),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    TextAlignment = TextAlignment.Center,
                    MaxWidth = 150,
                    Margin = new Thickness(0, 5, 0, 0)
                };
                stack.Children.Add(descText);
                
                // Selected indicator
                if (isSelected)
                {
                    var checkmark = new TextBlock
                    {
                        Text = "✓",
                        FontSize = 12,
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ThemeService.GetThemeAccentColor(theme))),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 8, 0, 0)
                    };
                    stack.Children.Add(checkmark);
                }
                
                card.Child = stack;
                
                var themeName = theme;
                card.MouseLeftButtonDown += (s, e) => SelectTheme(themeName);
                
                ThemesList.Children.Add(card);
            }
        }

        private void SelectTheme(string themeName)
        {
            _selectedTheme = themeName;
            _settingsService.SetTheme(themeName);
            _themeService.ApplyTheme(themeName);
            BuildThemesUI(); // Refresh UI to show selection
        }

        private void BuildLayoutUI()
        {
            CardSizePanel.Children.Clear();
            
            var sizes = new[] { CardSizeOption.Small, CardSizeOption.Medium, CardSizeOption.Large };
            var labels = new[] { "Small", "Medium", "Large" };
            var widths = new[] { "150px", "200px", "280px" };
            
            for (int i = 0; i < sizes.Length; i++)
            {
                var size = sizes[i];
                var isSelected = size == _selectedCardSize;
                
                var card = new Border
                {
                    MinWidth = 120,
                    Padding = new Thickness(20, 15, 20, 15),
                    Margin = new Thickness(0, 0, 10, 0),
                    CornerRadius = new CornerRadius(10),
                    Cursor = System.Windows.Input.Cursors.Hand,
                    Background = isSelected 
                        ? (Brush)FindResource("AccentBrush")
                        : (Brush)FindResource("InputBackgroundBrush"),
                    BorderThickness = new Thickness(2),
                    BorderBrush = isSelected 
                        ? (Brush)FindResource("AccentLightBrush")
                        : (Brush)FindResource("InputBorderBrush")
                };
                
                var stack = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
                
                var nameText = new TextBlock
                {
                    Text = labels[i],
                    FontSize = 14,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = (Brush)FindResource("TextPrimaryBrush"),
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                stack.Children.Add(nameText);
                
                var sizeText = new TextBlock
                {
                    Text = widths[i],
                    FontSize = 11,
                    Foreground = (Brush)FindResource("TextSecondaryBrush"),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 3, 0, 0)
                };
                stack.Children.Add(sizeText);
                
                card.Child = stack;
                
                var sizeOption = size;
                card.MouseLeftButtonDown += (s, e) => SelectCardSize(sizeOption);
                
                CardSizePanel.Children.Add(card);
            }
            
            // Set slider values
            SpacingSlider.Value = _settingsService.Settings.Layout.CardSpacing;
            SpacingValue.Text = $"{_settingsService.Settings.Layout.CardSpacing} px";
            ShowDetailsCheckbox.IsChecked = _settingsService.Settings.Layout.ShowFileDetails;
        }

        private void SelectCardSize(CardSizeOption size)
        {
            _selectedCardSize = size;
            _settingsService.SetCardSize(size);
            BuildLayoutUI();
        }

        private void SpacingSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (SpacingValue != null)
            {
                var value = (int)e.NewValue;
                SpacingValue.Text = $"{value} px";
                _settingsService?.SetCardSpacing(value);
            }
        }

        private void ShowDetailsCheckbox_Changed(object sender, RoutedEventArgs e)
        {
            _settingsService?.SetShowFileDetails(ShowDetailsCheckbox.IsChecked ?? true);
        }

        private void BuildIconPacksUI()
        {
            IconPacksPanel.Children.Clear();
            
            foreach (var pack in IconPackSettings.AvailablePacks)
            {
                var isSelected = pack.Name == _selectedIconPack;
                
                var card = new Border
                {
                    Padding = new Thickness(15, 12, 15, 12),
                    Margin = new Thickness(0, 0, 0, 10),
                    CornerRadius = new CornerRadius(10),
                    Cursor = System.Windows.Input.Cursors.Hand,
                    Background = isSelected 
                        ? (Brush)FindResource("HighlightBackgroundBrush")
                        : (Brush)FindResource("InputBackgroundBrush"),
                    BorderThickness = new Thickness(2),
                    BorderBrush = isSelected 
                        ? (Brush)FindResource("AccentBrush")
                        : new SolidColorBrush(Colors.Transparent)
                };
                
                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                
                // Icon
                var iconText = new TextBlock
                {
                    Text = pack.Icon,
                    FontSize = 24,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(iconText, 0);
                grid.Children.Add(iconText);
                
                // Name and description
                var textStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
                var nameText = new TextBlock
                {
                    Text = pack.Name,
                    FontSize = 14,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = (Brush)FindResource("TextPrimaryBrush")
                };
                textStack.Children.Add(nameText);
                
                var descText = new TextBlock
                {
                    Text = pack.Description,
                    FontSize = 12,
                    Foreground = (Brush)FindResource("TextSecondaryBrush"),
                    Margin = new Thickness(0, 2, 0, 0)
                };
                textStack.Children.Add(descText);
                
                Grid.SetColumn(textStack, 1);
                grid.Children.Add(textStack);
                
                // Selected indicator
                if (isSelected)
                {
                    var checkmark = new TextBlock
                    {
                        Text = "✓",
                        FontSize = 18,
                        Foreground = (Brush)FindResource("AccentBrush"),
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    Grid.SetColumn(checkmark, 2);
                    grid.Children.Add(checkmark);
                }
                
                card.Child = grid;
                
                var packName = pack.Name;
                card.MouseLeftButtonDown += (s, e) => SelectIconPack(packName);
                
                IconPacksPanel.Children.Add(card);
            }
        }

        private void SelectIconPack(string packName)
        {
            _selectedIconPack = packName;
            _settingsService.SetIconPack(packName);
            BuildIconPacksUI();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Reset all settings to defaults?",
                "Reset Settings",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Reset to defaults
                _settingsService.SetTheme("Dark");
                _settingsService.SetCardSize(CardSizeOption.Medium);
                _settingsService.SetCardSpacing(8);
                _settingsService.SetShowFileDetails(true);
                _settingsService.SetIconPack("Default");
                
                _selectedTheme = "Dark";
                _selectedCardSize = CardSizeOption.Medium;
                _selectedIconPack = "Default";
                
                _themeService.ApplyTheme("Dark");
                
                BuildThemesUI();
                BuildLayoutUI();
                BuildIconPacksUI();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
