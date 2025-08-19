using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly: CommandClass(typeof(SiteDesigner.Plugin.SiteDesignerCommands))]

namespace SiteDesigner.Plugin
{
    public class SiteDesignerCommands
    {
        [CommandMethod("HELLOTEST")]
        public void HelloTest()
        {
            var ed = AcadApp.DocumentManager.MdiActiveDocument.Editor;
            ed.WriteMessage("\nHELLOTEST: command discovered and running.");
        }

        [CommandMethod("SDSTART")]
        public void StartPalette()
        {
            var ed = AcadApp.DocumentManager.MdiActiveDocument.Editor;
            try
            {
                PaletteHost.ShowOrCreate();
                ed.WriteMessage("\nSDSTART: palette attempted to open.");
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage($"\nSDSTART error: {ex.GetType().Name}: {ex.Message}");
            }
        }

        [CommandMethod("SDSITESETUP")]
        public void SiteSetup()
        {
            var doc = AcadApp.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;

            var peo = new PromptEntityOptions("\nSelect closed site boundary polyline: ");
            peo.SetRejectMessage("\nMust be a polyline.");
            peo.AddAllowedClass(typeof(Polyline), exactMatch: false);

            var per = ed.GetEntity(peo);
            if (per.Status != PromptStatus.OK) return;

            using var tr = doc.TransactionManager.StartTransaction();
            var ent = (Entity)tr.GetObject(per.ObjectId, OpenMode.ForRead);
            if (ent is Polyline pl && pl.Closed)
            {
                AppState.SiteBoundaryId = per.ObjectId;
                ed.WriteMessage("\nSite boundary stored.");

                // Prompt for inside point to help determine inward offset direction
                var ppo = new PromptPointOptions("\nClick a point INSIDE the site: ");
                var pr = ed.GetPoint(ppo);
                if (pr.Status == PromptStatus.OK)
                {
                    AppState.InsidePoint = pr.Value;
                    ed.WriteMessage("\nInside point stored for offset direction detection.");
                }
                else
                {
                    AppState.InsidePoint = null;
                    ed.WriteMessage("\nNo inside point specified - will use area-based offset detection.");
                }
            }
            else
            {
                ed.WriteMessage("\nSelected polyline is not closed.");
            }
            tr.Commit();
        }

        // Float and resize palette if floating resize is not working or UI state persisted too small
        [CommandMethod("SDFLOATPALETTE")]
        public void FloatPalette()
        {
            PaletteHost.FloatAndResize();
        }

        [CommandMethod("SDRESETFLOAT")]
        public void ResetFloat()
        {
            PaletteHost.RecreateFloating(newGuid: true);
        }

        [CommandMethod("SDDRAWSETBACK")]
        public void DrawSetback()
        {
            var ed = AcadApp.DocumentManager.MdiActiveDocument.Editor;
            try
            {
                SetbackService.DrawUniformSetback(AppState.SiteBoundaryId, AppState.Config);
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage($"\nSDDRAWSETBACK error: {ex.GetType().Name}: {ex.Message}");
            }
        }

        [CommandMethod("SDDIAG")]
        public void Diagnostics()
        {
            var ed = AcadApp.DocumentManager.MdiActiveDocument.Editor;
            
            try
            {
                ed.WriteMessage("\n=== AutoCAD Palette Diagnostics ===");
                
                // System variables
                ed.WriteMessage($"\nLOCKUI: {AcadApp.GetSystemVariable("LOCKUI")}");
                ed.WriteMessage($"\nWorkspace: {AcadApp.GetSystemVariable("WORKSPACENAME")}");
                
                // AutoCAD version info
                var version = AcadApp.Version;
                ed.WriteMessage($"\nAutoCAD Version: {version.Major}.{version.Minor}.{version.Build}.{version.Revision}");
                
                // Screen/DPI info
                var screen = System.Windows.Forms.Screen.PrimaryScreen;
                ed.WriteMessage($"\nPrimary Screen: {screen.Bounds}");
                
                // Palette information
                var ps = PaletteHost.GetPaletteSet();
                if (ps != null)
                {
                    ed.WriteMessage($"\nPalette Exists: True");
                    ed.WriteMessage($"\nPalette Visible: {ps.Visible}");
                    ed.WriteMessage($"\nPalette Dock: {ps.Dock}");
                    ed.WriteMessage($"\nPalette Size: {ps.Size.Width}x{ps.Size.Height}");
                    ed.WriteMessage($"\nPalette Location: {ps.Location.X},{ps.Location.Y}");
                    ed.WriteMessage($"\nPalette MinSize: {ps.MinimumSize.Width}x{ps.MinimumSize.Height}");
                    ed.WriteMessage($"\nPalette Style: {ps.Style}");
                    ed.WriteMessage($"\nPalette KeepFocus: {ps.KeepFocus}");
                    ed.WriteMessage($"\nPalette DockEnabled: {ps.DockEnabled}");
                }
                else
                {
                    ed.WriteMessage("\nPalette Exists: False");
                }
                
                // ElementHost info
                var hostInfo = PaletteHost.GetElementHostInfo();
                if (!string.IsNullOrEmpty(hostInfo))
                {
                    ed.WriteMessage($"\nElementHost Info: {hostInfo}");
                }
                
                ed.WriteMessage("\n=== End Diagnostics ===");
                ed.WriteMessage("\nPress F2 to see full output in Text Window");
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage($"\nDiagnostic error: {ex.Message}");
            }
        }

        [CommandMethod("SDALTPALETTE")]
        public void AlternativePalette()
        {
            var ed = AcadApp.DocumentManager.MdiActiveDocument.Editor;
            try
            {
                PaletteHost.CreateAlternativePalette();
                ed.WriteMessage("\nSDALTPALETTE: Alternative palette creation attempted.");
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage($"\nSDALTPALETTE error: {ex.GetType().Name}: {ex.Message}");
            }
        }
    }
}
