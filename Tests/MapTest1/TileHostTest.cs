using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Diagnostics;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Abnaki.Albiruni.TileHost;
using Abnaki.Windows.Xml;

namespace MapTest1
{
    [TestClass]
    public class TileHostTest
    {
        [TestMethod]
        public void TestTileHostSupply()
        {
            try
            {
                TileHostSupply supply = new TileHostSupply();

                supply.Organizations.AddRange(Organization.CommercialProviders());

                supply.Write();

                TileHostSupply supplyReturned = TileHostSupply.Read();

                //Debug.WriteLine("Write/read returned " + orgReturned);
                //Organization.Mapbox.SquareUp(orgReturned);

                supplyReturned.SquareUpStatics();
            }
            catch ( Exception ex )
            {
                throw;
            }
        }
    }
}
