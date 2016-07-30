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
using System.Windows.Threading;

using MapControl;
using Abnaki.Windows.Software.Wpf;
using Abnaki.Windows.Software.Wpf.Ultimate;
using Abnaki.Windows.Software.Wpf.Profile;
using Abnaki.Albiruni.Tree;
using Abnaki.Albiruni.Graphic;

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

            hovtimer.Tick += hovtimer_Tick;

            MessageTube.Subscribe<FarewellMessage>(Farewell);

            Clear();
        }

        public new MapViewModel DataContext
        {
            get { return base.DataContext as MapViewModel; }
            set { base.DataContext = value; }
        }

        void Clear()
        {
            hovtimer.Stop();
            hovPoint = new Point();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            this.DataContext = new MapViewModel();
            this.DataContext.DisplayMesh = new Mesh((int)slprecision.Value);
            //this.DataContext.Testing();

            MapPref pref = Preference.ReadClassPrefs<Map, MapPref>();
            if ( pref != null )
            {
                slzoom.Value = pref.Zoom;
                slprecision.Value = pref.PrecisionPower;
                ChkSync.IsChecked = pref.SyncZoom;
            }
        }


        #region Originally from xamlmapcontrol/SampleApps/WpfApplication/MainWindow.xaml.cs

        private void MapMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // not raised if hit a MapRectangle 

            if (e.ClickCount == 2)
            {
                //Point p = e.GetPosition(map);
                //Location loc = map.ViewportPointToLocation(p);
                //map.ZoomMap(p, Math.Floor(map.ZoomLevel + 1.5));
                //map.TargetCenter = loc;
            }
        }

        private void MapMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            //if (e.ClickCount == 2)
            //{
            //    map.ZoomMap(e.GetPosition(map), Math.Ceiling(map.ZoomLevel - 1.5));
            //}
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

        #region Hover

        DispatcherTimer hovtimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(0.4) };
        Point hovPoint = new Point();

        private void map_MouseLeave(object sender, MouseEventArgs e)
        {
            hovtimer.Stop();
        }

        private void map_MouseMove(object sender, MouseEventArgs e)
        {
            hovtimer.Stop();

            Point pmoved = Mouse.GetPosition(map);
            if (pmoved != hovPoint)
            {
                hovtimer.Start();
            }
            // else, observed unwanted MouseMove events at same point if hovering on a drawn MapPolyline
        }

        void hovtimer_Tick(object sender, EventArgs e)
        {
            hovtimer.Stop();

            hovPoint = Mouse.GetPosition(map);

            Location loc = map.ViewportPointToLocation(hovPoint);
            //Debug.WriteLine("Hover on " + loc);

            this.DataContext.OnHover(loc);
        }


        #endregion

        bool syncingZoomPrecision = false;
        bool outdatedMesh = false;
        double? precisionMinusZoomSynced;

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if ( outdatedMesh )
            {
                if (this.DataContext != null)
                    this.DataContext.UpdateMesh();

                InvalidateMapPanels();
            }
        }

        private void slprecision_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //slzoom.Value += e.NewValue - e.OldValue of slprecision
            double deltaPrecision = e.NewValue - e.OldValue;
            ChangeBoundedBySlider(slzoom, e, slzoom.Value + deltaPrecision, z => map.ZoomLevel = z);

            outdatedMesh = true;
            InvalidateVisual();
        }

        private void slzoom_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //slprecision.Value = precisionMinusZoomSynced.Value + slzoom.Value
            ChangeBoundedBySlider(slprecision, e, precisionMinusZoomSynced + slzoom.Value, p => slprecision.Value = p);
            //outdatedMesh = true; 
            //InvalidateVisual(); // implied by slprecision Value change
        }

        void ChangeBoundedBySlider(Slider slitarget,
            RoutedPropertyChangedEventArgs<double> e,
            double? proposed,
            Action<double> actset)
        {
            if (syncingZoomPrecision)
                return;
            if (false == proposed.HasValue)
                return;
            if (false == precisionMinusZoomSynced.HasValue)
                return;

            bool allowed = false;
            syncingZoomPrecision = true;
            try
            {
                allowed = (slitarget.Minimum <= proposed && proposed <= slitarget.Maximum);

                if (allowed)
                {
                    actset(proposed.Value);
                }
                else // reject
                {
                    Slider slisource = (Slider)e.Source;
                    slisource.Value = e.OldValue;
                }
            }
            finally
            {
                syncingZoomPrecision = false;
            }
            //return allowed;
        }


        private void ChkSync_Checked(object sender, RoutedEventArgs e)
        {
            precisionMinusZoomSynced = slprecision.Value - slzoom.Value;
        }

        private void ChkSync_Unchecked(object sender, RoutedEventArgs e)
        {
            precisionMinusZoomSynced = null;
        }

        void InvalidateMapPanels()
        {
            foreach (MapPanel mp in LogicalTreeHelper.GetChildren(this.map).OfType<MapPanel>())
            {
                mp.InvalidateVisual();
            }
        }

        private void BuZoomFit_Click(object sender, RoutedEventArgs e)
        {
            PointSummary summary = DataContext.GetRootPointSummary();
            if (summary.Points > 0)
            {
                Location sw = new Location((double)summary.MinLatitude, (double)summary.MinLongitude);
                Location ne = new Location((double)summary.MaxLatitude, (double)summary.MaxLongitude);
                this.map.ZoomToBounds(sw, ne);

                //Debug.WriteLine(string.Format("Fit zoomed to {0} while slzoom is [{1}, {2}, {3}]", map.ZoomLevel,
                //    slzoom.Minimum, slzoom.Value, slzoom.Maximum));
            }
        }


        private void Farewell(FarewellMessage msg)
        {
            MapPref pref = new MapPref()
            {
                Zoom = slzoom.Value,
                PrecisionPower = (int)slprecision.Value,
                SyncZoom = ChkSync.IsChecked == true
            };

            Preference.WriteClassPrefs<Map, MapPref>(pref);
        }

        public class MapPref
        {
            public double Zoom { get; set; }
            public int PrecisionPower { get; set; }
            public bool SyncZoom { get; set; }
        }

    }
}
