using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

using Geo;
using Geo.Abstractions.Interfaces;

namespace Abnaki.Albiruni.Tree
{

    public class Node
    {
        protected Node()
        {

        }

        public static Node NewGlobalRoot()
        {
            return new Node() { Axis = Axis.NorthSouth, Degrees = -90, Delta = 180 };
        }

        public Axis Axis { get; private set; }

        public double Degrees { get; private set; }

        public double Delta { get; private set; }

        public Source Source { get; set; }

        List<Node> children = new List<Node>();

        // see Microsoft.Experimental.Collections
        Lazy<MultiValueDictionary<Source, IPosition>> mapSourcePositions = new Lazy<MultiValueDictionary<Source, IPosition>>();

        // find/add descendants, given point(s)
        void Grow(Node grandparent, IEnumerable<IPosition> positions, Source source, double minDelta = 0.0001)
        {
            double newDelta = this.Delta / 2;
            if (grandparent != null)
                newDelta = grandparent.Delta / 2;

            var validPositions = positions.Where(p => this.ContainPosition(p)).ToArray();
            if ( false == validPositions.Any() )
                return;

            if (newDelta < minDelta + double.Epsilon) // <= 
            {
                // leaf node

                mapSourcePositions.Value.AddRange(source, validPositions);

            }
            else
            // recurse, halving the applicable Delta
            {
                Axis newAxis = this.Axis == Albiruni.Axis.EastWest ? Albiruni.Axis.NorthSouth : Albiruni.Axis.EastWest;
                // i.e. same as grandparent if that exists

                double xmin, xmax;
                switch (newAxis)
                {
                    case Albiruni.Axis.NorthSouth:
                        xmin = -90;
                        xmax = 90;
                        break;
                    case Albiruni.Axis.EastWest:
                        xmin = -180;
                        xmax = 180;
                        break;
                    default:
                        throw new NotSupportedException("No support for Axis " + this.Axis);
                }
                if (grandparent != null)
                {
                    xmin = grandparent.Degrees;
                    xmax = xmin + grandparent.Delta;
                }

                Node lowChild = new Node() { Axis = newAxis, Degrees = xmin, Delta = (xmax - xmin) / 2, Source = source };

                Node highChild = new Node() { Axis = newAxis, Degrees = xmin + lowChild.Delta, Delta = lowChild.Delta, Source = source };

                children.Add(lowChild);
                children.Add(highChild);

                lowChild.Grow(this, validPositions, source, minDelta);
                highChild.Grow(this, validPositions, source, minDelta);
            }
        }

        bool ContainPosition(IPosition pos)
        {
            Coordinate c = pos.GetCoordinate();
            double xc;
            switch (this.Axis)
            {
                case Albiruni.Axis.NorthSouth: xc = c.Latitude; break;
                case Albiruni.Axis.EastWest: xc = c.Longitude; break;
                default:
                    throw new NotSupportedException("No support for Axis " + this.Axis);
            }

            return ((this.Degrees - double.Epsilon < xc) && (xc < this.Degrees + this.Delta + double.Epsilon));
        }

        /// <summary>
        /// Grow tree to cover given data
        /// </summary>
        /// <param name="gpx"></param>
        public void Populate(Providers.GpxFile gpx) // may want an interface
        {
            List<Coordinate> coordinates =
            gpx.GeoGpsData.Tracks
            .SelectMany(track => track.Segments)
            .SelectMany(seg => seg.Fixes)
            .Select(fix => fix.Coordinate)
            .ToList();

            coordinates.AddRange(
                gpx.GeoGpsData.Waypoints.Select(w => w.Coordinate));

            Source source = new Source(); // bare bones

            Grow(null, coordinates, source, minDelta: 0.1); // very coarse

        }

        public void DebugPrint()
        {
            Debug.WriteLine(this);

            Debug.Indent();

            foreach ( var key in mapSourcePositions.Value.Keys )
            {
                Debug.WriteLine(key);
                Debug.Indent();
                foreach ( IPosition pos in mapSourcePositions.Value[key] )
                {
                    Debug.WriteLine(pos.GetCoordinate());
                }
                Debug.Unindent();
            }

            foreach ( Node child in children )
            {
                child.DebugPrint();
            }

            Debug.Unindent();
        }

        public override string ToString()
        {
            return this.Axis + " " + this.Degrees + " +" + this.Delta + " " + (this.Degrees + this.Delta);
        }

    }
}
