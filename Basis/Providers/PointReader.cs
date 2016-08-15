using System;
using System.Collections.Generic;
using System.Linq;

namespace Abnaki.Albiruni.Providers
{
    /// <summary>
    /// Base class that can read or concoct a PointDump
    /// </summary>
    public class PointReader
    {
        public PointDump Points { get; protected set; }

    }
}
