using System;
using System.Collections.Generic;
using System.Linq;

using Abnaki.Albiruni.TileHost;

namespace Abnaki.Albiruni.Message
{
    class TileHostMessage
    {
        public TileHostMessage(LocatorTemplate loctemp)
        {
            this.LocatorTemplate = loctemp;
        }

        public LocatorTemplate LocatorTemplate { get; private set; }
    }
}
