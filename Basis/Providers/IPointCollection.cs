using System;
using System.Collections.Generic;
using System.Linq;

namespace Abnaki.Albiruni.Providers
{
    public interface IPointCollection
    {
        IEnumerable<IPoint> Points { get; }
    }
}
