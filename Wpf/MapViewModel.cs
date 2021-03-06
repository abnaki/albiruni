﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

using Abnaki.Albiruni.Tree;
using Abnaki.Albiruni.Graphic;
using Abnaki.Windows.Software.Wpf;
using Abnaki.Windows.Software.Wpf.Menu;
using MapControl;
using PropertyChanged;
using Abnaki.Albiruni.Message;
using System.Windows.Threading;

namespace Abnaki.Albiruni
{
    [ImplementPropertyChanged]
    public class MapViewModel 
    {
        public MapViewModel()
        {
            Rectangles = new List<MapRectangle>();
            Symbols = new ObservableCollection<FrameworkElement>();
            Tracks = new ObservableCollection<MapPath>();
            EmphasizedPaths = new ObservableCollection<MapPath>();
            ViewPortRect = new MapRectangle(); // unspecified

            SetCellBrush(cellHueLevel, 0, 0);

            ClearLastNodesFound();

            MessageTube.SubscribeCostly<Message.RootNodeMessage>(HandleTree);
            MessageTube.SubscribeCostly<DrawSourceMessage>(HandleDrawSource);
            ButtonBus<Menu.OptionMenuKey>.HookupSubscriber(HandleOption);
        }

        public Location MapCenter { get; set; }

        /// <summary>
        /// will be minimum limit on Delta of a Node to show individually as a MapRectangle
        /// </summary>
        public Mesh DisplayMesh { get; set; }

        /// <summary>Necessary to bind DisplayMesh Power</summary>
        public int DisplayMeshPower
        {
            get { return DisplayMesh.Power; }
            set { DisplayMesh = new Mesh(value); }
        }

        /// <summary>Finest Mesh for user control
        /// </summary>
        public int MeshMaximumPower { get; set; }

        public bool ScaleMetric { get; set; }

        public bool GraticuleEnabled { get; set; }

        public List<MapRectangle> Rectangles { get; private set; }
        
        // may want to use BulkObservableCollection or similar...no luck.

        /// <summary>such as waypoints</summary>
        public ObservableCollection<FrameworkElement> Symbols { get; private set; }

        public ObservableCollection<MapPath> Tracks { get; private set; }
        public ObservableCollection<MapPath> EmphasizedPaths { get; private set; }

        /// <summary>Last known rectangle; not bound to control.  i.e. set{} does not affect the map.
        /// </summary>
        MapRectangle ViewPortRect { get; set; }

        /// <summary>1 by 1 display units translated onto map</summary>
        MapRectangle DisplayUnitRect { get; set; }

        SourceMapper sourceMapper = new SourceMapper();

        public void SetViewPort(MapRectangle viewRect, MapRectangle unitRect)
        {
            DisplayUnitRect = unitRect;

            if (this.ViewPortRect.EqualCoordinates(viewRect))
            {
                // skip
            }
            else
            {
                using (new WaitCursor())
                {
                    this.ViewPortRect = viewRect;
                    UpdateAdornments();
                }
            }
        }

        public void UpdateMesh()
        {
            using (new WaitCursor())
            {
                ClearLastNodesFound();
                UpdateAdornments();
            }
        }

        void UpdateAdornments()
        {
            UpdateRectangles(newRoot: false);
        }

        public PointSummary GetRootPointSummary()
        {
            Node.Statistic stat;
            if (RootNode == null)
                stat = new Node.Statistic();
            else
                stat = RootNode.GetStatistic();

            return stat.ContentSummary.FinalSummary();
        }

        Message.RootNodeMessage RootNodeMessage { get; set; }
        
        Node RootNode
        {
            get
            {
                return (this.RootNodeMessage == null) ? null : this.RootNodeMessage.Root;
            }
        }

        public void HandleTree(Message.RootNodeMessage msg)
        {
            //root.DebugPrint();

            ClearTreeDependentObjects();
            ClearLastNodesFound();

            sourceMapper.SourceDirectory = msg.SourceDirectory;

            bool newRoot = msg.Root != RootNode;
            this.RootNodeMessage = msg;

            UpdateRectangles(newRoot);
            UpdateSolePoints();
        }

        /// <summary>
        /// depends on RootNode and ViewPortRect
        /// </summary>
        void UpdateRectangles(bool newRoot)
        {
            ClearViewPortDependentObjects();

            if (RootNode == null)
                return; // OK

            PointSummary pointSummary = GetRootPointSummary();
            if (pointSummary.Points > 0)
            {

                // (rather than MapRectangle, want an abstraction to allow for two disjoint shapes 
                // such as two rectangles abutting longitude +- 180; 
                // and easily check for its intersection with a Node.)

                DescentLimits limits = new DescentLimits();
                if (newRoot)
                {
                    decimal midLat = (pointSummary.MinLatitude.Value + pointSummary.MaxLatitude.Value) / 2;
                    decimal midLon = (pointSummary.MinLongitude.Value + pointSummary.MaxLongitude.Value) / 2;
                    MapCenter = new Location((double)midLat, (double)midLon);
                    // note that the view may still remain zoomed in far enough to not intersect any points in the tree.  ergo, user must zoom out.

                    // predicting bounds from last known view
                    double viewWidth = ViewPortRect.East - ViewPortRect.West;
                    double viewHeight = ViewPortRect.North - ViewPortRect.South;

                    limits.LogicalBound = MapExtensions.NewMapRectangle(

                        west: MapCenter.Longitude - viewWidth / 2,
                        east: MapCenter.Longitude + viewWidth / 2,
                        north: MapCenter.Latitude + viewHeight / 2,
                        south: MapCenter.Latitude - viewHeight / 2
                    );
                }
                else
                {
                    limits.LogicalBound = ViewPortRect;
                }

                limits.MinimumDelta = this.DisplayMesh.Delta;

                //Debug.WriteLine("limits " + limits);

                Queue<MapRectangle> qrect = new Queue<MapRectangle>();

                Stopwatch watch = new Stopwatch();
                watch.Start();

                AddDescendantRectangles(RootNode, null, limits, qrect);

                // all assorted non-coalesced rectangles
                this.Rectangles.AddRange(qrect);

                watch.Stop();

                Debug.WriteLine(Rectangles.Count + " Rectangles created in " + watch.Elapsed);

                //if (Rectangles.Count > 0)
                //{
                //    Debug.WriteLine("Westernmost rectangle " + Rectangles.Min(r => r.West));
                //    Debug.WriteLine("Easternmost rectangle " + Rectangles.Max(r => r.East));
                //    Debug.WriteLine("Southernmost rectangle " + Rectangles.Min(r => r.South));
                //    Debug.WriteLine("Northernmost rectangle " + Rectangles.Max(r => r.North));
                //}
            }
        }

        class DescentLimits
        {
            public MapRectangle LogicalBound { get; set; }
            public decimal MinimumDelta { get; set; }

            public override string ToString()
            {
                return LogicalBound.ToStringUseful() + ", MinimumDelta=" + MinimumDelta;
            }
        }

        enum DescentResult
        {
            /// <summary>no intersection of descendants with given region</summary>
            None = 0,
            /// <summary>descendants have any points in a given region</summary>
            PointsCovered = 1,
            /// <summary>implies PointsCovered and rectangles were added covering descendant points</summary>
            NewRectangles = 2
        };

        /// <summary>
        /// Here is the principal motivation for Albiruni.
        /// Add a Rectangle for every Node in tree that intersects ViewPortRect,
        /// not descending to any Nodes having Delta < MinimumPrecision.
        /// </summary>
        DescentResult AddDescendantRectangles(Node node, Node parent, DescentLimits limits, Queue<MapRectangle> quRects)
        {
            switch (node.Axis)
            {
                case Axis.NorthSouth:
                    if (node.Degrees > (decimal)limits.LogicalBound.North)
                        return DescentResult.None;
                    if (node.Degrees + node.Delta < (decimal)limits.LogicalBound.South)
                        return DescentResult.None;
                    break;
                case Axis.EastWest:
                    if (node.Degrees > (decimal)limits.LogicalBound.East)
                        return DescentResult.None;
                    if (node.Degrees + node.Delta < (decimal)limits.LogicalBound.West)
                        return DescentResult.None;
                    break;
            }

            //Debug.Indent(); // slow?

            List<DescentResult> results = new List<DescentResult>();
            Queue<MapRectangle> childQuRects = new Queue<MapRectangle>();
            
            if ( node.Children != null )
            {
                results.Add(AddDescendantRectangles(node.Children.Item1, node, limits, childQuRects));
                results.Add(AddDescendantRectangles(node.Children.Item2, node, limits, childQuRects));

            }

            if ( node.SourceCount > 0)
            {
                results.Add(DescentResult.PointsCovered);
            }

            MapRectangle rect = null;

            if (node.Delta >= limits.MinimumDelta
                && results.Any(r => r == DescentResult.PointsCovered) 
                && parent != null && parent.Delta >= limits.MinimumDelta )
            {
                // any descendants exist, and node has not been excluded for view or precision reasons (above)

                switch (node.Axis)
                {
                    case Axis.NorthSouth:
                        if ( (double)node.Delta < this.DisplayUnitRect.North - DisplayUnitRect.South)
                            break; //  not displayed

                        rect = new MapRectangle()
                        {
                            South = (double)node.Degrees,
                            North = (double)(node.Degrees + node.Delta),
                            West = (double)parent.Degrees,
                            East = (double)(parent.Degrees + parent.Delta)
                        };
                        break;
                    case Axis.EastWest:
                        if ( (double)node.Delta < this.DisplayUnitRect.East - DisplayUnitRect.West)
                            break; //  not displayed

                        rect = new MapRectangle()
                        {
                            South = (double)parent.Degrees,
                            North = (double)(parent.Degrees + parent.Delta),
                            West = (double)node.Degrees,
                            East = (double)(node.Degrees + node.Delta)
                        };
                        break;
                    default:
                        throw new NotSupportedException("No support for axis " + node.Axis);
                }
            }
            else if ( childQuRects.Count > 0  )
            {
                MapRectangle rectCoalesce = CoalesceRectangle(childQuRects);
                if ( rectCoalesce == null )
                {   // assortment of non-space-filling rectangles
                    while (childQuRects.Count > 0)
                    {
                        quRects.Enqueue(childQuRects.Dequeue());
                    }
                }
                else
                {
                    rect = rectCoalesce;
                }
            }

            if (rect != null)
            {
                CompleteRectangle(rect);

                quRects.Enqueue(rect);

                results.Add(DescentResult.NewRectangles);
            }

            //Debug.Unindent();

            if (results.Count > 0)
                return (DescentResult)results.Max();
            else
                return DescentResult.None;
        }

        void CompleteRectangle(MapRectangle r)
        {
            r.Fill = m_defaultFillBrush;

            // events unused because MapNodeLayer does not put MapRectangles in a control.
            ////r.MouseLeftButtonUp += MapRectangle_MouseLeftButtonUp; // never gets  e.ClickCount == 2
            //r.MouseLeftButtonDown += MapRectangle_MouseLeftButtonDown;
            //// need to be able to pan ParentMap if mouse hit rectangle.
            //r.AddHandler(UIElement.MouseDownEvent, new RoutedEventHandler(RouteToParentMap));
            //r.AddHandler(UIElement.MouseMoveEvent, new RoutedEventHandler(RouteToParentMap));
            //r.AddHandler(UIElement.MouseUpEvent, new RoutedEventHandler(RouteToParentMap));
            
        }

        /// <summary>
        /// Non-null if rectangles fill a rectangular space.  Otherwise null.
        /// </summary>
        /// <param name="rectangles">must be non-intersecting
        /// </param>
        MapRectangle CoalesceRectangle(IEnumerable<MapRectangle> rectangles)
        {
            MapRectangle bounds = new MapRectangle()
            {
                North = rectangles.Max(r => r.North),
                South = rectangles.Min(r => r.South),
                West = rectangles.Min(r => r.West),
                East = rectangles.Max(r => r.East)
            };

            Func<MapRectangle,double> Area = r => (r.North - r.South)*(r.East - r.West);
            double boundArea = Area(bounds);
            double sumArea = rectangles.Sum(r => Area(r));

            if (Math.Abs(sumArea - boundArea) < double.Epsilon)
                return bounds;

            return null;
        }

        static void RouteToParentMap(object sender, RoutedEventArgs e)
        {
            MapPath r = (MapPath)sender;
            r.ParentMap.RaiseEvent(e);
            e.Handled = true;
        }

        void MapRectangle_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                MapRectangle r = (MapRectangle)sender;
                MapBase map = r.ParentMap;
                System.Windows.Point p = e.GetPosition(map);
                Location loc = map.ViewportPointToLocation(p);

                OnLeftDoubleClick(loc);
            }
        }

        void ClearViewPortDependentObjects()
        {
            // different sets will need to be created depending on intersection with viewport
            Rectangles.Clear();
        }

        void ClearTreeDependentObjects()
        {
            ClearViewPortDependentObjects();
            EmphasizedPaths.Clear();
            Symbols.Clear();
            Tracks.Clear();
            sourceMapper.Clear();
        }

        Location lastLocation;
        Node.FindResult lastNodeSpan;

        void ClearLastNodesFound()
        {
            lastLocation = null;
            lastNodeSpan = null;
        }

        public void OnHover(Location loc)
        {
            bool hoverChanged;

            Message.SourceRecordMessage msg = SourceRecordOfLocation(loc, out hoverChanged); // depends on MinimumMesh

            if (msg != null && hoverChanged
                && msg.FinalSummary.Points > 0) // implies msg.NodeExtremes must contain non-nulls
            {
                MessageTube.Publish(msg);

                EmphasizedPaths.Clear();

                //MapRectangle r = MapExtensions.NewMapRectangle(ps.MinLongitude.Value, ps.MaxLongitude.Value, ps.MinLatitude.Value, ps.MaxLatitude.Value);
                // ps=msg.FinalSummary is inadequate for visibility; could be a single point or small/thin cluster.
                // Also MapControl 2.9 did not draw any MapRectangle Stroke.

                Graphic.Curve.OutlineRectangle r = new Graphic.Curve.OutlineRectangle(
                    west: msg.NodeExtremes.MinLong.Value,
                    east: msg.NodeExtremes.MaxLong.Value,
                    south: msg.NodeExtremes.MinLat.Value,
                    north: msg.NodeExtremes.MaxLat.Value);

                r.Stroke = Brushes.Yellow;
                r.StrokeThickness = 2;

                EmphasizedPaths.Add(r);
            }
        }

        public void CenterAroundEmphasis()
        {
            IEnumerable<MapPolyline> figures = EmphasizedPaths.OfType<MapPolyline>();

            if (figures.Any())
            {
                double midLat = (figures.Min(p => p.MinLatitude()) + figures.Max(p => p.MaxLatitude())) / 2;
                double midLon = (figures.Min(p => p.MinLongitude()) + figures.Max(p => p.MaxLongitude())) / 2;
                MapCenter = new Location(midLat, midLon);
            }
        }

        /// <summary>
        /// </summary>
        /// <returns>non null only if Nodes exist at location
        /// </returns>
        Message.SourceRecordMessage SourceRecordOfLocation(Location loc, out bool changed)
        {
            changed = false;

            if (RootNode == null)
                return null;

            if (loc.EqualCoordinates(lastLocation))
            {
                // lastNodes are correct
            }
            else
            {
                Node.FindResult nodes = new Node.FindResult();
                RootNode.FindNodes((decimal)loc.Latitude, (decimal)loc.Longitude, this.DisplayMesh, nodes);
                
                changed = (lastNodeSpan == null || false == lastNodeSpan.SameNodes(nodes));

                lastLocation = loc;
                lastNodeSpan = nodes;

            }

            Message.SourceRecordMessage msg = null;

            if (lastNodeSpan != null)
            {
                msg = new Message.SourceRecordMessage(lastNodeSpan);
            }

            return msg;

            //if (nodes.Count > 0)
            //{
            //    Debug.WriteLine("Hovered on ");
            //    Debug.Indent();
            //    foreach (Node n in nodes)
            //    {
            //        Debug.WriteLine(n);
            //    }
            //    Debug.Unindent();
            //}
        }

        internal void OnLeftDoubleClick(Location loc)
        {
            bool locationChanged; // unused.  user can invoke a source multiple times, not required to move mouse.
            Message.SourceRecordMessage recsMsg = SourceRecordOfLocation(loc, out locationChanged);
            if (recsMsg != null)
            {
                int n = recsMsg.SourceRecords.Count();
                if (n == 1)
                {
                    Message.InvokeSourceMessage msg = new Message.InvokeSourceMessage(recsMsg.SourceRecords.Single());
                    MessageTube.Publish(msg);
                }
                else
                {
                    Debug.WriteLine("Not invoking " + n + " sources.  Expect uniqueness.");
                }
            }
        }

        System.Windows.Media.Brush m_defaultFillBrush;
        const int cellHueLevel = 255;
        const int cellAlpha = 48;

        void SetCellBrush(int red, int green, int blue)
        {
            m_defaultFillBrush = new SolidColorBrush(Color.FromArgb((byte)cellAlpha, (byte)red, (byte)green, (byte)blue));
            UpdateRectangles(newRoot: false);
        }

        private void HandleOption(ButtonMessage<Menu.OptionMenuKey> msg)
        {
            switch (msg.Key)
            {
                case Menu.OptionMenuKey.MapScaleMetric:
                    ScaleMetric = msg.Checked == true;
                    MessageTube.Publish(new Message.InvalidateMessage());
                    break;
                case Menu.OptionMenuKey.MapScaleImperial:
                    ScaleMetric = msg.Checked != true;
                    MessageTube.Publish(new Message.InvalidateMessage());
                    break;

                case Menu.OptionMenuKey.MapGraticule:
                    GraticuleEnabled = msg.Checked == true;
                    MessageTube.Publish(new Message.InvalidateMessage());
                    break;

                case Menu.OptionMenuKey.MapDrawSolePoint:
                    UpdateSolePoints(msg.Checked == true);
                    break;
            }

            if (true == msg.Checked )
            {
                switch (msg.Key)
                {
                    case Menu.OptionMenuKey.MapCellColorRed:
                        SetCellBrush(cellHueLevel, 0, 0);
                        break;
                    case Menu.OptionMenuKey.MapCellColorGreen:
                        SetCellBrush(0, cellHueLevel, 0);
                        break;
                    case Menu.OptionMenuKey.MapCellColorBlue:
                        SetCellBrush(0, 0, cellHueLevel);
                        break;
                }
            }
            Menu.OptionMenuBus.GetMeshFromOption(msg.Key, p => MeshMaximumPower = p);
            
        }

        private void HandleDrawSource(DrawSourceMessage msg)
        {
            SubDrawSources(new[] { msg.SourceRecord.Source });
        }

        void SubDrawSources(IEnumerable<Source> sources)
        {
            // sometimes very costly
            Dispatcher.CurrentDispatcher.InvokeAsync(() =>
            {
                using (WaitCursor.InProgress())
                {
                    sourceMapper.UpdateSources(sources, this);
                }
            },
            DispatcherPriority.Background);
        }

        bool EnableSolePoints { get; set; }

        void UpdateSolePoints(bool enable)
        {
            EnableSolePoints = enable;
            UpdateSolePoints();
        }

        void UpdateSolePoints()
        {
            if (RootNodeMessage == null)
                return;

            IEnumerable<Source> solePointSources = this.RootNodeMessage.Sources.Where(s => s.SolePoint != null).ToArray();

            foreach (Source source in solePointSources )
            {
                source.Draw = EnableSolePoints;
            }

            SubDrawSources(solePointSources);
        }

        public void Testing()
        {
            MapCenter = new Location(30, -100); // Texas

            MapRectangle r = new MapRectangle();
            r.South = MapCenter.Latitude;
            r.West = MapCenter.Longitude;
            r.North = r.South + 0.1;
            r.East = r.West + 0.1;
            r.Fill = m_defaultFillBrush;
            Rectangles.Add(r);

            //var diamond = new Abnaki.Albiruni.Graphic.Symbol.Diamond(MapCenter, 0.01, 0.015);
            //diamond.Fill = new SolidColorBrush(Color.FromArgb((byte)64, (byte)0, (byte)0, (byte)255));
            //this.Symbols.Add(diamond);

            var track = new Abnaki.Albiruni.Graphic.Curve.Track();
            track.Locations = new Location[]
            {
                MapCenter,
                new Location(MapCenter.Latitude + 0.1, MapCenter.Longitude - 0.2)
            };
            track.Stroke = Brushes.Blue;
            track.StrokeThickness = 2;
            this.Tracks.Add(track);
        }

    }

}
