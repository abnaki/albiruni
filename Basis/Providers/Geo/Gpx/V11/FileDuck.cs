using System;
using System.Collections.Generic;
using System.Linq;
using System.Dynamic;

using ImpromptuInterface;
using ImpromptuInterface.Dynamic;
using global::Geo.Gps.Serialization.Xml.Gpx.Gpx11;
using GeoGpxFile = global::Geo.Gps.Serialization.Xml.Gpx.Gpx11.GpxFile;

namespace Abnaki.Albiruni.Providers.Geo.Gpx.V11
{
    /// <summary>
    /// To provide IFile from Geo.Gps.Serialization.Xml.Gpx.Gpx11.GpxFile
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

        static IEnumerable<IPoint> PointsFromWaypoints(IEnumerable<GpxWaypoint> points)
        {
            if (points == null)
                return Enumerable.Empty<IPoint>();

            return points.Select(PointFromGpx);
        }

        static IPoint PointFromGpx(GpxWaypoint wpt)
        {
            //return ProxyFactory.DuckInterface<IPoint>(wpt, m_pointProvider);

            return PointDuck.PointFromGpx(wpt);
        }

        static IEnumerable<IPoint> PointsFromTracks(IEnumerable<GpxTrack> tracks)
        {
            if (tracks == null)
                return Enumerable.Empty<IPoint>();

            IEnumerable<GpxTrackSegment> segs = tracks.SelectMany(t => t.trkseg);
            return segs.SelectMany(seg => seg.trkpt).Select(PointFromGpx);
        }

        static IEnumerable<IPoint> PointsFromRoutes(IEnumerable<GpxRoute> routes)
        {
            if (routes == null)
                return Enumerable.Empty<IPoint>();

            return routes.SelectMany(r => r.rtept).Select(PointFromGpx);
        }
    }
}
