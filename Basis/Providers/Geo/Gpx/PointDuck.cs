using System;
using System.Collections.Generic;
using System.Linq;
using System.Dynamic;

using ImpromptuInterface;
using ImpromptuInterface.Dynamic;
using Geo.Gps.Serialization.Xml.Gpx;
using System.Diagnostics;

namespace Abnaki.Albiruni.Providers.Geo.Gpx
{
    /// <summary>
    /// Adapts Geo.Gps.Serialization.Xml.Gpx.Gpx11.GpxWaypoint to Albiruni IPoint.
    /// </summary>
    /// <remarks>
    /// Geo.Geometries.Point does not have time, name...
    /// Such fields are in Geo.Gps.Serialization.Xml.Gpx.GpxWaypointBase 
    ///  (has subtypes such as Geo.Gps.Serialization.Xml.Gpx.Gpx11.GpxWaypoint)
    /// Geo/Geo/Gps/Serialization/Gpx11Serializer.cs  creates Points from GpxWaypoint, dropping other detail.
    /// </remarks>
    class PointDuck
    {
        /// <summary>
        /// Conversion
        /// </summary>
        /// <param name="wpt"></param>
        /// <param name="requireElevation">
        ///   true requires consistency with actually visiting a waypoint;
        ///   but may be false for tracks as they may have less frequent elevation recordings;
        ///   software is likely to create waypoints with fictitious times, but not tracks.
        /// </param>
        public static IPoint PointFromGpx(GpxWaypointBase wpt, bool requireElevation)
        {
            dynamic temp = Build<ExpandoObject>.NewObject(
                Latitude: wpt.lat,
                Longitude: wpt.lon,
                Time: DateTimeOfWpt(wpt, requireElevation),
                TimeReliable: wpt.timeSpecified && wpt.eleSpecified,
                Elevation: wpt.eleSpecified ? (decimal?)wpt.ele : (decimal?)null);

            return Impromptu.ActLike<IPoint>(temp); // duck typing
        }

        static DateTime? DateTimeOfWpt(GpxWaypointBase wpt, bool requireElevation)
        {
            if (wpt.timeSpecified)
            {
                DateTime dt = new DateTime(wpt.time.Ticks, DateTimeKind.Utc);  // undesired DateTimeKind.Unspecified from xml serializer

                if (wpt.eleSpecified || !requireElevation)
                {
                    return dt;
                }
                else if (requireElevation)
                {
                    // conclude that it is not measured by a gps;
                    // that implies time is unreliable;
                    // It was an unwanted output of desktop software, 
                    //  or waypoints created by browsing a map on GPSr.
                    if (false == string.IsNullOrWhiteSpace(wpt.name))
                    {
                        Debug.WriteLine("Disregarding non-gps time of point " + wpt.name + " " + dt);
                    }
                }
            }


            return null;
        }

        //public static int CountSuspiciousTimes(IEnumerable<GpxWaypointBase> waypoints, object track)
        //{
        //    int nreport = waypoints.Count(wpt => wpt.timeSpecified && !wpt.eleSpecified);
        //    if (nreport > 0)
        //    {
        //        Debug.WriteLine("Point(s) of " + track.GetType().Name + " having suspicious times = " + nreport);
        //    }
        //    return nreport;
        //}
    }
}
