﻿using System;
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


        public static readonly Organization Carto = new Organization("http://basemaps.cartocdn.com", "abcd")
        {
            Copyright = "[OpenStreetMap](http://www.openstreetmap.org/copyright) contributors and [CARTO](https://carto.com/attributions)"
        };

        public static readonly Organization Stamen = new Organization("http://tile.stamen.com")
        {
            Copyright = "[Stamen Design](http://stamen.com/), Data by [OpenStreetMap](http://openstreetmap.org/)"
        };

        public static readonly Organization Osm = new Organization("http://tile.openstreetmap.org", "abc")
        {
            Copyright = "[OpenStreetMap Contributors](http://www.openstreetmap.org/copyright)"
        };

    }
}