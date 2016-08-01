using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Diagnostics;

using MapControl;
using System.Windows.Threading;
using Abnaki.Windows.Software.Wpf.Menu;

namespace Abnaki.Albiruni
{
    /// <summary>
    /// Draws MapViewModel Rectangles as fast as possible
    /// </summary>
    class MapNodeLayer : MapPanel
    {
        public MapNodeLayer()
        {
            vptimer.Tick += vptimer_Tick;

            ButtonBus<Menu.OptionMenuKey>.HookupSubscriber(HandleOption);
        }

        public new MapViewModel DataContext
        {
            get { return (MapViewModel)base.DataContext;  }
            set { base.DataContext = value; }
        }

        void Clear()
        {
            vptimer.Stop();
        }

        private void HandleOption(ButtonMessage<Menu.OptionMenuKey> msg)
        {
            if (false == msg.Checked)
                return;

            switch (msg.Key)
            {
                case Menu.OptionMenuKey.MapCellColorRed:
                case Menu.OptionMenuKey.MapCellColorGreen:
                case Menu.OptionMenuKey.MapCellColorBlue:
                    InvalidateVisual();
                    break;
            }

        }


        #region Community-suggested xamlmapcontrol viewport change event using a timer

        void vptimer_Tick(object sender, EventArgs e)
        {
            vptimer.Stop();

            ViewportChangeSettled();
        }

        DispatcherTimer vptimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(200) };

        protected override void OnViewportChanged()
        {
            base.OnViewportChanged();

            vptimer.Stop(); vptimer.Start(); // MS idea of Reset

            InvalidateVisual(); // all current graphics must be redrawn
        }


        #endregion

        void ViewportChangeSettled()
        {
            if (DataContext == null)
                return;

            var map = ParentMap;

            Point pNorthWest = new Point(0, 0);
            Point pSouthEast = new Point(map.ActualWidth, map.ActualHeight);
            Point pUnit = new Point(1, 1);
            Location locNorthWest = map.ViewportPointToLocation(pNorthWest);
            Location locSouthEast = map.ViewportPointToLocation(pSouthEast);
            Location locUnit = map.ViewportPointToLocation(pUnit);

            MapRectangle viewRect = new MapRectangle()
            {
                North = locNorthWest.Latitude,
                South = locSouthEast.Latitude,
                West = locNorthWest.Longitude,
                East = locSouthEast.Longitude
            };

            MapRectangle unitRect = new MapRectangle()
            {
                North = locNorthWest.Latitude,
                South = locUnit.Latitude,
                West = locNorthWest.Longitude,
                East = locUnit.Longitude
            };

            this.DataContext.SetViewPort(viewRect, unitRect);

            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            if (ParentMap == null || this.DataContext == null )
                return;

            //Debug.WriteLine(GetType().Name + " rendering " + this.DataContext.Rectangles.Count + " rectangles");

            foreach ( MapRectangle mrect in this.DataContext.Rectangles )
            {
                Render(dc, mrect);
            }
        }

        void Render(DrawingContext dc, MapRectangle mrect)
        {
            Rect r = RectFromMap(mrect);
            dc.DrawRectangle(mrect.Fill, pen: null, rectangle: r);
        }

        Rect RectFromMap(MapRectangle mrect)
        {
            return new Rect(ParentMap.LocationToViewportPoint(new Location(mrect.North, mrect.West)),
                ParentMap.LocationToViewportPoint(new Location(mrect.South, mrect.East)));

        }
    }
}
