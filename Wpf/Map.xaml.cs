using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Diagnostics;

using MapControl;

namespace Abnaki.Albiruni
{
    /// <summary>
    /// Interaction logic for Map.xaml
    /// </summary>
    partial class Map : UserControl
    {
        public Map()
        {
            InitializeComponent();

            vptimer.Tick += vptimer_Tick;
        }

        public new MapViewModel DataContext
        {
            get { return base.DataContext as MapViewModel; }
            set { base.DataContext = value; }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            this.DataContext = new MapViewModel();
            this.DataContext.Testing();
        }

        #region Originally from xamlmapcontrol/SampleApps/WpfApplication/MainWindow.xaml.cs

        private void MapMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                map.ZoomMap(e.GetPosition(map), Math.Floor(map.ZoomLevel + 1.5));
                //map.TargetCenter = map.ViewportPointToLocation(e.GetPosition(map));
            }
        }

        private void MapMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                map.ZoomMap(e.GetPosition(map), Math.Ceiling(map.ZoomLevel - 1.5));
            }
        }

        private void MapManipulationInertiaStarting(object sender, ManipulationInertiaStartingEventArgs e)
        {
            e.TranslationBehavior.DesiredDeceleration = 0.001;
        }

        private void MapItemTouchDown(object sender, TouchEventArgs e)
        {
            var mapItem = (MapItem)sender;
            mapItem.IsSelected = !mapItem.IsSelected;
            e.Handled = true;
        }

        #endregion

        #region Community-suggested xamlmapcontrol viewport change event using a timer

        void vptimer_Tick(object sender, EventArgs e)
        {
            vptimer.Stop();

            Point pNorthWest = new Point(0, 0);
            Point pSouthEast = new Point(map.ActualWidth, map.ActualHeight);
            Location locNorthWest = map.ViewportPointToLocation(pNorthWest);
            Location locSouthEast = map.ViewportPointToLocation(pSouthEast);

            this.DataContext.ViewPortRect = new MapRectangle()
            {
                North = locNorthWest.Latitude,
                South = locSouthEast.Latitude,
                West = locNorthWest.Longitude,
                East = locSouthEast.Longitude
            };
        }

        System.Windows.Threading.DispatcherTimer vptimer 
            = new System.Windows.Threading.DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(200) };

        private void map_ViewportChanged(object sender, EventArgs e)
        {
            vptimer.Stop(); vptimer.Start(); // MS idea of Reset
        }


        #endregion

    }
}
