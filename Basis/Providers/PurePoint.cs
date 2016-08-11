using System;
using System.Collections.Generic;
using System.Linq;

namespace Abnaki.Albiruni.Providers
{
    /// <summary>
    /// Only pure Abnaki implementation needed for IPoint
    /// </summary>
    public class PurePoint : IPoint
    {
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
    }
}
