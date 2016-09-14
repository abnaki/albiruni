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
        public static readonly LocatorTemplate HereNormal = new LocatorTemplate(Organization.Here, hereSuffix, hereSubdir + "normal.day") { Subdomain = "base", Style = "basemap" };
        public static readonly LocatorTemplate HereHybrid = new LocatorTemplate(Organization.Here, hereSuffix, hereSubdir + "hybrid.day") { Subdomain = "aerial", Style = "aerial" };

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
            yield return HereNormal;
            yield return HereHybrid;
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
            this.ImageSuffix = imageExt;
        }

        /// <summary>
        /// In some cases may be passed to MapControl.TileSource(uriFormat)
        /// </summary>
        public string Template
        {
            get
            {
                // The specific host logic is uniform with an Organization.
                string subdom = null;
                //if (Org.SubdomainTemplate != null)
                //{
                //    subdom = Org.SubdomainTemplate;
                //}
                //else
                if (Org.Subdomains == null)
                {
                    // nothing
                }
                // Template logic follows TileSource.cs.
                else if (Org.Subdomains.FirstOrDefault() == "a")
                {
                    subdom = "{c}.";
                }
                else if (Org.Subdomains.FirstOrDefault() == "1")
                {
                    subdom = "{n}.";
                }

                // More general subdomain of a style of map
                if (false == string.IsNullOrEmpty(this.Subdomain))
                {
                    subdom += Subdomain + ".";
                }

                string subdir = null;
                if (Subdirectory != null)
                    subdir = Subdirectory + "/";

                string relativeUrl = "{z}/{x}/{y}";

                if (false == string.IsNullOrEmpty(ImageSuffix))
                {
                    if (false == ImageSuffix.StartsWith("/"))
                        relativeUrl += ".";

                    relativeUrl += ImageSuffix;
                }

                if (false == string.IsNullOrEmpty(Org.UserKey))
                    relativeUrl += Org.UriDelimitUserKey + Org.UserKey;

                string port = null;
                if (false == Org.Domain.Uri.IsDefaultPort)
                    port = ":" + Org.Domain.Uri.Port;

                return string.Format("{0}://{1}{2}{3}/{4}{5}", Org.Domain.Uri.Scheme,
                    subdom, Org.Domain.Uri.Host, port,
                    subdir, relativeUrl);

            }
        }

        public string FileKey
        {
            get
            {
                return this.Org + "/" + Subdirectory;
            }
        }

        string ImageSuffix { get; set; }

        internal Organization Org { get; private set; }

        public string Subdomain { get; set; }

        public string Subdirectory { get; private set; }

        /// <summary>
        /// Optional style of colors etc., if not obvious from Subdirectory; 
        /// no logical purpose
        /// </summary>
        public string Style { get; set; }

        public bool Valid
        {
            get
            {
                return this.Org.Public || this.Org.UserKey != Organization.UndefinedKey;
            }
        }

        /// <summary>True implies it should be serialized.  False implies application owns it.</summary>
        //public bool UserDefined { get; set; }

        //public IEnumerable<string> Subdomains { get; private set; }

        LocatorInstance GetInstance(object subdomain)
        {
            throw new NotImplementedException();
            string uri;
            if (subdomain == null)
                uri = this.Template;
            else
                //uri = this.Template.Replace(DefaultSubdomainTemplate, subdomain.ToString());
                throw new NotImplementedException();

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
            int cmp = this.Org.CompareTo(other.Org);
            if (cmp != 0)
                return cmp;

            // need nullable compare
            cmp = Abnaki.Windows.Compare.CompareNullPossible(this.Subdirectory, other.Subdirectory);
            if (cmp != 0)
                return cmp;

            cmp = Abnaki.Windows.Compare.CompareNullPossible(this.Subdomain, other.Subdomain);
            return cmp;
        }

        public static IEnumerable<IGrouping<Organization,LocatorTemplate>> PredefinedOrganizationGroups()
        {
            return Predefined().GroupBy(loctemp => loctemp.Org);
        }
    }
}
