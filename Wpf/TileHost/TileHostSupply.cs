using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;

using Abnaki.Windows.Xml;
using Abnaki.Windows;
using Abnaki.Windows.Software.Wpf.Diplomat;

namespace Abnaki.Albiruni.TileHost
{
    /// <summary>
    /// Allows limited external configurations of objects
    /// </summary>
    public class TileHostSupply
    {
        public TileHostSupply()
        {
            Organizations = new List<Organization>();
        }

        public List<Organization> Organizations { get; private set; }

        public const string ConfigFilename = "MapTileHosts.xml";

        public void Write()
        {
            FileInfo fi = XmlFileInfo();
            AbnakiXml.Write(fi.FullName, this, Subtypes());
            Debug.WriteLine("Wrote " + fi.FullName);
        }

        public static TileHostSupply Read()
        {
            try
            {
                FileInfo fi = XmlFileInfo();
                if (fi.Exists)
                    return AbnakiXml.Read<TileHostSupply>(fi, Subtypes());
            }
            catch ( Exception ex )
            {
                Notifier.Error(ex);
            }

            return null;
        }

        /// <summary>
        /// Existing static objects are set substantially equal to values inside this class
        /// </summary>
        public void SquareUpStatics()
        {
            foreach ( Organization org in Organization.CommercialProviders() )
            {
                Organization userDefinedOrg = FindOrganization(org);
                if (userDefinedOrg != null)
                    org.SquareUp(userDefinedOrg);
            }
        }

        Organization FindOrganization(Organization basis)
        {
            return this.Organizations.Where(org => org.FileKey != null).FirstOrDefault(org => org.FileKey == basis.FileKey);
        }

        static FileInfo XmlFileInfo()
        {
            var a = System.Reflection.Assembly.GetEntryAssembly();
            string dir;
            if (a == null)
                dir = Environment.CurrentDirectory;
            else
                dir = Path.GetDirectoryName(a.Location);

            return new FileInfo(Path.Combine(dir, ConfigFilename));
        }

        static Type[] Subtypes()
        {
            return new Type[] {  typeof(Organization) };
        }

    }
}
