using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Abnaki.Albiruni;
using Abnaki.Albiruni.Providers;
using Abnaki.Albiruni.Tree;

namespace ProviderTest1
{
    [TestClass]
    public class UnitTest1
    {
        DirectoryInfo TestingGpxDir()
        {
            // Not only does Basis project reference Geo (geospatial library) by Nuget, but also
            // github.com/sibartlett/Geo is cloned into a sibling workspace;
            // and note test executes in CurrentDirectory equal to bin\debug under project.
            string ddir = @"..\..\..\..\..\Geo\reference\gpx";

            return new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, ddir));
        }

        IEnumerable<FileInfo> TestingGpxFiles()
        {
            DirectoryInfo di = TestingGpxDir();

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

            var root = Node.NewGlobalRoot();

            Source source = new Abnaki.Albiruni.Tree.Source(figpx);

            root.Populate(source);

            root.DebugPrint();
        }

        [TestMethod]
        public void TestNursery()
        {
            DirectoryInfo di = TestingGpxDir();
            DirectoryInfo dicur = new DirectoryInfo(Environment.CurrentDirectory);
            DirectoryInfo ditarget = dicur.CreateSubdirectory("albiruni");

            CleanDirectory(ditarget);
            
            Node root = Node.NewGlobalRoot();

            string baseWildcard = "c*"; // sample of files

            Nursery.GrowTree(root, di, ditarget, baseWildcard);

            //root.DebugPrint();

            Node checkRoot = Node.NewGlobalRoot();
            Nursery.Read(checkRoot, ditarget, baseWildcard + Nursery.FileExt);

            //checkRoot.DebugPrint();
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
