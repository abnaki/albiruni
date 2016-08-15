using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Abnaki.Albiruni;
using Abnaki.Albiruni.Graphic;
using Abnaki.Albiruni.Tree;
using MapControl;
using System.Collections.Generic;
using Abnaki.Albiruni.Providers;

namespace Abnaki.Albiruni.Tests.Map
{
    [TestClass]
    public class MapTest
    {
        [TestMethod]
        public void TestMapTree()
        {
            MapViewModel vm = new MapViewModel();

            var provTest = new Abnaki.Albiruni.Tests.Provider.UnitTest1();
            Node root = provTest.TestRootGpx();

            //root.DebugPrint();

            vm.DisplayMesh = new Mesh(12);

            var viewRect = new MapRectangle()
            {
                West = -100,
                East = 60,
                North = 50,
                South = -40
            };

            TestRectangles(root, viewRect, displayMeshPower: 12);
        }

        /// <summary>
        /// A complete family of descendant rectangles should coalesce into fewer rectangles
        /// </summary>
        [TestMethod]
        public void TestCoalesce()
        {
            Source source = new Source(new HardcodedPoints());
            Mesh minimumMesh;

            Node root = Node.NewGlobalRoot();
            root.Populate(source, HardMesh);
            root.DebugPrint();

            MapRectangle viewRect = new MapRectangle()
            {
                West = -2 * (double)HardMesh.Delta,
                East = 2 * (double)HardMesh.Delta,
                North = 2 * (double)HardMesh.Delta,
                South = -2 * (double)HardMesh.Delta
            };

            int displayMeshPower = HardMesh.Power;
            int nr = TestRectangles(root, viewRect, displayMeshPower);

            if (nr == 1)
                Debug.WriteLine("Coalesced all rectangles.");
            else if (nr == HardcodedPointFile.Count)
                Debug.WriteLine("No coalesced rectangles.");

            Assert.IsTrue(nr == 1);
        }

        int TestRectangles(Node root, MapRectangle viewRect, int displayMeshPower)
        {
            MapViewModel vm = new MapViewModel();

            vm.DisplayMesh = new Mesh(displayMeshPower);

            var unitRect = new MapControl.MapRectangle()
            {
                North = viewRect.North,
                West = viewRect.West,
                South = viewRect.North - 0.01,
                East = viewRect.West + 0.01
            };

            vm.SetViewPort(viewRect, unitRect);

            var msg = new Message.RootNodeMessage(root, disource: null);

            vm.HandleTree(msg);

            Debug.WriteLine(vm.Rectangles.Count + " Rectangles of " + vm);
            Debug.Indent();
            foreach (var r in vm.Rectangles)
            {
                Debug.WriteLine(r.ToStringUseful() + ", " + r.Fill);
            }
            Debug.Unindent();

            return vm.Rectangles.Count;
        }

        public static readonly Mesh HardMesh = new Mesh(6);

        class HardcodedPoints : PointReader
        {
            public HardcodedPoints()
            {
                this.Points = new PointDump(new HardcodedPointFile());
            }
        }

        class HardcodedPointFile : IFile
        {
            public const int Count = 4;

            IEnumerable<IPoint> IFile.WayPoints
            {
                get
                {
                    yield return new PurePoint(0, 0);
                    yield return new PurePoint(HardMesh.Delta, 0);
                    yield return new PurePoint(0, HardMesh.Delta);
                    yield return new PurePoint(HardMesh.Delta, HardMesh.Delta);
                }
            }

            IEnumerable<IPoint> IFile.TrackPoints
            {
                get { yield break; }
            }

            IEnumerable<IPoint> IFile.RoutePoints
            {
                get { yield break; }
            }
        }
    }
}
