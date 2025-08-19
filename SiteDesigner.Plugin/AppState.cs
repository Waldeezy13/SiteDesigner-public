using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using SiteDesigner.Core;

namespace SiteDesigner.Plugin
{
    public static class AppState
    {
        public static ObjectId SiteBoundaryId { get; set; }
        public static Point3d? InsidePoint { get; set; }
        public static bool LastOffsetInside { get; set; } = true; // Default to inside

        public static SiteConfig Config { get; } = new SiteConfig();
    }
}
