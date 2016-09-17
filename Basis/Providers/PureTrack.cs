using System;
using System.Collections.Generic;
using System.Linq;

namespace Abnaki.Albiruni.Providers
{
    class PureTrack : ITrack
    {
        public IEnumerable<IPoint> Points
        {
            get;
            set;
        }
    }
}
