using System;
using System.Collections.Generic;
using System.Linq;
using System.Dynamic;

using ImpromptuInterface;
using ImpromptuInterface.Dynamic;
using global::Geo.Gps.Serialization.Xml.Gpx.Gpx10;
using GeoGpxFile = global::Geo.Gps.Serialization.Xml.Gpx.Gpx10.GpxFile;

namespace Abnaki.Albiruni.Providers.Geo.Gpx.V10
{
    /// <summary>
    /// To provide IFile from Geo.Gps.Serialization.Xml.Gpx.Gpx10.GpxFile
    /// </summary>
    class FileDuck
    {

        public static IFile FileFromGpx(GeoGpxFile gfile)
        {
            dynamic temp = Build<ExpandoObject>.NewObject(
                WayPoints: PointsFromWaypoints(gfile.wpt),
                TrackPoints: PointsFromTracks(gfile.trk),
                RoutePoints: PointsFromRoutes(gfile.rte)
                );

            return Impromptu.ActLike<IFile>(temp);
        }

        static IPoint PointFromGpx(GpxPoint wpt)
        {
            // GeniusCode.Components.ProxyFactory.DuckInterface<IPoint>(wpt, m_pointProvider); loaded a new damn assembly every time
            //return ProxyFactory.DuckInterface<IPoint>(wpt, m_pointProvider);

            return PointDuck.PointFromGpx(wpt);
        }

        static IPoint PointFromTrackPoint(GpxTrackPoint pt)
        {
            // note GpxTrackPoint also has course and speed
            return PointFromGpx(pt);
        }

        static IEnumerable<IPoint> PointsFromWaypoints(IEnumerable<GpxPoint> points)
        {
            if (points == null)
                return Enumerable.Empty<IPoint>();

            return points.Select(PointFromGpx).ToArray();
        }

        static IEnumerable<IPoint> PointsFromTracks(IEnumerable<GpxTrack> tracks)
        {
            if (tracks == null)
                return Enumerable.Empty<IPoint>();

            IEnumerable<GpxTrackSegment> segs = tracks.Where(t => t.trkseg != null)
                .SelectMany(t => t.trkseg)
                .Where(seg => seg != null);

            return segs.Where(seg => seg.trkpt != null)
                .SelectMany(seg => seg.trkpt)
                .Where(p => p != null)
                .Select(PointFromTrackPoint).ToArray();
        }

        static IEnumerable<IPoint> PointsFromRoutes(IEnumerable<GpxRoute> routes)
        {
            if (routes == null)
                return Enumerable.Empty<IPoint>();

            return routes.Where(r => r.rtept != null)
                .SelectMany(r => r.rtept)
                .Where(p => p != null)
                .Select(PointFromGpx).ToArray();
        }
    }
}
