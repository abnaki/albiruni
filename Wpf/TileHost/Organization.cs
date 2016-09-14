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
    public partial class Organization : IComparable<Organization>
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
        public string Copyright { get; private set; }

        /// <summary>
        /// Access token
        /// </summary>
        public string UserKey { get; set; }

        // wish to refactor UriDelimitUserKey and UserKey into a class responsible
        // for multiple named key parameters, http GET format, and help/information to user.

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



    }
}
