using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Abnaki.Albiruni.TileHost
{
    /// <summary>
    /// Independent legal entity that serves up map tiles
    /// </summary>
    class Organization
    {
        public Organization(string partUri, IEnumerable subdoms = null)
        {
            this.Domain = new Uri(partUri);

            if (subdoms == null)
                this.Subdomains = Enumerable.Empty<object>();
            else
                this.Subdomains = subdoms.Cast<object>();
        }

        /// <summary>
        /// Significant parts are scheme, host, port.
        /// Does not specify any subdomain(s) or local path.
        /// </summary>
        public Uri Domain { get; private set; }

        /// <summary>
        /// Optional strings or chars of subdomains of org
        /// </summary>
        public IEnumerable<object> Subdomains { get; private set; }

        /// <summary>
        /// Markdown format.  Words/urls only.  Don't include (c) symbol.
        /// </summary>
        public string Copyright { get; set; }

        /// <summary>
        /// Future
        /// </summary>
        public string UserKey { get; set; }

        /// <summary>Open to the world
        /// </summary>
        public bool Public { get { return string.IsNullOrEmpty(UserKey); } }

        const string citeosm = "[OpenStreetMap Contributors](http://openstreetmap.org/copyright)";

        public static readonly Organization Carto = new Organization("http://basemaps.cartocdn.com", "abcd")
        {
            Copyright = "[CARTO](https://carto.com/attributions), CC BY 3.0, Data by " + citeosm + ", ODbL"
        };

        public static readonly Organization Stamen = new Organization("http://tile.stamen.com")
        {
            Copyright = "[Stamen Design](http://stamen.com/), Data by " + citeosm
        };

        public static readonly Organization Osm = new Organization("http://tile.openstreetmap.org", "abc")
        {
            Copyright = citeosm
        };

        public static readonly Organization MapXyz = new Organization("http://osm.maptiles.xyz", "abc")
        {
            Copyright = citeosm
        };

        public static readonly Organization WmfLabs = new Organization("http://tiles.wmflabs.org", "abc")
        {
            Copyright = "[wmflabs](http://wmflabs.org/), Data by " + citeosm
        };

        public static readonly Organization Thunderforest = new Organization("http://tile.thunderforest.com", "abc")
        {
            Copyright = citeosm
        };

        //public static readonly Organization OpenPublicTransport = new Organization("http://www.openptmap.org")
        //{
        //    Copyright = citeosm
        //};

        // public static readonly Organization Usgs = new Organization("http://basemap.nationalmap.gov");

    }
}
