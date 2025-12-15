using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HighlightMe.Helpers;
using HighlightMe.Models;

namespace HighlightMe.Services
{
    public class DesktopScannerService
    {
        public IEnumerable<DesktopItem> ScanDesktop(bool includePublicDesktop = true, bool includeHidden = true)
        {
            var items = new List<DesktopItem>();

            // Scan user's desktop
            var userDesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            items.AddRange(ScanDirectory(userDesktopPath, includeHidden));

            // Scan public desktop if requested
            if (includePublicDesktop)
            {
                var publicDesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory);
                if (!string.IsNullOrEmpty(publicDesktopPath) && Directory.Exists(publicDesktopPath))
                {
                    var publicItems = ScanDirectory(publicDesktopPath, includeHidden);
                    // Avoid duplicates by checking names
                    var existingNames = new HashSet<string>(items.Select(i => i.Name), StringComparer.OrdinalIgnoreCase);
                    items.AddRange(publicItems.Where(i => !existingNames.Contains(i.Name)));
                }
            }

            return items.OrderBy(i => !i.IsDirectory).ThenBy(i => i.Name);
        }

        private IEnumerable<DesktopItem> ScanDirectory(string path, bool includeHidden)
        {
            var items = new List<DesktopItem>();

            try
            {
                // Get directories
                foreach (var dir in Directory.GetDirectories(path))
                {
                    try
                    {
                        var dirInfo = new DirectoryInfo(dir);
                        
                        bool isHidden = (dirInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
                        bool isSystem = (dirInfo.Attributes & FileAttributes.System) == FileAttributes.System;
                        bool isLocked = (dirInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;
                        
                        // Skip system folders, but include hidden if requested
                        if (isSystem) continue;
                        if (isHidden && !includeHidden) continue;

                        items.Add(new DesktopItem
                        {
                            Name = dirInfo.Name,
                            FullPath = dirInfo.FullName,
                            IsDirectory = true,
                            IsHidden = isHidden,
                            IsLocked = isLocked,
                            DateModified = dirInfo.LastWriteTime,
                            Icon = IconHelper.GetIcon(dirInfo.FullName, true)
                        });
                    }
                    catch
                    {
                        // Skip directories we can't access
                    }
                }

                // Get files
                foreach (var file in Directory.GetFiles(path))
                {
                    try
                    {
                        var fileInfo = new FileInfo(file);
                        
                        bool isHidden = (fileInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
                        bool isLocked = (fileInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;
                        
                        // Skip desktop.ini
                        if (fileInfo.Name.Equals("desktop.ini", StringComparison.OrdinalIgnoreCase))
                            continue;
                        
                        // Skip hidden files if not requested
                        if (isHidden && !includeHidden) continue;

                        items.Add(new DesktopItem
                        {
                            Name = fileInfo.Name,
                            FullPath = fileInfo.FullName,
                            IsDirectory = false,
                            IsHidden = isHidden,
                            IsLocked = isLocked,
                            Size = fileInfo.Length,
                            DateModified = fileInfo.LastWriteTime,
                            Icon = IconHelper.GetIcon(fileInfo.FullName, false)
                        });
                    }
                    catch
                    {
                        // Skip files we can't access
                    }
                }
            }
            catch
            {
                // Return whatever we managed to get
            }

            return items;
        }

        public static void ToggleHidden(string path)
        {
            try
            {
                var attributes = File.GetAttributes(path);
                if ((attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                {
                    // Remove hidden attribute
                    File.SetAttributes(path, attributes & ~FileAttributes.Hidden);
                }
                else
                {
                    // Add hidden attribute
                    File.SetAttributes(path, attributes | FileAttributes.Hidden);
                }
            }
            catch
            {
                // Ignore errors (permission issues, etc.)
            }
        }

        public static void ToggleLock(string path)
        {
            try
            {
                var attributes = File.GetAttributes(path);
                if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    // Remove readonly attribute (unlock)
                    File.SetAttributes(path, attributes & ~FileAttributes.ReadOnly);
                }
                else
                {
                    // Add readonly attribute (lock)
                    File.SetAttributes(path, attributes | FileAttributes.ReadOnly);
                }
            }
            catch
            {
                // Ignore errors (permission issues, etc.)
            }
        }
    }
}
