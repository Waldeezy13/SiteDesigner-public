using Autodesk.AutoCAD.Windows;
using System.Windows.Forms.Integration; // ElementHost
using System.Windows.Forms;             // DockStyle
using SiteDesigner.Core;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace SiteDesigner.Plugin
{
    public static class PaletteHost
    {
        private static PaletteSet? _ps;
        private static SiteDesigner.UI.SiteDesignerPanel? _panel;
        private static System.Guid _paletteGuid = new System.Guid("8B1B7B52-9B9A-4B61-8D2A-2D8D9426E4E2");
        private static ElementHost? _currentHost;

        public static void ShowOrCreate()
        {
            if (_ps == null)
            {
                CreatePalette(_paletteGuid);
            }
            else if (_panel != null)
            {
                // Re-wire in case panel was recreated
                _panel.ViewModel.ApplyAction = () => _panel.SaveTo(AppState.Config);
                _panel.ViewModel.PlaceTestLayoutAction = () =>
                    LayoutService.PlaceTestLayout(AppState.SiteBoundaryId, AppState.Config);
                _panel.ViewModel.PickBoundaryAction = () =>
                    AcadApp.DocumentManager.MdiActiveDocument.SendStringToExecute("._SDSITESETUP ", true, false, true);
                _panel.ViewModel.DrawSetbackAction = () =>
                    SetbackService.DrawUniformSetback(AppState.SiteBoundaryId, AppState.Config);
            }

            _ps!.Visible = true;
        }

        private static void CreatePalette(System.Guid guid)
        {
            // FIXED: Use the working configuration from the alternative palette
            _ps = new PaletteSet("Site Designer", guid)
            {
                Style = PaletteSetStyles.ShowCloseButton | PaletteSetStyles.ShowPropertiesMenu, // Simplified style
                DockEnabled = DockSides.Left | DockSides.Right, // Allow left/right docking but not all sides
                MinimumSize = new System.Drawing.Size(320, 200),
                Size = new System.Drawing.Size(400, 500),
                KeepFocus = false // This was key - KeepFocus = true interferes with resizing
            };

            _panel = new SiteDesigner.UI.SiteDesignerPanel();
            _panel.Bind(AppState.Config);

            _panel.ViewModel.ApplyAction = () => _panel.SaveTo(AppState.Config);
            _panel.ViewModel.PlaceTestLayoutAction = () =>
                LayoutService.PlaceTestLayout(AppState.SiteBoundaryId, AppState.Config);
            _panel.ViewModel.PickBoundaryAction = () =>
                AcadApp.DocumentManager.MdiActiveDocument.SendStringToExecute("._SDSITESETUP ", true, false, true);
            _panel.ViewModel.DrawSetbackAction = () =>
                SetbackService.DrawUniformSetback(AppState.SiteBoundaryId, AppState.Config);

            // Store ElementHost reference for diagnostics
            _currentHost = new ElementHost 
            { 
                Dock = DockStyle.Fill, 
                Child = _panel,
                AutoSize = false,
                BackColor = System.Drawing.SystemColors.Control
            };
            
            _ps.Add("Design", _currentHost);
        }

        // Recreate the palette and float it; optionally use a new GUID to reset persisted state
        public static void RecreateFloating(bool newGuid)
        {
            // Dispose current palette if any
            if (_ps != null)
            {
                try { _ps.Visible = false; } catch { }
                try { _ps.Dispose(); } catch { }
                _ps = null;
                _panel = null;
                _currentHost = null;
            }

            var guid = newGuid ? System.Guid.NewGuid() : _paletteGuid;
            if (newGuid) _paletteGuid = guid;

            CreatePalette(guid);

            // Float and resize/move
            try { _ps!.Dock = DockSides.None; } catch { }
            _ps!.Size = new System.Drawing.Size(450, 550);
            _ps!.Location = new System.Drawing.Point(100, 100);
            _ps!.Visible = true;
        }

        // Float current palette and apply size/location without recreating it
        public static void FloatAndResize(int width = 450, int height = 550, int left = 100, int top = 100)
        {
            ShowOrCreate();
            if (_ps == null) return;
            try { _ps.Dock = DockSides.None; } catch { }
            _ps.Size = new System.Drawing.Size(width, height);
            _ps.Location = new System.Drawing.Point(left, top);
            _ps.Visible = true;
        }

        // DIAGNOSTIC METHODS
        public static PaletteSet? GetPaletteSet() => _ps;

        public static string GetElementHostInfo()
        {
            if (_currentHost == null) return "ElementHost is null";
            
            try
            {
                return $"AutoSize={_currentHost.AutoSize}, Dock={_currentHost.Dock}, " +
                       $"Size={_currentHost.Size}, Enabled={_currentHost.Enabled}, " +
                       $"Visible={_currentHost.Visible}";
            }
            catch (System.Exception ex)
            {
                return $"Error getting ElementHost info: {ex.Message}";
            }
        }

        // ALTERNATIVE PALETTE CREATION METHOD
        public static void CreateAlternativePalette()
        {
            // Dispose existing first
            if (_ps != null)
            {
                try { _ps.Visible = false; _ps.Dispose(); } catch { }
                _ps = null; _panel = null; _currentHost = null;
            }

            // Create with different approach - minimal styles, floating only
            _ps = new PaletteSet("Site Designer ALT", System.Guid.NewGuid())
            {
                Style = PaletteSetStyles.ShowCloseButton, // Minimal style
                DockEnabled = DockSides.None, // Force floating only
                MinimumSize = new System.Drawing.Size(300, 400),
                Size = new System.Drawing.Size(400, 500),
                KeepFocus = false // Different focus behavior
            };

            _panel = new SiteDesigner.UI.SiteDesignerPanel();
            _panel.Bind(AppState.Config);

            // Wire actions
            _panel.ViewModel.ApplyAction = () => _panel.SaveTo(AppState.Config);
            _panel.ViewModel.PlaceTestLayoutAction = () =>
                LayoutService.PlaceTestLayout(AppState.SiteBoundaryId, AppState.Config);
            _panel.ViewModel.PickBoundaryAction = () =>
                AcadApp.DocumentManager.MdiActiveDocument.SendStringToExecute("._SDSITESETUP ", true, false, true);
            _panel.ViewModel.DrawSetbackAction = () =>
                SetbackService.DrawUniformSetback(AppState.SiteBoundaryId, AppState.Config);

            // Alternative ElementHost configuration
            _currentHost = new ElementHost 
            { 
                Dock = DockStyle.Fill, 
                Child = _panel,
                AutoSize = false,
                BackColor = System.Drawing.SystemColors.Window // Different background
            };
            
            _ps.Add("Design", _currentHost);
            
            // Force float and show
            _ps.Dock = DockSides.None;
            _ps.Location = new System.Drawing.Point(150, 150);
            _ps.Visible = true;
        }

        public static SiteDesigner.UI.SiteDesignerPanel? Panel => _panel;
    }
}
