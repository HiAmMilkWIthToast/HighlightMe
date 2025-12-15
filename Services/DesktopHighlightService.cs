using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Threading;
using HighlightMe.Helpers;
using HighlightMe.Views;

namespace HighlightMe.Services
{
    public class DesktopHighlightService : IDisposable
    {
        private readonly List<HighlightOverlayWindow> _activeOverlays = new();
        private readonly List<ArrowPointerWindow> _activeArrows = new();
        private readonly List<NewItemOverlayWindow> _newItemOverlays = new();
        private List<DesktopIconHelper.DesktopIconInfo> _cachedIcons = new();
        private DateTime _lastRefresh = DateTime.MinValue;
        private readonly TimeSpan _cacheExpiry = TimeSpan.FromSeconds(5);
        private readonly CategorySettingsService _categorySettings;
        
        // Real-time tracking
        private readonly DispatcherTimer _trackingTimer;
        private string _currentSearchQuery = string.Empty;
        private readonly Dictionary<string, HighlightOverlayWindow> _overlaysByName = new();
        private readonly Dictionary<string, ArrowPointerWindow> _arrowsByName = new();
        private bool _isTracking = false;

        public bool ShowArrows { get; set; } = true;

        public DesktopHighlightService()
        {
            _categorySettings = new CategorySettingsService();
            
            // Timer for real-time position tracking (updates every 200ms)
            _trackingTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(200)
            };
            _trackingTimer.Tick += TrackingTimer_Tick;
        }

        public CategorySettingsService CategorySettings => _categorySettings;

        private void TrackingTimer_Tick(object? sender, EventArgs e)
        {
            if (!_isTracking) return;
            
            // Refresh icon positions
            _cachedIcons = DesktopIconHelper.GetDesktopIcons();
            _lastRefresh = DateTime.Now;
            
            // Update overlay positions for search highlights
            foreach (var kvp in _overlaysByName)
            {
                var iconName = kvp.Key;
                var overlay = kvp.Value;
                
                var icon = _cachedIcons.FirstOrDefault(i => 
                    i.Name.Equals(iconName, StringComparison.OrdinalIgnoreCase));
                
                if (icon != null)
                {
                    overlay.SetPosition(icon.X, icon.Y, icon.Width, icon.Height);
                }
            }
            
            // Update arrow positions
            foreach (var kvp in _arrowsByName)
            {
                var iconName = kvp.Key;
                var arrow = kvp.Value;
                
                var icon = _cachedIcons.FirstOrDefault(i => 
                    i.Name.Equals(iconName, StringComparison.OrdinalIgnoreCase));
                
                if (icon != null)
                {
                    arrow.SetPosition(icon.X, icon.Y);
                }
            }
            
            // Update new item overlay positions
            foreach (var overlay in _newItemOverlays)
            {
                if (!string.IsNullOrEmpty(overlay.ItemPath))
                {
                    var fileName = Path.GetFileName(overlay.ItemPath);
                    var icon = _cachedIcons.FirstOrDefault(i => 
                        i.Name.Equals(fileName, StringComparison.OrdinalIgnoreCase));
                    
                    if (icon != null)
                    {
                        overlay.SetPosition(icon.X, icon.Y, icon.Width, icon.Height);
                    }
                }
            }
        }

        public void RefreshIconPositions()
        {
            _cachedIcons = DesktopIconHelper.GetDesktopIcons();
            _lastRefresh = DateTime.Now;
        }

        public void HighlightMatchingIcons(string searchQuery, Dictionary<string, bool>? itemIsDirectory = null)
        {
            // Clear existing search overlays
            ClearSearchHighlights();

            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                StopTracking();
                return;
            }

            _currentSearchQuery = searchQuery;

            // Always refresh for real-time tracking
            RefreshIconPositions();

            // Find matching icons
            var matchingIcons = _cachedIcons
                .Where(icon => icon.Name.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                .ToList();

            // Create overlay windows and arrows for each matching icon
            foreach (var icon in matchingIcons)
            {
                // Determine if this is a directory
                bool isDirectory = itemIsDirectory != null && 
                    itemIsDirectory.TryGetValue(icon.Name, out bool isDir) && isDir;
                
                // Get the appropriate color for this file type
                string glowColor = _categorySettings.GetColorForFile(icon.Name, isDirectory);

                // Glow overlay with category color
                var overlay = new HighlightOverlayWindow();
                overlay.ShowActivated = false;
                overlay.SetColor(glowColor);
                overlay.SetPosition(icon.X, icon.Y, icon.Width, icon.Height);
                overlay.Show();
                _activeOverlays.Add(overlay);
                _overlaysByName[icon.Name] = overlay;

                // Bouncing arrow pointer
                if (ShowArrows)
                {
                    var arrow = new ArrowPointerWindow();
                    arrow.ShowActivated = false;
                    arrow.SetPosition(icon.X, icon.Y);
                    arrow.Show();
                    _activeArrows.Add(arrow);
                    _arrowsByName[icon.Name] = arrow;
                }
            }

            // Start real-time tracking if we have overlays
            if (_activeOverlays.Count > 0 || _activeArrows.Count > 0)
            {
                StartTracking();
            }
        }

        private void StartTracking()
        {
            if (!_isTracking)
            {
                _isTracking = true;
                _trackingTimer.Start();
            }
        }

        private void StopTracking()
        {
            _isTracking = false;
            _trackingTimer.Stop();
        }

        public void HighlightNewItems(IEnumerable<string> newItemPaths)
        {
            // Clear existing new item overlays
            ClearNewItemHighlights();

            // Refresh icon positions to find new items
            RefreshIconPositions();

            foreach (var path in newItemPaths)
            {
                // Find the icon for this path
                var icon = _cachedIcons.FirstOrDefault(i => 
                    path.EndsWith(i.Name, StringComparison.OrdinalIgnoreCase) ||
                    i.Name.Equals(Path.GetFileName(path), StringComparison.OrdinalIgnoreCase));

                if (icon != null)
                {
                    var overlay = new NewItemOverlayWindow();
                    overlay.ItemPath = path;
                    overlay.ShowActivated = false;
                    overlay.SetPosition(icon.X, icon.Y, icon.Width, icon.Height);
                    overlay.Show();
                    _newItemOverlays.Add(overlay);
                }
            }

            // Start tracking for new item overlays too
            if (_newItemOverlays.Count > 0)
            {
                StartTracking();
            }
        }

        public void ClearSearchHighlights()
        {
            foreach (var overlay in _activeOverlays)
            {
                overlay.Close();
            }
            _activeOverlays.Clear();
            _overlaysByName.Clear();

            foreach (var arrow in _activeArrows)
            {
                arrow.Close();
            }
            _activeArrows.Clear();
            _arrowsByName.Clear();

            // Stop tracking if no overlays remain
            if (_newItemOverlays.Count == 0)
            {
                StopTracking();
            }
        }

        public void ClearNewItemHighlights()
        {
            foreach (var overlay in _newItemOverlays)
            {
                overlay.Close();
            }
            _newItemOverlays.Clear();

            // Stop tracking if no search overlays remain either
            if (_activeOverlays.Count == 0 && _activeArrows.Count == 0)
            {
                StopTracking();
            }
        }

        public void ClearHighlights()
        {
            ClearSearchHighlights();
            ClearNewItemHighlights();
            StopTracking();
        }

        public int GetMatchingIconCount(string searchQuery)
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
                return 0;

            if (DateTime.Now - _lastRefresh > _cacheExpiry)
            {
                RefreshIconPositions();
            }

            return _cachedIcons
                .Count(icon => icon.Name.Contains(searchQuery, StringComparison.OrdinalIgnoreCase));
        }

        public void Dispose()
        {
            StopTracking();
            ClearHighlights();
        }
    }
}

