using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Abnaki.Albiruni.TileHost
{
    // static instances

    partial class LocatorTemplate
    {
        public static IEnumerable<LocatorTemplate> Predefined()
        {
            yield return CartoLight;
            yield return CartoDark;
            yield return WmfLabs;
            yield return MapXyz;
            yield return TfLandscape;
            yield return TfOutdoors;
            yield return Osm;
            yield return StamenToner;
            yield return StamenTerrain;
            yield return MapBoxStreets;
            yield return MapBoxSatellite;
            yield return MapBoxOutdoors;
            yield return HereNormal;
            yield return HereHybrid;
            //yield return MapBoxTerrain;
            //yield return OpenPt;
            //yield return StamenWatercolor;
            //yield return UsgsBase;
        }

        public static readonly LocatorTemplate Osm = new LocatorTemplate(Organization.Osm, "png");

        public static readonly LocatorTemplate CartoLight = new LocatorTemplate(Organization.Carto, "png", "light_all") { Style = "positron" };
        public static readonly LocatorTemplate CartoDark = new LocatorTemplate(Organization.Carto, "png", "dark_all") { Style = "dark matter" };

        public static readonly LocatorTemplate StamenTerrain = new LocatorTemplate(Organization.Stamen, "jpg", "terrain");
        public static readonly LocatorTemplate StamenToner = new LocatorTemplate(Organization.Stamen, "png", "toner");
        //public static readonly LocatorTemplate StamenWatercolor = new LocatorTemplate(Organization.Stamen, "jpg", "watercolor"); // not informative

        public static readonly LocatorTemplate MapXyz = new LocatorTemplate(Organization.MapXyz, "png");

        public static readonly LocatorTemplate WmfLabs = new LocatorTemplate(Organization.WmfLabs, "png", "osm");

        public static readonly LocatorTemplate TfLandscape = new LocatorTemplate(Organization.Thunderforest, "png", "landscape");
        public static readonly LocatorTemplate TfOutdoors = new LocatorTemplate(Organization.Thunderforest, "png", "outdoors");

        public static readonly LocatorTemplate MapBoxStreets = new LocatorTemplate(Organization.Mapbox, "png", "v4/mapbox.streets") { Style = "streets" };
        public static readonly LocatorTemplate MapBoxOutdoors = new LocatorTemplate(Organization.Mapbox, "png", "v4/mapbox.outdoors") { Style = "outdoors" };
        public static readonly LocatorTemplate MapBoxSatellite = new LocatorTemplate(Organization.Mapbox, "png", "v4/mapbox.satellite") { Style = "satellite" };
        //public static readonly LocatorTemplate MapBoxTerrain = new LocatorTemplate(Organization.Mapbox, "png", "v4/mapbox.terrain") { Style = "terrain" };

        const string hereSuffix = "/256/png8";
        const string hereSubdir = "maptile/2.1/maptile/newest/";
        static readonly Func<LocatorTemplate, string> hereCopyright =
            loc => string.Format("[sources]({0}://1.{1}.{2}/maptile/2.1/copyright/newest?output=xml&{3})",
                loc.Org.Domain.Uri.Scheme, loc.Subdomain, loc.Org.Domain.Uri.Host, loc.Org.UserKey);

        public static readonly LocatorTemplate HereNormal =
            new LocatorTemplate(Organization.Here, hereSuffix, hereSubdir + "normal.day") { Subdomain = "base", Style = "basemap", CopyrightFunc = hereCopyright };
        public static readonly LocatorTemplate HereHybrid = 
            new LocatorTemplate(Organization.Here, hereSuffix, hereSubdir + "hybrid.day") { Subdomain = "aerial", Style = "aerial", CopyrightFunc = hereCopyright };

        //public static readonly LocatorTemplate OpenPt = new LocatorTemplate(Organization.OpenPublicTransport, "png", "tiles") { Style = "public transport" };
        // public static readonly LocatorTemplate UsgsBase = new LocatorTemplate(Organization.Usgs, null, "arcgis/rest/services/USGSTopo/MapServer/tile");

    }
}
