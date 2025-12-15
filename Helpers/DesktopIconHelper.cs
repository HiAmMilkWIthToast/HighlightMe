using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace HighlightMe.Helpers
{
    public static class DesktopIconHelper
    {
        #region Win32 APIs

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string? lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string? lpszWindow);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint dwFreeType);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out int lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out int lpNumberOfBytesWritten);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

        private const uint LVM_FIRST = 0x1000;
        private const uint LVM_GETITEMCOUNT = LVM_FIRST + 4;
        private const uint LVM_GETITEMPOSITION = LVM_FIRST + 16;
        private const uint LVM_GETITEMRECT = LVM_FIRST + 14;
        
        // Use the Unicode version for getting item text
        private const uint LVM_GETITEMTEXTW = LVM_FIRST + 115;

        private const uint PROCESS_VM_OPERATION = 0x0008;
        private const uint PROCESS_VM_READ = 0x0010;
        private const uint PROCESS_VM_WRITE = 0x0020;

        private const uint MEM_COMMIT = 0x1000;
        private const uint MEM_RELEASE = 0x8000;
        private const uint PAGE_READWRITE = 0x04;

        private const int LVIR_BOUNDS = 0;
        private const int LVIR_ICON = 1;  // Get just the icon area, not the text
        private const uint LVIF_TEXT = 0x0001;

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        // Use explicit layout for 64-bit compatibility
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct LVITEM
        {
            public uint mask;
            public int iItem;
            public int iSubItem;
            public uint state;
            public uint stateMask;
            public IntPtr pszText;
            public int cchTextMax;
            public int iImage;
            public IntPtr lParam;
            public int iIndent;
            public int iGroupId;
            public uint cColumns;
            public IntPtr puColumns;
            public IntPtr piColFmt;
            public int iGroup;
        }

        #endregion

        public class DesktopIconInfo
        {
            public string Name { get; set; } = string.Empty;
            public int X { get; set; }
            public int Y { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public System.Windows.Rect Bounds => new System.Windows.Rect(X, Y, Width, Height);
        }

        public static IntPtr GetDesktopListViewHandle()
        {
            // Try Progman first (normal desktop)
            IntPtr progman = FindWindow("Progman", null);
            IntPtr shellView = FindWindowEx(progman, IntPtr.Zero, "SHELLDLL_DefView", null);

            if (shellView == IntPtr.Zero)
            {
                // Desktop might be in a WorkerW window (when slideshow wallpaper is active)
                IntPtr workerW = IntPtr.Zero;

                do
                {
                    workerW = FindWindowEx(IntPtr.Zero, workerW, "WorkerW", null);
                    shellView = FindWindowEx(workerW, IntPtr.Zero, "SHELLDLL_DefView", null);
                } while (shellView == IntPtr.Zero && workerW != IntPtr.Zero);
            }

            if (shellView == IntPtr.Zero)
                return IntPtr.Zero;

            return FindWindowEx(shellView, IntPtr.Zero, "SysListView32", null);
        }

        public static List<DesktopIconInfo> GetDesktopIcons()
        {
            var icons = new List<DesktopIconInfo>();
            IntPtr listView = GetDesktopListViewHandle();

            if (listView == IntPtr.Zero)
            {
                Debug.WriteLine("DesktopIconHelper: Could not find desktop ListView");
                return icons;
            }

            int count = (int)SendMessage(listView, LVM_GETITEMCOUNT, IntPtr.Zero, IntPtr.Zero);
            if (count == 0)
            {
                Debug.WriteLine("DesktopIconHelper: No items found on desktop");
                return icons;
            }

            Debug.WriteLine($"DesktopIconHelper: Found {count} items on desktop");

            // Get the process ID of the desktop window
            GetWindowThreadProcessId(listView, out uint processId);
            IntPtr hProcess = OpenProcess(PROCESS_VM_OPERATION | PROCESS_VM_READ | PROCESS_VM_WRITE, false, processId);

            if (hProcess == IntPtr.Zero)
            {
                Debug.WriteLine("DesktopIconHelper: Could not open Explorer process");
                return icons;
            }

            try
            {
                int lvItemSize = Marshal.SizeOf<LVITEM>();
                const int textBufferSize = 520; // MAX_PATH * 2 for Unicode

                // Allocate memory in the remote process
                IntPtr pPoint = VirtualAllocEx(hProcess, IntPtr.Zero, (uint)Marshal.SizeOf<POINT>(), MEM_COMMIT, PAGE_READWRITE);
                IntPtr pRect = VirtualAllocEx(hProcess, IntPtr.Zero, (uint)Marshal.SizeOf<RECT>(), MEM_COMMIT, PAGE_READWRITE);
                IntPtr pLvItem = VirtualAllocEx(hProcess, IntPtr.Zero, (uint)lvItemSize, MEM_COMMIT, PAGE_READWRITE);
                IntPtr pText = VirtualAllocEx(hProcess, IntPtr.Zero, textBufferSize, MEM_COMMIT, PAGE_READWRITE);

                if (pPoint == IntPtr.Zero || pRect == IntPtr.Zero || pLvItem == IntPtr.Zero || pText == IntPtr.Zero)
                {
                    Debug.WriteLine("DesktopIconHelper: Failed to allocate remote memory");
                    return icons;
                }

                try
                {
                    for (int i = 0; i < count; i++)
                    {
                        var iconInfo = new DesktopIconInfo();

                        // Get item name using LVM_GETITEMTEXTW (Unicode)
                        var lvItem = new LVITEM
                        {
                            mask = LVIF_TEXT,
                            iItem = i,
                            iSubItem = 0,
                            pszText = pText,
                            cchTextMax = 260
                        };

                        byte[] lvItemBytes = StructToBytes(lvItem);
                        WriteProcessMemory(hProcess, pLvItem, lvItemBytes, (uint)lvItemBytes.Length, out _);
                        
                        // Use the Unicode version of LVM_GETITEMTEXT
                        int textLen = (int)SendMessage(listView, LVM_GETITEMTEXTW, (IntPtr)i, pLvItem);

                        byte[] textBuffer = new byte[textBufferSize];
                        ReadProcessMemory(hProcess, pText, textBuffer, textBufferSize, out _);
                        
                        // Find the actual string length (look for null terminator)
                        string name = "";
                        for (int j = 0; j < textBuffer.Length - 1; j += 2)
                        {
                            if (textBuffer[j] == 0 && textBuffer[j + 1] == 0)
                            {
                                name = Encoding.Unicode.GetString(textBuffer, 0, j);
                                break;
                            }
                        }
                        
                        if (string.IsNullOrEmpty(name) && textLen > 0)
                        {
                            // Fallback: use the text length returned
                            name = Encoding.Unicode.GetString(textBuffer, 0, textLen * 2);
                        }
                        
                        iconInfo.Name = name.TrimEnd('\0');

                        // Skip items with empty names
                        if (string.IsNullOrWhiteSpace(iconInfo.Name))
                        {
                            Debug.WriteLine($"DesktopIconHelper: Item {i} has empty name, skipping");
                            continue;
                        }

                        // Get item rectangle (includes icon and text label)
                        byte[] rectBytes = new byte[Marshal.SizeOf<RECT>()];
                        rectBytes[0] = LVIR_BOUNDS; // Get full bounds including text
                        WriteProcessMemory(hProcess, pRect, rectBytes, (uint)rectBytes.Length, out _);
                        SendMessage(listView, LVM_GETITEMRECT, (IntPtr)i, pRect);
                        ReadProcessMemory(hProcess, pRect, rectBytes, (uint)rectBytes.Length, out _);
                        var rect = BytesToStruct<RECT>(rectBytes);

                        // Convert top-left and bottom-right to screen coordinates
                        var topLeft = new POINT { X = rect.Left, Y = rect.Top };
                        var bottomRight = new POINT { X = rect.Right, Y = rect.Bottom };
                        ClientToScreen(listView, ref topLeft);
                        ClientToScreen(listView, ref bottomRight);

                        iconInfo.X = topLeft.X;
                        iconInfo.Y = topLeft.Y;
                        iconInfo.Width = bottomRight.X - topLeft.X;
                        iconInfo.Height = bottomRight.Y - topLeft.Y;

                        // Use default size if we couldn't get valid dimensions
                        if (iconInfo.Width <= 0) iconInfo.Width = 75;
                        if (iconInfo.Height <= 0) iconInfo.Height = 75;

                        Debug.WriteLine($"DesktopIconHelper: Found icon '{iconInfo.Name}' at ({iconInfo.X}, {iconInfo.Y})");
                        icons.Add(iconInfo);
                    }
                }
                finally
                {
                    VirtualFreeEx(hProcess, pPoint, 0, MEM_RELEASE);
                    VirtualFreeEx(hProcess, pRect, 0, MEM_RELEASE);
                    VirtualFreeEx(hProcess, pLvItem, 0, MEM_RELEASE);
                    VirtualFreeEx(hProcess, pText, 0, MEM_RELEASE);
                }
            }
            finally
            {
                CloseHandle(hProcess);
            }

            Debug.WriteLine($"DesktopIconHelper: Successfully retrieved {icons.Count} icons");
            return icons;
        }

        private static byte[] StructToBytes<T>(T obj) where T : struct
        {
            int size = Marshal.SizeOf<T>();
            byte[] bytes = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(obj, ptr, false);
                Marshal.Copy(ptr, bytes, 0, size);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
            return bytes;
        }

        private static T BytesToStruct<T>(byte[] bytes) where T : struct
        {
            int size = Marshal.SizeOf<T>();
            IntPtr ptr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.Copy(bytes, 0, ptr, size);
                return Marshal.PtrToStructure<T>(ptr);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }
    }
}
