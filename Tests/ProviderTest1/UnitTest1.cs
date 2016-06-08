using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Abnaki.Albiruni;
using Abnaki.Albiruni.Providers;

namespace ProviderTest1
{
    [TestClass]
    public class UnitTest1
    {
        IEnumerable<FileInfo> TestingGpxFiles()
        {
            // Not only does Basis project reference Geo (geospatial library) by Nuget, but also
            // github.com/sibartlett/Geo is cloned into a sibling workspace;
            // and note test executes in CurrentDirectory equal to bin\debug under project.
            string ddir = @"..\..\..\..\..\Geo\reference\gpx";

            DirectoryInfo di = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, ddir));
            return di.GetFiles("*" + GpxFile.Extension);
        }

        [TestMethod]
        public void TestTree()
        {
            FileInfo fi = TestingGpxFiles().OrderBy(f => f.Length).First();

            CompleteTest(fi);
        }

        [TestMethod]
        public void TestTreeMedium()
        {
            FileInfo fi = TestingGpxFiles().Where(f => f.Length < 15000).OrderBy(f => f.Length).Last();

            CompleteTest(fi);
        }

        void CompleteTest(FileInfo figpx)
        {
            Debug.WriteLine(figpx.FullName);

            var root = Abnaki.Albiruni.Tree.Node.NewGlobalRoot();

            Abnaki.Albiruni.Tree.Source source = new Abnaki.Albiruni.Tree.Source(figpx);

            root.Populate(source);

            root.DebugPrint();
        }

    }
}
