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
                Tracks: GetTracks(gfile.trk),
                Routes: GetRoutes(gfile.rte)
                );

            return Impromptu.ActLike<IFile>(temp);
        }

        static IEnumerable<IPoint> PointsFromWaypoints(IEnumerable<GpxWaypoint> points)
        {
            if (points == null)
                return Enumerable.Empty<IPoint>();

            return points.Select(p => PointDuck.PointFromGpx(p, requireElevation: true)).ToArray();
        }

        static IEnumerable<ITrack> GetTracks(IEnumerable<GpxTrack> tracks)
        {
            if (tracks == null)
                return Enumerable.Empty<ITrack>();

            return tracks.Where(trk => trk != null && trk.trkseg != null).Select(TrackFromGpx).ToArray();
        }

        static ITrack TrackFromGpx(GpxTrack trk)
        {
            IEnumerable<GpxWaypoint> trackPoints = trk.trkseg
                .Where(seg => seg.trkpt != null)
                .SelectMany(seg => seg.trkpt)
                .Where(p => p != null).ToArray();

            //int nreport = PointDuck.CountSuspiciousTimes(trackPoints, trk);

            dynamic temp = Build<ExpandoObject>.NewObject(
                Points: trackPoints.Select(p => PointDuck.PointFromGpx(p, requireElevation: false))
                .ToArray()
                );

            return Impromptu.ActLike<ITrack>(temp);
        }

        static IEnumerable<IRoute> GetRoutes(IEnumerable<GpxRoute> routes)
        {
            if (routes == null)
                return Enumerable.Empty<IRoute>();

            return routes.Where(r => r.rtept != null).Select(RouteFromGpx).ToArray();
        }

        static IRoute RouteFromGpx(GpxRoute gr)
        {
            dynamic temp = Build<ExpandoObject>.NewObject(
                Points: gr.rtept.Where(p => p != null)
                .Select(p => PointDuck.PointFromGpx(p, requireElevation: true)).ToArray()
                );

            return Impromptu.ActLike<IRoute>(temp);
        }

    }
}
