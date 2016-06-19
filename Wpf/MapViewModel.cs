using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Media;

using Abnaki.Albiruni.Tree;
using Abnaki.Windows.Software.Wpf;
using MapControl;
using PropertyChanged;

namespace Abnaki.Albiruni
{
    [ImplementPropertyChanged]
    class MapViewModel 
    {
        public MapViewModel()
        {
            Rectangles = new ObservableCollection<MapRectangle>();
            Symbols = new ObservableCollection<MapPath>();
            Tracks = new ObservableCollection<MapPath>();

            MessageTube.Subscribe<Node>(HandleTree);
        }

        public Location MapCenter { get; set; }

        public ObservableCollection<MapRectangle> Rectangles { get; private set; }
        public ObservableCollection<MapPath> Symbols { get; private set; }
        public ObservableCollection<MapPath> Tracks { get; private set; }

        //public ObservableCollection<Point> Points { get; set; }
        //public ObservableCollection<Polyline> Polylines { get; set; }

        void HandleTree(Node root)
        {
            var stat = root.GetStatistic();
            Debug.WriteLine(stat);

            ClearAdornments();

            // here is the whole motivation of Albiruni

        }

        void ClearAdornments()
        {
            Rectangles.Clear();
            Symbols.Clear();
            Tracks.Clear();
        }

        public void Testing()
        {
            MapCenter = new Location(30, -100);

            MapRectangle r = new MapRectangle();
            r.South = MapCenter.Latitude;
            r.West = MapCenter.Longitude;
            r.North = r.South + 0.1;
            r.East = r.West + 0.1;
            r.Fill = new SolidColorBrush(Color.FromArgb((byte)32, (byte)255, (byte)0, (byte)0));
            Rectangles.Add(r);

            var diamond = new Abnaki.Albiruni.Graphic.Symbol.Diamond(MapCenter, 0.01, 0.015);
            diamond.Fill = new SolidColorBrush(Color.FromArgb((byte)64, (byte)0, (byte)0, (byte)255));
            this.Symbols.Add(diamond);

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
