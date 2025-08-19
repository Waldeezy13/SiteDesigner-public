using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using SiteDesigner.Core;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace SiteDesigner.Plugin
{
    public static class LayoutService
    {
        public static void PlaceTestLayout(ObjectId boundaryId, SiteConfig cfg)
        {
            var doc = AcadApp.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            var ed = doc.Editor;

            if (boundaryId == ObjectId.Null)
            {
                ed.WriteMessage("\nNo site boundary stored. Run SDSITESETUP first.");
                return;
            }

            // IMPORTANT: palette actions run outside a command => lock the doc
            using (doc.LockDocument())
            {
                // Calculate inward offset distance using the smallest setback
                var inward = System.Math.Max(0.1, System.Math.Min(cfg.SetbackFront,
                                System.Math.Min(cfg.SetbackSide, cfg.SetbackRear)));

                // Prompt for offset direction with keyword options
                var offsetInside = AppState.LastOffsetInside; // Default to last choice

                var o = new PromptKeywordOptions("\nOffset direction")
                {
                    AllowNone = true
                };
                o.Keywords.Add("Inside");
                o.Keywords.Add("Outside");
                o.Keywords.Default = offsetInside ? "Inside" : "Outside";

                var keywordResult = ed.GetKeywords(o);
                if (keywordResult.Status == PromptStatus.OK)
                {
                    offsetInside = keywordResult.StringResult.Equals("Inside", System.StringComparison.OrdinalIgnoreCase);
                    AppState.LastOffsetInside = offsetInside; // Remember user's choice
                }
                else if (keywordResult.Status != PromptStatus.None)
                {
                    // User cancelled or error - abort operation
                    return;
                }
                // If PromptStatus.None (just pressed Enter), use the default

                using var tr = db.TransactionManager.StartTransaction();
                var ent = (Entity)tr.GetObject(boundaryId, OpenMode.ForRead);
                if (ent is not Polyline pl || !pl.Closed)
                {
                    ed.WriteMessage("\nStored entity is not a closed polyline.");
                    return;
                }

                // Use appropriate offset selection based on user choice
                Polyline? offsetPolyline;
                string directionText;
                if (offsetInside)
                {
                    offsetPolyline = GeometryUtil.ChooseInwardOffset(pl, inward, AppState.InsidePoint);
                    directionText = "inward";
                }
                else
                {
                    offsetPolyline = GeometryUtil.ChooseOutwardOffset(pl, inward, AppState.InsidePoint);
                    directionText = "outward";
                }
                
                if (offsetPolyline == null)
                {
                    ed.WriteMessage($"\nCould not create {directionText} offset. Check polyline geometry and offset distance.");
                    return;
                }

                // Add the offset polyline to Model Space
                var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                ms.AppendEntity(offsetPolyline);
                tr.AddNewlyCreatedDBObject(offsetPolyline, true);

                tr.Commit();
                
                var detectionInfo = AppState.InsidePoint.HasValue ? "using inside point" : "using area-based detection";
                ed.WriteMessage($"\nPlaced {directionText} offset ~{inward:0.##} ft as a test layout ({detectionInfo}).");
            }
        }
    }
}
