using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace HighlightMe.Views
{
    public partial class ArrowPointerWindow : Window
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

        public ArrowPointerWindow()
        {
            InitializeComponent();
            SourceInitialized += OnSourceInitialized;
        }

        private void OnSourceInitialized(object? sender, EventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW | WS_EX_TRANSPARENT);
        }

        public void SetPosition(double iconX, double iconY)
        {
            // Position arrow above the icon, centered
            Left = iconX + 5; // Offset to center arrow over icon
            Top = iconY - 85;  // Position above the icon
        }
    }
}
