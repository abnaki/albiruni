using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Abnaki.Albiruni.TileHost
{
    /// <summary>
    /// Independent legal entity that serves up map tiles
    /// </summary>
    public class Organization : IComparable<Organization>
    {
        public Organization(string partUri, IEnumerable subdoms = null)
        {
            this.Domain = new Abnaki.Windows.Xml.Yuri(partUri);

            if (subdoms == null)
                this.Subdomains = new string[] { };
            else
                this.Subdomains = subdoms.Cast<object>().Select(v => Convert.ToString(v)).ToArray();
        }

        public Organization() // serializ.
        {

        }

        public void SquareUp(Organization userInput)
        {
            if (this.FileKey != userInput.FileKey)
                throw new ArgumentException("Cannot SquareUp " + GetType().Name + " of mismatching FileKey, " + userInput.FileKey + ", with existing " + this.FileKey);

            this.Domain = userInput.Domain;
            this.Subdomains = userInput.Subdomains;
            this.UserKey = userInput.UserKey;
        }

        /// <summary>
        /// Significant parts are scheme, host, port.
        /// Does not specify any subdomain(s) or local path.
        /// </summary>
        public Abnaki.Windows.Xml.Yuri Domain { get; set; }

        /// <summary>
        /// Optional strings or chars of subdomains of org.
        /// </summary>
        public string[] Subdomains { get; set; }

        //public string SubdomainTemplate { get; set; }

        /// <summary>
        /// Markdown format.  Words/urls only.  Don't include (c) symbol.
        /// </summary>
        [XmlIgnore]
        public string Copyright { get; set; }

        /// <summary>
        /// Access token
        /// </summary>
        public string UserKey { get; set; }

        /// <summary>
        /// Will appear file path in uri, before UserKey, e.g. ?access_token=
        /// </summary>
        [XmlIgnore]
        public string UriDelimitUserKey { get; set; }

        /// <summary>
        /// ID of Organization within a configuration file, if necssary
        /// </summary>
        public string FileKey { get; set; }

        /// <summary>Open to the world
        /// </summary>
        // does not yet consider a local server needing no key
        public bool Public { get { return string.IsNullOrEmpty(UriDelimitUserKey); } }

        /// <summary>
        /// Future
        /// </summary>
        [XmlIgnore]
        public bool AllowMultiUserCache = true;

        public override string ToString()
        {
            return Domain.ToString();
        }

        public int CompareTo(Organization other)
        {
            return this.Domain.CompareTo(other.Domain);
        }

        const string citeosm = "[OpenStreetMap Contributors](http://openstreetmap.org/copyright)";

        public const string UndefinedKey = "undefined";

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

        public static readonly Organization Mapbox = new Organization("https://tiles.mapbox.com", "abcd")
        {
            Copyright = "[Mapbox](https://www.mapbox.com/about/maps/), " + citeosm,
            FileKey = "mapbox",
            AllowMultiUserCache = false, // terms of service
            UserKey = UndefinedKey,
            UriDelimitUserKey = "?access_token="
        };

        public static readonly Organization Here = new Organization("https://maps.cit.api.here.com","1234")
        {
            FileKey = "here.com",
            AllowMultiUserCache = false, // terms of service
            UserKey = UndefinedKey,
            UriDelimitUserKey = "?"
            // Copyright will require query involving UserKey
        };

        public static IEnumerable<Organization> CommercialProviders()
        {
            yield return Mapbox;
            yield return Here;
        }

        //public static readonly Organization OpenPublicTransport = new Organization("http://www.openptmap.org")
        //{
        //    Copyright = citeosm
        //};

        // public static readonly Organization Usgs = new Organization("http://basemap.nationalmap.gov");


    }
}
