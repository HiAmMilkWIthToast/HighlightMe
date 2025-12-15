using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace HighlightMe.Views
{
    public partial class NewItemOverlayWindow : Window
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

        public string ItemPath { get; set; } = string.Empty;

        public NewItemOverlayWindow()
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

        public void SetPosition(double x, double y, double width, double height)
        {
            double padding = 20;
            Left = x - padding;
            Top = y - padding;
            Width = width + (padding * 2);
            Height = height + (padding * 2);
        }
    }
}
