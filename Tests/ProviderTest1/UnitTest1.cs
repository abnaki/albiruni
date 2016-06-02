using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Abnaki.Albiruni;
using Abnaki.Albiruni.Providers;

namespace ProviderTest1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestGpx()
        {
            var gfile = new Abnaki.Albiruni.Providers.GpxFile();
            // Not only does Basis project reference Geo (geospatial library) by Nuget, but also
            // github.com/sibartlett/Geo is cloned into a sibling workspace;
            // and note test executes in CurrentDirectory equal to bin\debug under project.
            string ddir = @"..\..\..\..\..\Geo\reference\gpx"; 

            DirectoryInfo di = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, ddir));
            FileInfo[] gpxfiles = di.GetFiles("*" + GpxFile.Extension);
            FileInfo fi = gpxfiles.OrderBy(f => f.Length).First();

            gfile.Deserialize(fi);

        }
    }
}
