using System;
using System.Collections.Generic;
using System.Linq;

using Abnaki.Albiruni.Tree;

namespace Abnaki.Albiruni
{
    class SourceRecord
    {
        public SourceRecord(Source source, SourceContentSummary summary)
        {
            this.Source = source;
            this.Summary = summary;
            this.OverallPoints = summary.FinalSummary();
        }

        Source Source { get; set; }
        public SourceContentSummary Summary { get; private  set; }
        PointSummary OverallPoints { get; set; }

        public string Path { get { return this.Source.Path; } }

        public int Waypoints { get { return this.Summary.WayPoints.Points;  } }
        public int Trackpoints { get { return this.Summary.TrackPoints.Points; } }
        public int Routepoints { get { return this.Summary.RoutePoints.Points; } }

        public DateTime? MinTime { get { return this.OverallPoints.MinTime; } }
        public DateTime? MaxTime { get { return this.OverallPoints.MaxTime; } }

        public override string ToString()
        {
            return GetType().Name + ", " + Source + ", " + Summary;
        }
    }
}
