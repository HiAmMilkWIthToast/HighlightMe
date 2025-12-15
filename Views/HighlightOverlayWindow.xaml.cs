using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace HighlightMe.Views
{
    public partial class HighlightOverlayWindow : Window
    {
        // Win32 constants for making window non-activating
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_NOACTIVATE = 0x08000000;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_EX_TRANSPARENT = 0x00000020;

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        public HighlightOverlayWindow()
        {
            InitializeComponent();
            
            // Set up the window to not steal focus
            SourceInitialized += OnSourceInitialized;
        }

        private void OnSourceInitialized(object? sender, EventArgs e)
        {
            // Get window handle
            var hwnd = new WindowInteropHelper(this).Handle;
            
            // Get current extended style
            int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            
            // Add WS_EX_NOACTIVATE (don't activate/focus), WS_EX_TOOLWINDOW (hide from taskbar), and WS_EX_TRANSPARENT (click-through)
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW | WS_EX_TRANSPARENT);
        }

        protected override void OnActivated(EventArgs e)
        {
            // Prevent activation - immediately deactivate
            base.OnActivated(e);
        }

        public void SetPosition(double x, double y, double width, double height)
        {
            // Add padding for the glow effect
            double padding = 12;
            Left = x - padding;
            Top = y - padding;
            Width = width + (padding * 2);
            Height = height + (padding * 2);
        }

        public void SetColor(string hexColor)
        {
            try
            {
                var color = (Color)ColorConverter.ConvertFromString(hexColor);
                var brush = new SolidColorBrush(color);
                
                // Update border color
                GlowBorder.BorderBrush = brush;
                
                // Update drop shadow color
                if (GlowBorder.Effect is DropShadowEffect shadow)
                {
                    shadow.Color = color;
                }
                
                // Update animated border background with transparency
                color.A = 32; // Low alpha for subtle fill
                AnimatedBorder.Background = new SolidColorBrush(color);
            }
            catch
            {
                // Keep default color on error
            }
        }
    }
}
