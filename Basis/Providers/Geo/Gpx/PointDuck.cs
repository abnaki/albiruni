using System;
using System.Collections.Generic;
using System.Linq;
using System.Dynamic;

using ImpromptuInterface;
using ImpromptuInterface.Dynamic;
using Geo.Gps.Serialization.Xml.Gpx;

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
                Time: wpt.timeSpecified ? (DateTime?)wpt.time : (DateTime?)null,
                Elevation: wpt.eleSpecified ? (decimal?)wpt.ele : (decimal?)null);

            return Impromptu.ActLike<IPoint>(temp); // duck typing
        }

    }
}
