using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

//using Geo.Abstractions.Interfaces; // used IPosition
using Abnaki.Albiruni.Providers;

namespace Abnaki.Albiruni.Tree
{
    public class SourceContentSummary
    {
        public SourceContentSummary()
        {
            WayPoints = new PointSummary();
            TrackPoints = new PointSummary();
            RoutePoints = new PointSummary();
        }

        public SourceContentSummary(PointDump points)
            : this(points.WayPoints, points.TrackPoints, points.RoutePoints)
        {

        }

        SourceContentSummary(IEnumerable<IPoint> waypoints, 
            IEnumerable<IPoint> trackpoints,
            IEnumerable<IPoint> routepoints)
        {
            WayPoints = new PointSummary(waypoints);
            TrackPoints = new PointSummary(trackpoints);
            RoutePoints = new PointSummary(routepoints);
        }

        /// <summary>
        /// Aggregate of all inputs
        /// </summary>
        public SourceContentSummary(IEnumerable<SourceContentSummary> summaries)
            : this()
        {
            foreach ( SourceContentSummary s in summaries )
            {
                AggregateWith(s);
            }
        }

        public PointSummary WayPoints { get; private set; }
        public PointSummary TrackPoints { get; private set; }
        public PointSummary RoutePoints { get; private set; }

        const int filever = 3;

        public void Write(BinaryWriter bw)
        {
            bw.Write(filever);

            WayPoints.Write(bw);
            TrackPoints.Write(bw);
            RoutePoints.Write(bw);
        }

        public void Read(BinaryReader br)
        {
            int v = br.ReadInt32();

            if (v < 3)
                throw new NotSupportedException("Old file version");

            WayPoints.Read(br);
            TrackPoints.Read(br);
            RoutePoints.Read(br);
        }

        public void AggregateWith(SourceContentSummary subset)
        {
            this.WayPoints.AggregateWith(subset.WayPoints);
            this.TrackPoints.AggregateWith(subset.TrackPoints);
            this.RoutePoints.AggregateWith(subset.RoutePoints);
        }

        public PointSummary FinalSummary()
        {
            PointSummary ps = new PointSummary();
            ps.AggregateWith(this.WayPoints);
            ps.AggregateWith(this.TrackPoints);
            ps.AggregateWith(this.RoutePoints);
            return ps;
        }

        public override string ToString()
        {
            return Summarize("Way", WayPoints) + Summarize("Track", TrackPoints) + Summarize("Route", RoutePoints);
        }

        static string Summarize(string prefix, PointSummary ps)
        {
            return string.Format("{0}({1}) ", prefix, ps);
        }
    }
}
