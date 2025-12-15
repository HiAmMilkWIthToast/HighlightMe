using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

namespace HighlightMe.Services
{
    public class FileLockService
    {
        private readonly Dictionary<string, FileStream> _lockedFileHandles = new();
        
        public static bool IsRunningAsAdmin()
        {
            try
            {
                using var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Lock a file using NTFS permissions (denies delete) + file handle
        /// </summary>
        public bool LockFile(string path)
        {
            bool success = false;
            
            // Method 1: NTFS permissions (permanent, requires admin for best results)
            success = SetDenyDeletePermission(path, true);
            
            // Method 2: Keep file handle open (runtime protection)
            try
            {
                if (!_lockedFileHandles.ContainsKey(path) && File.Exists(path))
                {
                    // Open with FileShare.Read so file can still be read but not deleted
                    var handle = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                    _lockedFileHandles[path] = handle;
                    success = true;
                }
            }
            catch
            {
                // File handle lock failed, but NTFS lock might have worked
            }

            return success;
        }

        /// <summary>
        /// Unlock a file by removing NTFS deny permission and releasing file handle
        /// </summary>
        public bool UnlockFile(string path)
        {
            bool success = false;
            
            // Remove NTFS deny permission
            success = SetDenyDeletePermission(path, false);
            
            // Release file handle
            if (_lockedFileHandles.TryGetValue(path, out var handle))
            {
                try
                {
                    handle.Close();
                    handle.Dispose();
                    _lockedFileHandles.Remove(path);
                    success = true;
                }
                catch { }
            }

            return success;
        }

        /// <summary>
        /// Toggle lock state for a file
        /// </summary>
        public bool ToggleLock(string path, bool currentlyLocked)
        {
            if (currentlyLocked)
            {
                return UnlockFile(path);
            }
            else
            {
                return LockFile(path);
            }
        }

        /// <summary>
        /// Set or remove deny delete permission using icacls command
        /// </summary>
        private bool SetDenyDeletePermission(string path, bool deny)
        {
            try
            {
                var currentUser = Environment.UserName;
                string arguments;
                
                if (deny)
                {
                    // Deny delete permission for current user
                    arguments = $"\"{path}\" /deny \"{currentUser}:(D,DC)\"";
                }
                else
                {
                    // Remove the deny rule
                    arguments = $"\"{path}\" /remove:d \"{currentUser}\"";
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = "icacls",
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using var process = Process.Start(startInfo);
                process?.WaitForExit(5000);
                
                return process?.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check if a file is currently locked by this app
        /// </summary>
        public bool IsFileLocked(string path)
        {
            return _lockedFileHandles.ContainsKey(path);
        }

        /// <summary>
        /// Release all file handles on app shutdown
        /// </summary>
        public void ReleaseAllLocks()
        {
            foreach (var handle in _lockedFileHandles.Values)
            {
                try
                {
                    handle.Close();
                    handle.Dispose();
                }
                catch { }
            }
            _lockedFileHandles.Clear();
        }
    }
}
