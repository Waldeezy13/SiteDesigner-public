using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace SiteDesigner.Plugin
{
    public static class GeometryUtil
    {
        /// <summary>
        /// Determines if a point is inside a closed polyline using ray casting algorithm
        /// </summary>
        /// <param name="pl">Closed polyline</param>
        /// <param name="pt">Point to test</param>
        /// <returns>True if point is inside polyline</returns>
        public static bool IsPointInPolyline(Polyline pl, Point3d pt)
        {
            if (!pl.Closed) return false;

            int intersections = 0;
            var testPt = new Point2d(pt.X, pt.Y);
            
            // Cast ray to the right from test point
            for (int i = 0; i < pl.NumberOfVertices; i++)
            {
                var p1 = pl.GetPoint2dAt(i);
                var p2 = pl.GetPoint2dAt((i + 1) % pl.NumberOfVertices);

                // Check if ray intersects this edge
                if (RayIntersectsSegment(testPt, p1, p2))
                {
                    intersections++;
                }
            }

            // Odd number of intersections means inside
            return (intersections % 2) == 1;
        }

        /// <summary>
        /// Calculates signed area of a closed polyline using shoelace formula
        /// Positive area = counter-clockwise, negative = clockwise
        /// </summary>
        /// <param name="pl">Closed polyline</param>
        /// <returns>Signed area</returns>
        public static double SignedArea(Polyline pl)
        {
            if (!pl.Closed) return 0;

            double area = 0;
            for (int i = 0; i < pl.NumberOfVertices; i++)
            {
                var p1 = pl.GetPoint2dAt(i);
                var p2 = pl.GetPoint2dAt((i + 1) % pl.NumberOfVertices);
                area += (p1.X * p2.Y) - (p2.X * p1.Y);
            }
            return area / 2.0;
        }

        /// <summary>
        /// Intelligently chooses the inward offset from a polyline
        /// </summary>
        /// <param name="pl">Original polyline</param>
        /// <param name="distance">Offset distance (positive)</param>
        /// <param name="insidePoint">Optional point known to be inside the polyline</param>
        /// <returns>Inward offset polyline or null if no valid offset found</returns>
        public static Polyline? ChooseInwardOffset(Polyline pl, double distance, Point3d? insidePoint)
        {
            return ChooseOffset(pl, distance, insidePoint, preferInward: true);
        }

        /// <summary>
        /// Chooses the outward offset from a polyline
        /// </summary>
        /// <param name="pl">Original polyline</param>
        /// <param name="distance">Offset distance (positive)</param>
        /// <param name="insidePoint">Optional point known to be inside the polyline</param>
        /// <returns>Outward offset polyline or null if no valid offset found</returns>
        public static Polyline? ChooseOutwardOffset(Polyline pl, double distance, Point3d? insidePoint)
        {
            return ChooseOffset(pl, distance, insidePoint, preferInward: false);
        }

        /// <summary>
        /// Chooses offset direction based on preference
        /// </summary>
        /// <param name="pl">Original polyline</param>
        /// <param name="distance">Offset distance (positive)</param>
        /// <param name="insidePoint">Optional point known to be inside the polyline</param>
        /// <param name="preferInward">True for inward, false for outward</param>
        /// <returns>Offset polyline or null if no valid offset found</returns>
        private static Polyline? ChooseOffset(Polyline pl, double distance, Point3d? insidePoint, bool preferInward)
        {
            if (!pl.Closed || distance <= 0) return null;

            try
            {
                // Get both offset candidates
                var positiveCurves = pl.GetOffsetCurves(distance);
                var negativeCurves = pl.GetOffsetCurves(-distance);

                Polyline? positiveCandidate = null;
                Polyline? negativeCandidate = null;

                // Extract first polyline from each set
                foreach (var obj in positiveCurves)
                {
                    if (obj is Polyline pPl && pPl.Closed)
                    {
                        positiveCandidate = pPl;
                        break;
                    }
                    else if (obj is System.IDisposable d)
                    {
                        d.Dispose();
                    }
                }

                foreach (var obj in negativeCurves)
                {
                    if (obj is Polyline nPl && nPl.Closed)
                    {
                        negativeCandidate = nPl;
                        break;
                    }
                    else if (obj is System.IDisposable d)
                    {
                        d.Dispose();
                    }
                }

                // Clean up remaining objects
                foreach (var obj in positiveCurves)
                {
                    if (obj != positiveCandidate && obj is System.IDisposable d)
                        d.Dispose();
                }
                foreach (var obj in negativeCurves)
                {
                    if (obj != negativeCandidate && obj is System.IDisposable d)
                        d.Dispose();
                }

                // If only one candidate exists, use it
                if (positiveCandidate == null && negativeCandidate != null) return negativeCandidate;
                if (negativeCandidate == null && positiveCandidate != null) return positiveCandidate;
                if (positiveCandidate == null && negativeCandidate == null) return null;

                // Both candidates exist - choose based on preference
                var originalArea = System.Math.Abs(SignedArea(pl));
                var positiveArea = System.Math.Abs(SignedArea(positiveCandidate!));
                var negativeArea = System.Math.Abs(SignedArea(negativeCandidate!));

                // Determine which is inward (smaller area) and which is outward (larger area)
                bool positiveIsInward = positiveArea < originalArea;
                bool negativeIsInward = negativeArea < originalArea;

                Polyline? preferredCandidate = null;
                Polyline? otherCandidate = null;

                if (preferInward)
                {
                    // Want inward offset
                    if (positiveIsInward && !negativeIsInward)
                    {
                        preferredCandidate = positiveCandidate;
                        otherCandidate = negativeCandidate;
                    }
                    else if (negativeIsInward && !positiveIsInward)
                    {
                        preferredCandidate = negativeCandidate;
                        otherCandidate = positiveCandidate;
                    }
                }
                else
                {
                    // Want outward offset
                    if (!positiveIsInward && negativeIsInward)
                    {
                        preferredCandidate = positiveCandidate;
                        otherCandidate = negativeCandidate;
                    }
                    else if (!negativeIsInward && positiveIsInward)
                    {
                        preferredCandidate = negativeCandidate;
                        otherCandidate = positiveCandidate;
                    }
                }

                if (preferredCandidate != null)
                {
                    otherCandidate?.Dispose();
                    return preferredCandidate;
                }

                // If both are inward/outward or neither, use inside point to break tie
                if (insidePoint.HasValue)
                {
                    bool positiveContainsInside = IsPointInPolyline(positiveCandidate, insidePoint.Value);
                    bool negativeContainsInside = IsPointInPolyline(negativeCandidate, insidePoint.Value);

                    if (preferInward)
                    {
                        // For inward, prefer the one that still contains the inside point
                        if (positiveContainsInside && !negativeContainsInside)
                        {
                            negativeCandidate.Dispose();
                            return positiveCandidate;
                        }
                        if (negativeContainsInside && !positiveContainsInside)
                        {
                            positiveCandidate.Dispose();
                            return negativeCandidate;
                        }
                    }
                    else
                    {
                        // For outward, prefer the one that does NOT contain the inside point
                        if (!positiveContainsInside && negativeContainsInside)
                        {
                            negativeCandidate.Dispose();
                            return positiveCandidate;
                        }
                        if (!negativeContainsInside && positiveContainsInside)
                        {
                            positiveCandidate.Dispose();
                            return negativeCandidate;
                        }
                    }
                }

                // Final fallback: choose based on area preference
                if (preferInward)
                {
                    // Choose smaller area
                    if (positiveArea <= negativeArea)
                    {
                        negativeCandidate.Dispose();
                        return positiveCandidate;
                    }
                    else
                    {
                        positiveCandidate.Dispose();
                        return negativeCandidate;
                    }
                }
                else
                {
                    // Choose larger area
                    if (positiveArea >= negativeArea)
                    {
                        negativeCandidate.Dispose();
                        return positiveCandidate;
                    }
                    else
                    {
                        positiveCandidate.Dispose();
                        return negativeCandidate;
                    }
                }
            }
            catch (System.Exception)
            {
                // If offset operation fails, return null
                return null;
            }
        }

        /// <summary>
        /// Helper method for ray casting - checks if a horizontal ray intersects a line segment
        /// </summary>
        private static bool RayIntersectsSegment(Point2d rayStart, Point2d segStart, Point2d segEnd)
        {
            // Ensure segStart.Y <= segEnd.Y
            if (segStart.Y > segEnd.Y)
            {
                (segStart, segEnd) = (segEnd, segStart);
            }

            // Ray must be within Y bounds of segment
            if (rayStart.Y < segStart.Y || rayStart.Y >= segEnd.Y) return false;

            // Check if intersection point is to the right of ray start
            if (System.Math.Abs(segEnd.Y - segStart.Y) < 1e-10) return false; // Horizontal segment

            double intersectionX = segStart.X + (rayStart.Y - segStart.Y) * (segEnd.X - segStart.X) / (segEnd.Y - segStart.Y);
            return intersectionX > rayStart.X;
        }
    }
}