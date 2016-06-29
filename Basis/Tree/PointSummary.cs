using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using Abnaki.Albiruni.Providers;

namespace Abnaki.Albiruni.Tree
{
    /// <summary>
    /// Aggregate statistics on points of a single category
    /// </summary>
    public class PointSummary
    {
        public PointSummary()
        {
        }

        public PointSummary(IEnumerable<IPoint> points)
        {
            Points = points.Count();

            if (Points > 0)
            {
                MinLatitude = points.Min(c => c.Latitude);
                MaxLatitude = points.Max(c => c.Latitude);
                MinLongitude = points.Min(c => c.Longitude);
                MaxLongitude = points.Max(c => c.Longitude);

                MinTime = points.Aggregate<IPoint, DateTime?>(null, MinNullableTime);
                MaxTime = points.Aggregate<IPoint, DateTime?>(null, MaxNullableTime);
                //etc.
            }
        }

        public int Points { get; private set; }

        public decimal? MinLatitude { get; private set; }
        public decimal? MaxLatitude { get; private set; }
        public decimal? MinLongitude { get; private set; }
        public decimal? MaxLongitude { get; private set; }

        public DateTime? MinTime { get; set; }
        public DateTime? MaxTime { get; set; }

        public decimal? MinElevation { get; set; }
        public decimal? MaxElevation { get; set; }

        public decimal? MinSpeed { get; set; }
        public decimal? MaxSpeed { get; set; }

        const int filever = 2;

        public void Write(BinaryWriter bw)
        {
            bw.Write(filever);

            bw.Write(Points);
            if (Points > 0)
            {
                bw.Write(MinLatitude.Value);
                bw.Write(MaxLatitude.Value);
                bw.Write(MinLongitude.Value);
                bw.Write(MaxLongitude.Value);
            }

            WriteNullable(MinTime, bw);
            WriteNullable(MaxTime, bw);
            WriteNullable(MinElevation, bw);
            WriteNullable(MaxElevation, bw);
        }

        public void Read(BinaryReader br)
        {
            int v = br.ReadInt32();

            Points = br.ReadInt32();
            if (Points > 0)
            {
                MinLatitude = br.ReadDecimal();
                MaxLatitude = br.ReadDecimal();
                MinLongitude = br.ReadDecimal();
                MaxLongitude = br.ReadDecimal();
            }

            MinTime = ReadNullableDateTime(br);
            MaxTime = ReadNullableDateTime(br);
            MinElevation = ReadNullableDecimal(br);
            MaxElevation = ReadNullableDecimal(br);
        }

        public void AggregateWith(PointSummary subset)
        {
            this.Points += subset.Points;

            this.MinLatitude = NullableExtreme(this.MinLatitude, subset.MinLatitude, -1);
            this.MaxLatitude = NullableExtreme(this.MaxLatitude, subset.MaxLatitude, 1);
            this.MinLongitude = NullableExtreme(this.MinLongitude, subset.MinLongitude, -1);
            this.MaxLongitude = NullableExtreme(this.MaxLongitude, subset.MaxLongitude, 1);

            this.MinTime = MinNullableTime(this.MinTime, subset.MinTime);
            this.MaxTime = MaxNullableTime(this.MaxTime, subset.MaxTime);

            this.MinElevation = NullableExtreme(this.MinElevation, subset.MinElevation, -1);
            this.MaxElevation = NullableExtreme(this.MaxElevation, subset.MaxElevation, 1);
        }

        /// <summary>
        /// Of the non-null args, return the extreme
        /// </summary>
        /// <param name="sign">1 implies max, -1 implies min
        /// </param>
        static decimal? NullableExtreme(decimal? a, decimal? b, int sign)
        {
            if (a.HasValue)
            {
                if (b.HasValue)
                {
                    if (b.Value.CompareTo(a.Value) == sign)
                        return b;
                }
                return a;
            }
            else if (b.HasValue)
            {
                return b;
            }
            return null;
        }

        static DateTime? MinNullableTime(DateTime? dt, IPoint p)
        {
            return MinNullableTime(dt, p.Time);
        }

        static DateTime? MinNullableTime(DateTime? dt, DateTime? ptime)
        {
            if ( dt.HasValue )
            {
                if (ptime.HasValue && ptime.Value < dt.Value)
                    return ptime;

                return dt;
            }
            return ptime;
        }

        static DateTime? MaxNullableTime(DateTime? dt, IPoint p)
        {
            return MaxNullableTime(dt, p.Time);
        }

        static DateTime? MaxNullableTime(DateTime? dt, DateTime? ptime)
        {
            if (dt.HasValue)
            {
                if (ptime.HasValue && dt.Value < ptime.Value)
                    return ptime;

                return dt;
            }
            return ptime;
        }

        static void WriteNullable(DateTime? d, BinaryWriter bw)
        {
            bw.Write(d.HasValue);
            if (d.HasValue)
                bw.Write((Int64)d.Value.ToBinary());
        }

        static void WriteNullable(decimal? d, BinaryWriter bw)
        {
            bw.Write(d.HasValue);
            if (d.HasValue)
                bw.Write(d.Value);
        }

        static DateTime? ReadNullableDateTime(BinaryReader br)
        {
            bool exist = br.ReadBoolean();
            if (exist)
                return DateTime.FromBinary(br.ReadInt64());

            return null;
        }


        static decimal? ReadNullableDecimal(BinaryReader br)
        {
            bool exist = br.ReadBoolean();
            if (exist)
                return br.ReadDecimal();
            return null;
        }

        public override string ToString()
        {
            return string.Format("{0} Points, Lat [{1},{2}], Long [{3},{4}]", Points, MinLatitude, MaxLatitude, MinLongitude, MaxLongitude);
        }


    }
}
