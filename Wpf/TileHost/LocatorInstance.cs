using System;
using System.Collections.Generic;
using System.Linq;

namespace Abnaki.Albiruni.TileHost
{
    /// <summary>
    /// Free from subdomain
    /// </summary>
    class LocatorInstance
    {
        public LocatorInstance(string uriFormat)
        {
            this.UriFormat = uriFormat;
        }

        /// <summary>
        /// To pass to MapControl.TileSource(uriFormat), no unspecified subdomain
        /// </summary>
        public string UriFormat { get; private set; }

        public Uri TestImageUri()
        {
            return new Uri(UriFormat.Replace("{z}", "1") // zoom level
                .Replace("{x}", "1") // 0 North America, 1 Asia
                .Replace("{y}", "0"));
        }

    }
}
