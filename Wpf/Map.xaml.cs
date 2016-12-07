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
using Abnaki.Windows;
using System.IO;
using System.Reflection;
using Abnaki.Albiruni.TileHost;
using Abnaki.Albiruni.Message;

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

            // Helps delay logic after interactive panning/zooming has stopped briefly.
            this.map.ViewportChanged += vptimer.OnChanged;

            MapNodeLayer layer = GetMapNodeLayer();
            vptimer.Changed += (s, e) => layer.InvalidateVisual();
            vptimer.Settled += (s, e) =>
            {
                CompleteZoom();
                ViewportChangeSettled();
                layer.InvalidateVisual();
            };

            // delay costly logic
            slprecision.ValueChanged += slprecTimer.OnChanged;
            slprecTimer.Settled += slprecTimer_Settled;
            
            MessageTube.Subscribe<FarewellMessage>(Farewell);
            MessageTube.Subscribe<InvalidateMessage>(HandleInvalidate);
            MessageTube.Subscribe<TileLoaderMessage>(HandleTileHost);

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
            priorZoom = 0;
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

        internal void ViewportChangeSettled()
        {
            if (DataContext == null)
                return;

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
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            slzoom.Focus(); // only way to assure KeyUp is raised by something within Map visual tree
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            // never happens before focus

            base.OnKeyUp(e);

            int sign = 0;

            switch (e.Key)
            {
                case Key.OemPlus:
                    if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)
                        || Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                    {
                        sign = 1;
                    }
                    break;

                case Key.OemMinus:
                    if ( Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift) )
                    {
                        // no _
                    }
                    else 
                    {
                        sign = -1;
                    }
                    break;

                case Key.Add:
                    sign = 1;
                    break;

                case Key.Subtract:
                    sign = -1;
                    break;

                case Key.F: // finer precision
                    if (slprecision.Value <= slprecision.Maximum - slprecision.LargeChange)
                        slprecision.Value += slprecision.LargeChange;
                    break;

                case Key.J: // larger precision
                    if (slprecision.Value >= slprecision.Minimum + slprecision.LargeChange)
                        slprecision.Value -= slprecision.LargeChange;
                    break;

            }

            if (sign != 0)
            {
                SafeChangeZoom(sign, e);
            }

        }

        void SafeChangeZoom(int sign, KeyEventArgs e)
        {
            map.ZoomLevel = MapExtensions.Bounded(slzoom.Minimum, map.ZoomLevel + sign * slzoom.LargeChange, slzoom.Maximum);
            e.Handled = true;
            PostZoomInvalidate();
            Debug.WriteLine("Zoomed " + sign);
        }

        #region Originally from xamlmapcontrol/SampleApps/WpfApplication/MainWindow.xaml.cs

        private void MapMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // not raised if hit a MapRectangle 

            if (e.ClickCount == 2)
            {
                Point p = e.GetPosition(map);
                Location loc = map.ViewportPointToLocation(p);

                // map never got mouseup if synchronous:
                Dispatcher.InvokeAsync(() =>{
                    System.Threading.Thread.Sleep(500);
                    this.DataContext.OnLeftDoubleClick(loc);
                }, DispatcherPriority.Background);

                // old demo
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

        double priorZoom;
        LagTimer<EventArgs> vptimer = new LagTimer<EventArgs>();

        LagTimer<RoutedPropertyChangedEventArgs<double>> slprecTimer = new LagTimer<RoutedPropertyChangedEventArgs<double>>();

        bool syncingZoomPrecision = false;
        bool outdatedMesh = false;
        double? precisionMinusZoomSynced;
        bool flexibleSync = false;

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

        void slprecTimer_Settled(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //slzoom.Value += e.NewValue - e.OldValue of slprecision
            double deltaPrecision = e.NewValue - e.OldValue;
            ChangeBoundedBySlider(slzoom, vptimer, e, slzoom.Value + deltaPrecision, z => map.ZoomLevel = z);

            PostZoomInvalidate();
        }

        void PostZoomInvalidate()
        {
            outdatedMesh = true;
            InvalidateVisual();
        }

        private void slzoom_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // this has precedence over the binding to MapViewModel affecting MapBase.
            //CompleteZoom(sender, e);
            //outdatedMesh = true; 
            //InvalidateVisual(); // implied by slprecision Value change
        }

        void CompleteZoom()
        {
            var ezoom = new RoutedPropertyChangedEventArgs<double>(priorZoom, slzoom.Value, Slider.ValueChangedEvent)
            {
                Source = slzoom
            };
            CompleteZoom(ezoom);
            priorZoom = slzoom.Value;
        }

        void CompleteZoom(RoutedPropertyChangedEventArgs<double> ezoom)
        {
            ChangeBoundedBySlider(slprecision, slprecTimer, ezoom, precisionMinusZoomSynced + slzoom.Value, p => slprecision.Value = p);
            priorZoom = ezoom.OldValue;
        }


        void ChangeBoundedBySlider(Slider slitarget,
            LagTimer lagTimer,
            RoutedPropertyChangedEventArgs<double> e,
            double? proposed,
            Action<double> actset)
        {
            if (syncingZoomPrecision) // vestige of old logic without LagTimer
                return;
            if (false == proposed.HasValue)
                return;
            if (false == precisionMinusZoomSynced.HasValue)
                return;

            bool allowed = false;
            syncingZoomPrecision = true;
            // programmatic change of a slider, no lag wanted; and correct logic requires syncingZoomPrecision here.
            lagTimer.Bypass = true;  

            try
            {
                if (slitarget.Minimum <= proposed && proposed <= slitarget.Maximum)
                {
                    actset(proposed.Value);
                }
                else if (flexibleSync)
                {
                    double vbound = MapExtensions.Bounded(slitarget.Minimum, proposed.Value, slitarget.Maximum);
                    actset(vbound);
                }
                else if (e != null) // expect true
                {  // reject
                    Slider slisource = (Slider)e.Source;
                    slisource.Value = e.OldValue;
                }
            }
            finally
            {
                syncingZoomPrecision = false;
                lagTimer.Bypass = false;
            }
            //return allowed;
        }

        void EstablishSync()
        {
            precisionMinusZoomSynced = slprecision.Value - slzoom.Value;
            UpdateSlidersWithSync();
        }

        private void ChkSync_Checked(object sender, RoutedEventArgs e)
        {
            EstablishSync();
        }

        private void ChkSync_Unchecked(object sender, RoutedEventArgs e)
        {
            precisionMinusZoomSynced = null;
            UpdateSlidersWithSync();
        }

        void UpdateSlidersWithSync()
        {
            // Slider Selection implies range of agreeable values
            bool enableRange = precisionMinusZoomSynced.HasValue && (ChkSync.IsChecked == true);

            UpdateSliderSelection(enableRange, slzoom, slprecision, -1);
            UpdateSliderSelection(enableRange, slprecision, slzoom, 1);
        }

        /// <summary>
        /// Change slider Selection to otherSlider [Minimum,Maximum] + sign*precisionMinusZoomSynced
        /// </summary>
        void UpdateSliderSelection(bool enable, Slider slider, Slider otherSlider, double sign)
        {
            slider.IsSelectionRangeEnabled = enable;
            try
            {
                if ( enable )
                {
                    double offset = sign * precisionMinusZoomSynced.Value;
                    slider.SelectionStart = MapExtensions.Bounded(slider.Minimum, otherSlider.Minimum + offset, slider.Maximum);
                    slider.SelectionEnd = MapExtensions.Bounded(slider.SelectionStart, otherSlider.Maximum + offset, slider.Maximum);
                }
            }
            catch
            {
                slider.IsSelectionRangeEnabled = false;
            }
        }

        void InvalidateMapPanels()
        {
            foreach (PanelBase mp in ChildMapPanels())
            {
                mp.InvalidateVisual();
            }
        }

        IEnumerable<PanelBase> ChildMapPanels()
        {
            return LogicalTreeHelper.GetChildren(this.map).OfType<PanelBase>();
        }

        MapNodeLayer GetMapNodeLayer()
        {
            return ChildMapPanels().OfType<MapNodeLayer>().Single();
        }

        private void BuZoomFit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                flexibleSync = true;
                PointSummary summary = DataContext.GetRootPointSummary();
                if (summary.Points > 0)
                {
                    slprecision.Value = slprecision.Minimum;
                    if (precisionMinusZoomSynced.HasValue)
                        EstablishSync();

                    Location sw = new Location((double)summary.MinLatitude, (double)summary.MinLongitude);
                    Location ne = new Location((double)summary.MaxLatitude, (double)summary.MaxLongitude);
                    if (sw.EqualCoordinates(ne)) // unusual
                        this.DataContext.MapCenter = sw;
                    else  // usual
                        this.map.ZoomToBounds(sw, ne);

                    CompleteZoom();
                    //Debug.WriteLine(string.Format("Fit zoomed to {0} while slzoom is [{1}, {2}, {3}]", map.ZoomLevel,
                    //    slzoom.Minimum, slzoom.Value, slzoom.Maximum));

                    if (precisionMinusZoomSynced.HasValue) // sometimes may be revised
                        EstablishSync();
                }
            }
            finally
            {
                flexibleSync = false;
            }
        }

        private void HandleInvalidate(Message.InvalidateMessage msg)
        {
            this.map.InvalidateVisual();
            InvalidateMapPanels();
        }

        #region Tiles

        void HandleTileHost(TileLoaderMessage msg)
        {
            Debug.WriteLine(GetType().Name + " handles " + msg.LocatorTemplate);

            ChangeTileLayer(msg.LocatorTemplate);
        }

        Dictionary<LocatorTemplate, MapTiLayer> mapTileLayers = new Dictionary<LocatorTemplate, MapTiLayer>();
        MapTiLayer emptyMapTileLayer = new MapTiLayer();

        void ChangeTileLayer(LocatorTemplate loctemp)
        {
            MapTiLayer layer = null;
            if (loctemp == null)
            {
                layer = emptyMapTileLayer;
            }
            else
            {
                if (mapTileLayers.ContainsKey(loctemp))
                {
                    layer = mapTileLayers[loctemp];
                }
                else
                {
                    layer = new MapTiLayer(loctemp);
                }

                slzoom.Maximum = map.MaxZoomLevel = layer.MaxZoomLevel;
            }

            if (layer != map.TileLayer)
            {
                ClearTileLayer();
                map.TileLayer = layer;
                ClearTileLayer();

                map.InvalidateVisual();
            }
        }

        void ClearTileLayer()
        {
            if (map.TileLayer is MapTiLayer)
                ((MapTiLayer)map.TileLayer).ClearUpdate();
        }

        #endregion

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
