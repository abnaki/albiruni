using System;
using System.Collections.Generic;
using System.Linq;
using System.Dynamic;

using ImpromptuInterface;
using ImpromptuInterface.Dynamic;
using global::Geo.Gps.Serialization.Xml.Gpx.Gpx10;
using GeoGpxFile = global::Geo.Gps.Serialization.Xml.Gpx.Gpx10.GpxFile;
using System.Diagnostics;

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
                Tracks: GetTracks(gfile.trk),
                Routes: GetRoutes(gfile.rte)
                );

            return Impromptu.ActLike<IFile>(temp);
        }

        //static IPoint PointFromTrackPoint(GpxTrackPoint pt)
        //{
        //    // note GpxTrackPoint also has course and speed
        //    return PointDuck.PointFromGpx(pt);
        //}

        static IEnumerable<IPoint> PointsFromWaypoints(IEnumerable<GpxPoint> points)
        {
            if (points == null)
                return Enumerable.Empty<IPoint>();

            return points.Select(w => PointDuck.PointFromGpx(w, requireElevation: true)).ToArray();
        }

        static IEnumerable<ITrack> GetTracks(IEnumerable<GpxTrack> tracks)
        {
            if (tracks == null)
                return Enumerable.Empty<ITrack>();

            return tracks.Where(trk => trk != null && trk.trkseg != null).Select(TrackFromGpx).ToArray();
        }

        static ITrack TrackFromGpx(GpxTrack trk)
        {
            IEnumerable<GpxTrackPoint> trackPoints = trk.trkseg
                .Where(seg => seg.trkpt != null)
                .SelectMany(seg => seg.trkpt)
                .Where(p => p != null).ToArray();

            //PointDuck.CountSuspiciousTimes(trackPoints, trk);

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
                .Select(p => PointDuck.PointFromGpx(p, requireElevation: true))
                .ToArray()
                );

            return Impromptu.ActLike<IRoute>(temp);
        }
    }
}
