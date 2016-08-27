﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Abnaki.Albiruni.TileHost
{
    /// <summary>
    /// Creates a Template, for example,
    /// http://{c}.host.com/subdir/{z}/{x}/{y}.png
    /// </summary>
    /// <remarks>
    /// Can be used with MapControl.TileSource but not strictly compatible with it.
    /// {c} would be substituted with a choice from [abc]
    /// 
    /// May allow for an access key in the future.
    /// </remarks>
    public class LocatorTemplate : IComparable<LocatorTemplate>
    {
        const string SubdomainTemplate = "{c}";  // for TileSource

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

        //public static readonly LocatorTemplate OpenPt = new LocatorTemplate(Organization.OpenPublicTransport, "png", "tiles") { Style = "public transport" };
        // public static readonly LocatorTemplate UsgsBase = new LocatorTemplate(Organization.Usgs, null, "arcgis/rest/services/USGSTopo/MapServer/tile");

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
            //yield return MapBoxTerrain;
            //yield return OpenPt;
            //yield return StamenWatercolor;
            //yield return UsgsBase;
        }

        /// <summary>
        /// </summary>
        /// <param name="org"></param>
        /// <param name="imageExt">extension of image file, no dot</param>
        /// <param name="subdirectory">optional host subdirectory</param>
        internal LocatorTemplate(Organization org, string imageExt, string subdirectory = null)
        {
            this.Subdirectory = subdirectory;
            this.Org = org;

            string subdom = null;
            if (org.Subdomains == null)
            {
                //Subdomains = Enumerable.Empty<string>();
            }
            else
            {
                subdom = SubdomainTemplate + ".";
                //Subdomains = subdoms.Cast<object>().Select(s => Convert.ToString(s));
            }

            string subdir = null;
            if (subdirectory != null)
                subdir = subdirectory + "/";

            string relativeUrl = "{z}/{x}/{y}";

            if (false == string.IsNullOrEmpty(imageExt))
                relativeUrl += "." + imageExt;

            if (false == string.IsNullOrEmpty(org.UserKey))
                relativeUrl += org.UriDelimitUserKey + org.UserKey;

            string port = null;
            if (false == org.Domain.Uri.IsDefaultPort)
                port = ":" + org.Domain.Uri.Port;

            this.Template = string.Format("{0}://{1}{2}{3}/{4}{5}", org.Domain.Uri.Scheme,
                subdom, org.Domain.Uri.Host, port,
                subdir, relativeUrl);
        }

        /// <summary>
        /// In some cases may be passed to MapControl.TileSource(uriFormat)
        /// </summary>
        public string Template { get; private set; }

        internal Organization Org { get; private set; }

        public string Subdirectory { get; private set; }

        /// <summary>
        /// Optional style of colors etc., if not obvious from Subdirectory; 
        /// no logical purpose
        /// </summary>
        public string Style { get; set; }

        /// <summary>True implies it should be serialized.  False implies application owns it.</summary>
        //public bool UserDefined { get; set; }

        //public IEnumerable<string> Subdomains { get; private set; }

        LocatorInstance GetInstance(object subdomain)
        {
            string uri;
            if (subdomain == null)
                uri = this.Template;
            else
                uri = this.Template.Replace(SubdomainTemplate, subdomain.ToString());

            return new LocatorInstance(uri);
        }

        internal LocatorInstance FirstInstance()
        {
            object subdomain = null;
            if (this.Org.Subdomains != null)
                subdomain = this.Org.Subdomains.First();

            return GetInstance(subdomain);
        }

        public override string ToString()
        {
            return Template;
        }

        public int CompareTo(LocatorTemplate other)
        {
            return this.Template.CompareTo(other.Template);
        }
    }
}
