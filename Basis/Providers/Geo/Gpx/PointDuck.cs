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
        public static IPoint PointFromGpx(GpxWaypointBase wpt)
        {
            dynamic temp = Build<ExpandoObject>.NewObject(
                Latitude: wpt.lat,
                Longitude: wpt.lon,
                Time: DateTimeOfWpt(wpt),
                TimeReliable: wpt.timeSpecified && wpt.eleSpecified,
                Elevation: wpt.eleSpecified ? (decimal?)wpt.ele : (decimal?)null);

            return Impromptu.ActLike<IPoint>(temp); // duck typing
        }

        static DateTime? DateTimeOfWpt(GpxWaypointBase wpt)
        {
            if (wpt.timeSpecified)
            {
                DateTime dt = new DateTime(wpt.time.Ticks, DateTimeKind.Utc);  // undesired DateTimeKind.Unspecified from xml serializer

                if (wpt.eleSpecified) // consistent with actually visiting the point.
                {
                    return dt;
                }
                else
                {
                    // time is unreliable if not measured by a gps.
                    // It has been an unwanted output of desktop software, 
                    //  or waypoints created by browsing a map on GPSr.
                    if (false == string.IsNullOrWhiteSpace(wpt.name))
                    {
                        Debug.WriteLine("Disregarding non-gps time of point " + wpt.name + " " + dt);
                    }
                }
            }


            return null;
        }

        public static int CountSuspiciousTimes(IEnumerable<GpxWaypointBase> waypoints)
        {
            return waypoints.Count(wpt => wpt.timeSpecified && !wpt.eleSpecified);
        }
    }
}
