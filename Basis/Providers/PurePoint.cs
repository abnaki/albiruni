using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Abnaki.Windows;

namespace Abnaki.Albiruni.Providers
{
    /// <summary>
    /// Only pure Abnaki implementation needed for IPoint
    /// </summary>
    public class PurePoint : IPoint
    {
        public PurePoint()
        {

        }

        public PurePoint(IPoint p)
        {
            this.Latitude = p.Latitude;
            this.Longitude = p.Longitude;
            this.Elevation = p.Elevation;
            this.Time = p.Time;
            this.TimeReliable = p.TimeReliable;
        }

        public PurePoint(decimal lat, decimal lon)
        {
            this.Latitude = lat;
            this.Longitude = lon;
        }

        public decimal Latitude
        {
            get;
            set;
        }

        public decimal Longitude
        {
            get;
            set;
        }

        public decimal? Elevation
        {
            get;
            set;
        }

        public DateTime? Time
        {
            get;
            set;
        }

        public bool TimeReliable
        {
            get;
            set;
        }

        const int version = 1;

        public void Write(BinaryWriter bw)
        {
            bw.Write(version);
            bw.Write(Latitude);
            bw.Write(Longitude);
            AbnakiFile.WriteNullable(Time, bw);
            bw.Write(TimeReliable);
            AbnakiFile.WriteNullable(Elevation, bw);
        }

        public void Read(BinaryReader br)
        {
            int readVersion = br.ReadInt32();
            Latitude = br.ReadDecimal();
            Longitude = br.ReadDecimal();
            Time = AbnakiFile.ReadNullableDateTime(br);
            TimeReliable = br.ReadBoolean();
            Elevation = AbnakiFile.ReadNullableDecimal(br);
        }

        public override string ToString()
        {
            return GetType().Name + "(" + Latitude + ", " + Longitude + ")";
        }

    }
}
