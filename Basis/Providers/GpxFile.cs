using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using Geo.Gps;
using Geo.Gps.Serialization;
using Geo.Gps.Serialization.Xml;

namespace Abnaki.Albiruni.Providers
{
    /// <summary>
    /// Reads gpx 1.0 or 1.1
    /// </summary>
    public class GpxFile
    {
        public static string Extension
        {
            get { return ".gpx";  }
        }

        public void Deserialize(FileInfo fi)
        {
            if (false == fi.Exists)
                throw new FileNotFoundException("Nonexistent " + fi.FullName);

            var gser = NewSerializer(fi);
            using ( Stream str = fi.OpenRead() )
            using (StreamWrapper sw = new StreamWrapper(str))
            {
                gdata = gser.DeSerialize(sw);
            }
        }

        GpsData gdata;

        IGpsFileSerializer NewSerializer(FileInfo fi)
        {
            //XmlDocument xdoc = new XmlDocument();
            using (Stream str = fi.OpenRead())
            using (XmlReader xr = XmlTextReader.Create(str) )
            {
                bool b = xr.Read();

                string v = xr.GetAttribute("version");

                switch ( v )
                {
                    case "1.0":
                        return new Geo.Gps.Serialization.Gpx10Serializer();

                    case "1.1":
                        return new Geo.Gps.Serialization.Gpx11Serializer();

                    default:
                        throw new ApplicationException("No support for gpx version " + v);
                }
            }
            
        }
    }
}
