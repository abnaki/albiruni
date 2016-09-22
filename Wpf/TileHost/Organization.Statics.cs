using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Abnaki.Albiruni.TileHost
{
    partial class Organization
    {
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
            FileKey = "mapbox.com",
            AllowMultiUserCache = false, // terms of service
            UserKey = UndefinedKey,
            UriDelimitUserKey = "?access_token="
        };

        public static readonly Organization Here = new Organization("https://maps.cit.api.here.com", "1234")
        {
            Copyright = "[HERE](http://developer.here.com)",
            FileKey = "here.com",
            AllowMultiUserCache = false, // terms of service
            UserKey = UndefinedKey,
            UriDelimitUserKey = "?" // requires 2 parameters
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

        public static readonly Organization Usgs = new Organization("http://basemap.nationalmap.gov");

    }
}
