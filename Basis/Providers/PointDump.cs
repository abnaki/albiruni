using System;
using System.Collections.Generic;
using System.Linq;

namespace Abnaki.Albiruni.Providers
{
    /// <summary>
    /// Explicit copies to prevent costly enumeration through data in wrappers.
    /// </summary>
    public class PointDump
    {
        PointDump()
        {

        }

        public PointDump(IFile filedat)
        {
            this.WayPoints = new List<IPoint>(filedat.WayPoints);
            this.TrackPoints = new List<IPoint>(filedat.TrackPoints);
            this.RoutePoints = new List<IPoint>(filedat.RoutePoints);
        }

        public PointDump SuchThat(Func<IPoint,bool> allow)
        {
            PointDump sub = new PointDump();
            sub.WayPoints = this.WayPoints.Where(allow).ToList();
            sub.TrackPoints = this.TrackPoints.Where(allow).ToList();
            sub.RoutePoints = this.RoutePoints.Where(allow).ToList();
            return sub;
        }

        public IList<IPoint> WayPoints { get; private set; }
        public IList<IPoint> TrackPoints { get; private set; }
        public IList<IPoint> RoutePoints { get; private set; }

        public IEnumerable<IPoint> AllPoints
        {
            get
            {
                return WayPoints.Concat(TrackPoints).Concat(RoutePoints);
            }
        }

        public bool Any()
        {
            // return AllPoints.Any(); // slow ?

            return WayPoints.Count > 0
                || TrackPoints.Count > 0
                || RoutePoints.Count > 0;
        }

    }
}
