using System;
using System.Collections.Generic;
using System.Linq;

using MapControl;

namespace Abnaki.Albiruni.Graphic
{
    public static class MapExtensions
    {
        public static string ToStringUseful(this MapRectangle r)
        {
            return "W=" + r.West + ", E=" + r.East + ", S=" + r.South + ", N=" + r.North;
        }

        /// <summary>
        /// Truncates inputs to proper bounds
        /// </summary>
        public static MapRectangle NewMapRectangle(double west, double east, double south, double north)
        {
            double minLong = -180, maxLong = 180, minLat = -90, maxLat = 90;
            return new MapRectangle()
            {
                West = Bounded(minLong, west, maxLong), 
                East = Bounded(minLong, east, maxLong),
                South = Bounded(minLat, south, maxLat),
                North = Bounded(minLat, north, maxLat)
            };
        }

        static double Bounded(double min, double x, double max)
        {
            return Math.Max(min, Math.Min(x, max));
        }

        static bool EqualDouble(double x, double y)
        {
            return Math.Abs(x - y) < double.Epsilon;
        }

        /// <summary>Equality of 4 coordinates
        /// </summary>
        /// <remarks>Not the same as MapRectangle rect.Equals(other)
        /// </remarks>
        public static bool EqualCoordinates(this MapRectangle rect, MapRectangle other)
        {
            return EqualDouble(rect.West, other.West)
                && EqualDouble(rect.East, other.East)
                && EqualDouble(rect.South, other.South)
                && EqualDouble(rect.North, other.North);
        }

        public static bool EqualCoordinates(this Location loc, Location other)
        {
            if (other == null)
                return false;

            return EqualDouble(loc.Latitude, other.Latitude) && EqualDouble(loc.Longitude, other.Longitude);
        }
    }
}
