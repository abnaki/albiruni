using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

//using Geo.Gps;
//using Geo.Gps.Serialization;
//using Geo.Gps.Serialization.Xml;

namespace Abnaki.Albiruni.Providers
{
    /// <summary>
    /// Reads gpx 1.0 or 1.1
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class GpxFile : FileReader
    {
        public static string Extension
        {
            get { return ".gpx";  }
        }

        // GpsData was a lowest common denominator 
        //  with objects in terms of lat,lon and little else.
        //public GpsData GeoGpsData { get; private set; }

        protected override IFile OpenFile(FileInfo fi)
        {
            string v = null;
            using (Stream str = fi.OpenRead())
            using (XmlReader xr = XmlTextReader.Create(str))
            {
                for (int i = 0; i < 5 && xr.Name != "gpx"; i++)
                {
                    bool b = xr.Read();
                }
                if (xr.Name != "gpx")
                    throw new ApplicationException("Failed to find gpx in " + fi.FullName);

                v = xr.GetAttribute("version");
            }

            XmlSerializer xser = null;
            Stream rstr = fi.OpenRead();
            try {
                switch (v)
                {
                    case "1.0":
                        xser = new XmlSerializer(typeof(global::Geo.Gps.Serialization.Xml.Gpx.Gpx10.GpxFile));
                        var gobv10 = (global::Geo.Gps.Serialization.Xml.Gpx.Gpx10.GpxFile)
                            xser.Deserialize(rstr);
                        // return GeniusCode.Components.ProxyFactory.DuckInterface<IFile>(gobv10, new Providers.Geo.Gpx.V10.FileDuckProvider());
                        return Providers.Geo.Gpx.V10.FileDuck.FileFromGpx(gobv10);

                    case "1.1":
                        xser = new XmlSerializer(typeof(global::Geo.Gps.Serialization.Xml.Gpx.Gpx11.GpxFile));
                        var gobv11 = (global::Geo.Gps.Serialization.Xml.Gpx.Gpx11.GpxFile)
                            xser.Deserialize(rstr);
                        // return GeniusCode.Components.ProxyFactory.DuckInterface<IFile>(gobv11, new Providers.Geo.Gpx.V11.FileDuckProvider());
                        return Providers.Geo.Gpx.V11.FileDuck.FileFromGpx(gobv11);
                }
            }
            finally 
            {
                rstr.Dispose();
            }

            throw new ApplicationException("No support for gpx version " + v + " of " + fi.FullName);
        }
    }
}
