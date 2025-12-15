using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HighlightMe.Helpers
{
    public static class IconHelper
    {
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes,
            ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        private const uint SHGFI_ICON = 0x100;
        private const uint SHGFI_LARGEICON = 0x0;
        private const uint SHGFI_SMALLICON = 0x1;
        private const uint SHGFI_USEFILEATTRIBUTES = 0x10;
        private const uint FILE_ATTRIBUTE_NORMAL = 0x80;
        private const uint FILE_ATTRIBUTE_DIRECTORY = 0x10;

        public static ImageSource? GetIcon(string path, bool isDirectory, bool largeIcon = true)
        {
            try
            {
                var shinfo = new SHFILEINFO();
                uint flags = SHGFI_ICON | (largeIcon ? SHGFI_LARGEICON : SHGFI_SMALLICON);

                // If file doesn't exist, use file attributes to get icon by extension
                if (!File.Exists(path) && !Directory.Exists(path))
                {
                    flags |= SHGFI_USEFILEATTRIBUTES;
                    uint fileAttributes = isDirectory ? FILE_ATTRIBUTE_DIRECTORY : FILE_ATTRIBUTE_NORMAL;
                    SHGetFileInfo(path, fileAttributes, ref shinfo, (uint)Marshal.SizeOf(shinfo), flags);
                }
                else
                {
                    SHGetFileInfo(path, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), flags);
                }

                if (shinfo.hIcon == IntPtr.Zero)
                    return null;

                var imageSource = Imaging.CreateBitmapSourceFromHIcon(
                    shinfo.hIcon,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());

                DestroyIcon(shinfo.hIcon);

                imageSource.Freeze(); // Make it thread-safe
                return imageSource;
            }
            catch
            {
                return null;
            }
        }

        public static ImageSource? GetFolderIcon(bool largeIcon = true)
        {
            return GetIcon("folder", true, largeIcon);
        }

        public static ImageSource? GetFileIcon(string extension, bool largeIcon = true)
        {
            return GetIcon("file" + extension, false, largeIcon);
        }
    }
}
