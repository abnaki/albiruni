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
            Node root = provTest.TestNursery();

            root.DebugPrint();

            vm.PrecisionPower = 6;

            vm.SetViewPort(new MapControl.MapRectangle()
            {
                West = -100,
                East = 60,
                North = 50,
                South = -40
            });

            vm.HandleTree(root);

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
