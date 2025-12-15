using System.Collections.Generic;

namespace HighlightMe.Models
{
    public class AppSettings
    {
        public ThemeSettings Theme { get; set; } = new();
        public LayoutSettings Layout { get; set; } = new();
        public IconPackSettings IconPack { get; set; } = new();
    }

    public class ThemeSettings
    {
        public string CurrentTheme { get; set; } = "Dark";
        public string AccentColor { get; set; } = "#E94560";
        
        public static List<string> AvailableThemes => new()
        {
            "Dark", "Light", "Ocean", "Forest", "Sunset"
        };
    }

    public class LayoutSettings
    {
        public CardSizeOption CardSize { get; set; } = CardSizeOption.Medium;
        public int CardSpacing { get; set; } = 8;
        public bool ShowFileDetails { get; set; } = true;
        
        public int GetCardWidth() => CardSize switch
        {
            CardSizeOption.Small => 150,
            CardSizeOption.Medium => 200,
            CardSizeOption.Large => 280,
            _ => 200
        };
        
        public int GetCardPadding() => CardSize switch
        {
            CardSizeOption.Small => 10,
            CardSizeOption.Medium => 15,
            CardSizeOption.Large => 20,
            _ => 15
        };
    }

    public enum CardSizeOption
    {
        Small,
        Medium,
        Large
    }

    public class IconPackSettings
    {
        public string CurrentPack { get; set; } = "Default";
        
        public static List<IconPackInfo> AvailablePacks => new()
        {
            new IconPackInfo { Name = "Default", Description = "System file icons", Icon = "📁" },
            new IconPackInfo { Name = "Emoji", Description = "Colorful emoji icons", Icon = "🎨" },
            new IconPackInfo { Name = "Minimal", Description = "Simple monochrome icons", Icon = "◻️" },
            new IconPackInfo { Name = "Colorful", Description = "Vibrant colored icons", Icon = "🌈" }
        };
    }

    public class IconPackInfo
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Icon { get; set; } = "";
    }
}
