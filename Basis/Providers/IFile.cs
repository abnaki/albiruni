﻿using System;
using System.Collections.Generic;
using System.Linq;


namespace Abnaki.Albiruni.Providers
{
    /// <summary>
    /// Essential sets of data from a file
    /// </summary>
    public interface IFile
    {
        IEnumerable<IPoint> WayPoints { get; }

        IEnumerable<IPoint> TrackPoints { get; }

        IEnumerable<IPoint> RoutePoints { get; }
    }
}
