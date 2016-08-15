using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Abnaki.Albiruni;
using Abnaki.Albiruni.Providers;
using Abnaki.Albiruni.Tree;

namespace Abnaki.Albiruni.Tests.Provider
{
    [TestClass]
    public class UnitTest1
    {
        static DirectoryInfo RelativeDir(string relative)
        {
            return new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, relative));
        }

        DirectoryInfo GeoGpxDir()
        {
            // Not only does Basis project reference Geo (geospatial library) by Nuget, but also
            // github.com/sibartlett/Geo is cloned into a sibling workspace;
            // and note test executes in CurrentDirectory equal to bin\debug under project.

            return RelativeDir(@"..\..\..\..\..\Geo\reference\gpx");
        }

        DirectoryInfo SampleGpxDir()
        {
            return RelativeDir(@"..\..\..\..\Sample\Flight");
        }

        DirectoryInfo SampleImageDir()
        {
            return RelativeDir(@"..\..\..\..\Sample\Images");
        }

        IEnumerable<FileInfo> GeoGpxFiles()
        {
            DirectoryInfo di = GeoGpxDir();
            return di.GetFiles("*" + GpxFile.Extension);
        }

        [TestMethod]
        public void TestTree()
        {
            FileInfo fi = GeoGpxFiles().OrderBy(f => f.Length).First();

            CompleteTest(fi);
        }

        [TestMethod]
        public void TestTreeMedium()
        {
            FileInfo fi = GeoGpxFiles().Where(f => f.Length < 15000).OrderBy(f => f.Length).Last();

            CompleteTest(fi);
        }

        [TestMethod]
        public void TestTreeHard()
        {
            FileInfo fi = GeoGpxFiles().Where(f => f.Name.StartsWith("Bergamo")).First();

            CompleteTest(fi);
        }

        // becoming archaic
        void CompleteTest(FileInfo figpx)
        {
            Debug.WriteLine(figpx.FullName);

            var root = Node.NewGlobalRoot();

            DirectoryInfo dicur = new DirectoryInfo(Environment.CurrentDirectory);

            Source source = new Abnaki.Albiruni.Tree.Source(figpx, dicur);

            Mesh mesh = new Mesh() { Power = 9 };

            root.Populate(source, mesh);

            IPoint samplePoint = source.PointProvider.Points.AllPoints.First();

            Node.FindResult testFindNodes = new Node.FindResult();
            root.FindNodes(samplePoint.Latitude, samplePoint.Longitude, mesh, testFindNodes);

            Debug.Assert(testFindNodes.List.Count > 0);

            root.DebugPrint();
        }

        [TestMethod]
        public void TestGpx()
        {
            Node root = TestRootGpx();
        }

        public Node TestRootGpx()
        {
            return TestRootSource(SampleGpxDir());
        }

        [TestMethod]
        public void TestImage()
        {
            Node root = TestRootSource(SampleImageDir());
        }

        Node TestRootSource(DirectoryInfo di)
        {
            DirectoryInfo dicur = new DirectoryInfo(Environment.CurrentDirectory);
            DirectoryInfo ditarget = dicur.CreateSubdirectory(Nursery.CacheDir);

            CleanDirectory(ditarget);

            Node root = Node.NewGlobalRoot();

            Nursery.Guidance guidance = new Nursery.Guidance();
            guidance.MinimumMesh = new Mesh(13);
            Nursery.GrowTree(root, di, ditarget, guidance);

            //root.DebugPrint();

            Node checkRoot = Node.NewGlobalRoot();
            Nursery.Read(checkRoot, ditarget, guidance);

            //checkRoot.DebugPrint();

            return root;
        }

        static void CleanDirectory(DirectoryInfo di)
        {
            foreach ( FileInfo fidat in di.GetFiles("*" + Nursery.FileExt) )
            {
                fidat.Delete();
            }

            foreach (DirectoryInfo disub in di.EnumerateDirectories() )
            {
                CleanDirectory(disub);
                disub.Delete();
            }
        }
    }
}
