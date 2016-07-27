using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Abnaki.Albiruni;
using Abnaki.Albiruni.Graphic;
using Abnaki.Albiruni.Tree;

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

            vm.DisplayMesh = new Mesh(6);

            var viewRect = new MapControl.MapRectangle()
            {
                West = -100,
                East = 60,
                North = 50,
                South = -40
            };

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
            foreach ( var r in vm.Rectangles )
            {
                Debug.WriteLine(r.ToStringUseful() + ", " + r.Fill);
            }
            Debug.Unindent();
        }
    }
}
