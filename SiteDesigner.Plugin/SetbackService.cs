using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Colors;
using SiteDesigner.Core;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace SiteDesigner.Plugin
{
    public static class SetbackService
    {
        /// <summary>
        /// Draws a uniform setback polygon inside the boundary using minimum setback distance
        /// </summary>
        /// <param name="boundaryId">Site boundary polyline ObjectId</param>
        /// <param name="cfg">Site configuration with setback values</param>
        public static void DrawUniformSetback(ObjectId boundaryId, SiteConfig cfg)
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
                // Calculate minimum setback distance
                var dist = System.Math.Max(0.1, System.Math.Min(cfg.SetbackFront,
                                System.Math.Min(cfg.SetbackSide, cfg.SetbackRear)));

                using var tr = db.TransactionManager.StartTransaction();
                
                var ent = (Entity)tr.GetObject(boundaryId, OpenMode.ForRead);
                if (ent is not Polyline pl || !pl.Closed)
                {
                    ed.WriteMessage("\nStored entity is not a closed polyline.");
                    return;
                }

                // Create inward offset using our intelligent chooser or fallback
                Polyline? setbackPolyline = null;
                
                try
                {
                    // Try our intelligent inward chooser first
                    setbackPolyline = GeometryUtil.ChooseInwardOffset(pl, dist, AppState.InsidePoint);
                }
                catch
                {
                    // Fallback to basic offset if GeometryUtil fails
                    try
                    {
                        var curves = pl.GetOffsetCurves(-dist);
                        foreach (var obj in curves)
                        {
                            if (obj is Polyline offsetPl && offsetPl.Closed)
                            {
                                setbackPolyline = offsetPl;
                                break;
                            }
                            else if (obj is System.IDisposable d)
                            {
                                d.Dispose();
                            }
                        }
                        // Clean up remaining curves
                        foreach (var obj in curves)
                        {
                            if (obj != setbackPolyline && obj is System.IDisposable d)
                                d.Dispose();
                        }
                    }
                    catch
                    {
                        // If both methods fail, report error
                        ed.WriteMessage("\nCould not create setback offset. Check polyline geometry and setback distance.");
                        return;
                    }
                }

                if (setbackPolyline == null)
                {
                    ed.WriteMessage("\nCould not create setback offset. Check polyline geometry and setback distance.");
                    return;
                }

                // Create or get the setback layer
                var layerId = CreateOrGetSetbackLayer(tr, db);
                
                // Set the polyline properties
                setbackPolyline.LayerId = layerId;

                // Add to Model Space
                var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                ms.AppendEntity(setbackPolyline);
                tr.AddNewlyCreatedDBObject(setbackPolyline, true);

                tr.Commit();
                
                ed.WriteMessage($"\nSetback polygon created on layer C-SITE-SETBACK with {dist:0.##} ft offset.");
            }
        }

        /// <summary>
        /// Creates or gets the setback layer with proper properties
        /// </summary>
        private static ObjectId CreateOrGetSetbackLayer(Transaction tr, Database db)
        {
            const string layerName = "C-SITE-SETBACK";
            
            var layerTable = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
            
            if (layerTable.Has(layerName))
            {
                // Layer exists, return its ObjectId
                return layerTable[layerName];
            }

            // Create new layer
            layerTable.UpgradeOpen();
            
            var layerRecord = new LayerTableRecord
            {
                Name = layerName,
                Color = Autodesk.AutoCAD.Colors.Color.FromColorIndex(ColorMethod.ByAci, 3) // Green
            };

            // Try to set DASHED linetype, fallback to CONTINUOUS
            var linetypeTable = (LinetypeTable)tr.GetObject(db.LinetypeTableId, OpenMode.ForRead);
            if (linetypeTable.Has("DASHED"))
            {
                layerRecord.LinetypeObjectId = linetypeTable["DASHED"];
            }
            else if (linetypeTable.Has("CONTINUOUS"))
            {
                layerRecord.LinetypeObjectId = linetypeTable["CONTINUOUS"];
            }
            // If neither exists, AutoCAD will use default

            var layerId = layerTable.Add(layerRecord);
            tr.AddNewlyCreatedDBObject(layerRecord, true);
            
            return layerId;
        }
    }
}