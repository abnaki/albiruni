using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Abnaki.Albiruni.Tree
{
    public class SourceContentSummary
    {
        public int Points { get; set; }
        //public int Waypoints { get; set; }
        //public int Trackpoints { get; set; }

        public DateTime? MinDate { get; set; }
        public DateTime? MaxDate { get; set; }

        public double? MinElevation { get; set; }
        public double? MaxElevation { get; set; }

        public double? MinSpeed { get; set; }
        public double? MaxSpeed { get; set; }

        public override string ToString()
        {
            return this.Points + " Points";
        }

        const int filever = 1;

        public void Write(BinaryWriter bw)
        {
            bw.Write(filever);

            bw.Write(Points);
        }

        public void Read(BinaryReader br)
        {
            int v = br.ReadInt32();

            Points = br.ReadInt32();
        }

        public void AggregateWith(SourceContentSummary subset)
        {
            this.Points += subset.Points;

            // etc
        }
    }
}
