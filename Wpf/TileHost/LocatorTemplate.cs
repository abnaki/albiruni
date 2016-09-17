using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

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
    public partial class LocatorTemplate : IComparable<LocatorTemplate>
    {
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

            this.CoordinateSystem = "{z}/{x}/{y}"; // TMS
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

                string relativeUrl = CoordinateSystem;

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
        /// Part of URL denoting matrix z, column x, and row y.  For example,
        /// {z}/{x}/{y}
        /// </summary>
        public string CoordinateSystem { get; set; }

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

        public Func<LocatorTemplate,string> CopyrightFunc { get; private set; }

        [XmlIgnore]
        public string FullCopyright
        {
            get
            {
                var sb = new System.Text.StringBuilder();
                sb.Append("Maps © ");
                sb.Append(this.Org.Copyright);
                if (CopyrightFunc != null)
                {
                    if (sb.Length > 0)
                        sb.Append(", ");

                    sb.Append(CopyrightFunc(this));
                }
                return sb.ToString();
            }
        }

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
