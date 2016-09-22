using System;
using System.Collections.Generic;
using System.Linq;

using Abnaki.Albiruni.TileHost;

namespace Abnaki.Albiruni.Message
{
    /// <summary>
    /// Logically occurs after TileHostMessage was handled
    /// and then TileImageLoader was updated according to a LocatorTemplate.
    /// </summary>
    class TileLoaderMessage
    {
        public TileLoaderMessage(LocatorTemplate loctemp)
        {
            this.LocatorTemplate = loctemp;
        }

        public LocatorTemplate LocatorTemplate { get; private set; }

    }
}
