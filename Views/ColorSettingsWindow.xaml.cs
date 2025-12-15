using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using HighlightMe.Models;
using HighlightMe.Services;

namespace HighlightMe.Views
{
    public partial class ColorSettingsWindow : Window
    {
        private readonly CategorySettingsService _settingsService;
        
        // Preset colors for the color picker
        private static readonly string[] PresetColors = new[]
        {
            "#FFD700", // Gold
            "#FF6B6B", // Coral
            "#E91E63", // Pink
            "#9C27B0", // Purple
            "#673AB7", // Deep Purple
            "#3F51B5", // Indigo
            "#2196F3", // Blue
            "#03A9F4", // Light Blue
            "#00BCD4", // Cyan
            "#009688", // Teal
            "#4CAF50", // Green
            "#8BC34A", // Light Green
            "#CDDC39", // Lime
            "#FFEB3B", // Yellow
            "#FFC107", // Amber
            "#FF9800", // Orange
            "#FF5722", // Deep Orange
            "#795548", // Brown
            "#9E9E9E", // Grey
            "#607D8B"  // Blue Grey
        };

        public ColorSettingsWindow(CategorySettingsService settingsService)
        {
            InitializeComponent();
            _settingsService = settingsService;
            BuildCategoryUI();
        }

        private void BuildCategoryUI()
        {
            CategoryPanel.Children.Clear();

            // Add Folders category
            AddCategoryRow("📁 Folders", _settingsService.Settings.FolderColor, (color) =>
            {
                _settingsService.SetFolderColor(color);
            });

            // Add Default category
            AddCategoryRow("📄 Other Files", _settingsService.Settings.DefaultColor, (color) =>
            {
                _settingsService.SetDefaultColor(color);
            });

            // Add separator
            CategoryPanel.Children.Add(new Border
            {
                Height = 1,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3A3A5A")),
                Margin = new Thickness(0, 15, 0, 15)
            });

            // Add each file category
            foreach (var category in _settingsService.Settings.Categories)
            {
                string icon = GetCategoryIcon(category.Name);
                AddCategoryRow($"{icon} {category.Name}", category.GlowColor, (color) =>
                {
                    _settingsService.SetCategoryColor(category.Name, color);
                }, category.Extensions);
            }
        }

        private string GetCategoryIcon(string categoryName)
        {
            return categoryName switch
            {
                "Images" => "🖼️",
                "Documents" => "📝",
                "Videos" => "🎬",
                "Audio" => "🎵",
                "Archives" => "📦",
                "Code" => "💻",
                "Executables" => "⚡",
                _ => "📄"
            };
        }

        private void AddCategoryRow(string label, string currentColor, Action<string> onColorChanged, System.Collections.Generic.List<string>? extensions = null)
        {
            var border = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#252540")),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(15, 12, 15, 12),
                Margin = new Thickness(0, 5, 0, 5)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Left side: label and extensions
            var leftStack = new StackPanel();
            
            var labelText = new TextBlock
            {
                Text = label,
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Colors.White)
            };
            leftStack.Children.Add(labelText);

            if (extensions != null && extensions.Count > 0)
            {
                var extText = new TextBlock
                {
                    Text = string.Join(", ", extensions),
                    FontSize = 11,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#707090")),
                    TextWrapping = TextWrapping.Wrap,
                    MaxWidth = 250,
                    Margin = new Thickness(0, 3, 0, 0)
                };
                leftStack.Children.Add(extText);
            }

            Grid.SetColumn(leftStack, 0);
            grid.Children.Add(leftStack);

            // Right side: color picker
            var colorStack = new StackPanel { Orientation = Orientation.Horizontal };
            
            // Current color indicator
            var currentColorRect = new Rectangle
            {
                Width = 35,
                Height = 25,
                RadiusX = 5,
                RadiusY = 5,
                Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(currentColor)),
                Stroke = new SolidColorBrush(Colors.White),
                StrokeThickness = 2,
                Margin = new Thickness(0, 0, 10, 0)
            };
            colorStack.Children.Add(currentColorRect);

            // Color picker button
            var pickerButton = new Button
            {
                Content = "🎨",
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3A3A5A")),
                Foreground = new SolidColorBrush(Colors.White),
                BorderThickness = new Thickness(0),
                Padding = new Thickness(10, 5, 10, 5),
                Cursor = System.Windows.Input.Cursors.Hand
            };
            pickerButton.Click += (s, e) =>
            {
                ShowColorPicker(currentColorRect, onColorChanged);
            };
            colorStack.Children.Add(pickerButton);

            Grid.SetColumn(colorStack, 1);
            grid.Children.Add(colorStack);

            border.Child = grid;
            CategoryPanel.Children.Add(border);
        }

        private void ShowColorPicker(Rectangle colorRect, Action<string> onColorChanged)
        {
            var popup = new Window
            {
                Title = "Pick a Color",
                Width = 320,
                Height = 220,
                WindowStyle = WindowStyle.ToolWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E2A4A")),
                ResizeMode = ResizeMode.NoResize
            };

            var wrapPanel = new WrapPanel
            {
                Margin = new Thickness(15)
            };

            foreach (var color in PresetColors)
            {
                var colorButton = new Button
                {
                    Width = 40,
                    Height = 35,
                    Margin = new Thickness(4),
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color)),
                    BorderThickness = new Thickness(2),
                    BorderBrush = new SolidColorBrush(Colors.Transparent),
                    Cursor = System.Windows.Input.Cursors.Hand
                };
                colorButton.MouseEnter += (s, e) => colorButton.BorderBrush = new SolidColorBrush(Colors.White);
                colorButton.MouseLeave += (s, e) => colorButton.BorderBrush = new SolidColorBrush(Colors.Transparent);
                colorButton.Click += (s, e) =>
                {
                    colorRect.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
                    onColorChanged(color);
                    popup.Close();
                };
                wrapPanel.Children.Add(colorButton);
            }

            popup.Content = wrapPanel;
            popup.ShowDialog();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Reset all colors to defaults?",
                "Reset Colors",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Reload defaults
                var defaults = CategorySettings.CreateDefault();
                _settingsService.Settings.DefaultColor = defaults.DefaultColor;
                _settingsService.Settings.FolderColor = defaults.FolderColor;
                _settingsService.Settings.Categories = defaults.Categories;
                _settingsService.SaveSettings();
                BuildCategoryUI();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
