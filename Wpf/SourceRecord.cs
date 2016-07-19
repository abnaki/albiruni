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
        }

        Source Source { get; set; }
        SourceContentSummary Summary { get; set; }

        public string Path { get { return this.Source.Path; } }

        public int Waypoints { get { return this.Summary.WayPoints.Points;  } }
        public int Trackpoints { get { return this.Summary.TrackPoints.Points; } }

        // want min/max times

        public override string ToString()
        {
            return GetType().Name + ", " + Source + ", " + Summary;
        }
    }
}
