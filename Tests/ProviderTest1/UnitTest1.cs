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
        DirectoryInfo GeoGpxDir()
        {
            // Not only does Basis project reference Geo (geospatial library) by Nuget, but also
            // github.com/sibartlett/Geo is cloned into a sibling workspace;
            // and note test executes in CurrentDirectory equal to bin\debug under project.
            string ddir = @"..\..\..\..\..\Geo\reference\gpx";

            return new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, ddir));
        }

        DirectoryInfo SampleGpxDir()
        {
            string ddir = @"..\..\..\..\Sample\Flight";

            return new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, ddir));
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

        // becoming archaic
        void CompleteTest(FileInfo figpx)
        {
            Debug.WriteLine(figpx.FullName);

            var root = Node.NewGlobalRoot();

            Source source = new Abnaki.Albiruni.Tree.Source(figpx);

            root.Populate(source, minDelta: 1);

            root.DebugPrint();
        }

        [TestMethod]
        public Node TestNursery()
        {
            DirectoryInfo di = SampleGpxDir();
            DirectoryInfo dicur = new DirectoryInfo(Environment.CurrentDirectory);
            DirectoryInfo ditarget = dicur.CreateSubdirectory("albiruni");

            CleanDirectory(ditarget);

            Node root = Node.NewGlobalRoot();

            Nursery.Guidance guidance = new Nursery.Guidance();
            Nursery.GrowTree(root, di, ditarget, guidance);

            //root.DebugPrint();

            Node checkRoot = Node.NewGlobalRoot();
            Nursery.Read(checkRoot, ditarget, guidance.Wildcard + Nursery.FileExt);

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
