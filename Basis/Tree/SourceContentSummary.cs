using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using Geo.Abstractions.Interfaces;

namespace Abnaki.Albiruni.Tree
{
    public class SourceContentSummary
    {
        public SourceContentSummary()
        {

        }

        public SourceContentSummary(IEnumerable<IPosition> positions)
        {
            Points = positions.Count();

            IEnumerable<Geo.Coordinate> coordinates = positions.Select(p => p.GetCoordinate());

            if ( Points > 0 )
            {
                MinLatitude = coordinates.Min(c => c.Latitude);
                MaxLatitude = coordinates.Max(c => c.Latitude);
                MinLongitude = coordinates.Min(c => c.Longitude);
                MaxLongitude = coordinates.Max(c => c.Longitude);
            }
        }

        public int Points { get; private set; }
        //public int Waypoints { get; set; }
        //public int Trackpoints { get; set; }

        public double? MinLatitude { get; private set; }
        public double? MaxLatitude { get; private set; }
        public double? MinLongitude { get; private set; }
        public double? MaxLongitude { get; private set; }

        public DateTime? MinDate { get; set; }
        public DateTime? MaxDate { get; set; }

        public double? MinElevation { get; set; }
        public double? MaxElevation { get; set; }

        public double? MinSpeed { get; set; }
        public double? MaxSpeed { get; set; }

        public override string ToString()
        {
            return string.Format("{0} Points, Lat [{1},{2}], Long [{3},{4}]", Points, MinLatitude, MaxLatitude, MinLongitude, MaxLongitude);
        }

        const int filever = 2;

        public void Write(BinaryWriter bw)
        {
            bw.Write(filever);

            bw.Write(Points);
            if ( Points > 0 )
            {
                bw.Write(MinLatitude.Value);
                bw.Write(MaxLatitude.Value);
                bw.Write(MinLongitude.Value);
                bw.Write(MaxLongitude.Value);
            }
        }

        public void Read(BinaryReader br)
        {
            int v = br.ReadInt32();

            if (v < 2)
                throw new NotSupportedException("Old file version");

            Points = br.ReadInt32();
            if ( Points > 0 )
            {
                MinLatitude = br.ReadDouble();
                MaxLatitude = br.ReadDouble();
                MinLongitude = br.ReadDouble();
                MaxLongitude = br.ReadDouble();
            }
        }

        public void AggregateWith(SourceContentSummary subset)
        {
            this.Points += subset.Points;

            this.MinLatitude = NullableExtreme(this.MinLatitude, subset.MinLatitude, -1);
            this.MaxLatitude = NullableExtreme(this.MaxLatitude, subset.MaxLatitude, 1);
            this.MinLongitude = NullableExtreme(this.MinLongitude, subset.MinLongitude, -1);
            this.MaxLongitude = NullableExtreme(this.MaxLongitude, subset.MaxLongitude, 1);
        }

        /// <summary>
        /// Of the non-null args, return the extreme
        /// </summary>
        /// <param name="sign">1 implies max, -1 implies min
        /// </param>
        static double? NullableExtreme(double? a, double? b, int sign)
        {
            if ( a.HasValue )
            {
                if ( b.HasValue )
                {
                    if (b.Value.CompareTo(a.Value) == sign)
                        return b;
                }
                return a;
            }
            else if ( b.HasValue )
            {
                return b;
            }
            return null;
        }
    }
}
