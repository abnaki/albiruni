using System;
using System.Collections.Generic;
using System.Linq;

using Abnaki.Albiruni.Tree.InputOutput;

namespace Abnaki.Albiruni.Tree
{
    class TreeBinaryWrite : IBinaryWrite
    {
        internal const int FileVersion = 4;

        public System.IO.BinaryWriter Writer
        {
            get;
            private set;
        }

        /// <summary>
        /// True if code has evolved too much since version, so file should be ignored.
        /// </summary>
        /// <param name="version">may be from old file</param>
        public static bool ExpiredVersion(int version)
        {
            // FileVersion and signifcant progress:
            // 4 = Basis/Providers/Geo/Gpx/PointDuck.cs will not use a time lacking gps elevation.
            return version < 4;
        }

        public void WriteSources(Node root)
        {
            List<Source> sources = new List<Source>();
            
            root.GetSources(sources);

            this.Writer.Write(sources.Count);

            foreach ( Source source in sources )
            {
                //Writer.Write(source.SerialNumber);
                source.Write(this);
            }
        }

        public void Init(System.IO.Stream stream, Mesh minimumMesh)
        {
            this.Writer = new System.IO.BinaryWriter(stream);
            this.Writer.Write(FileVersion);

            this.Writer.Write(minimumMesh.Power);
        }

        public void Dispose()
        {
            if (Writer != null)
                Writer.Dispose();
        }

        public void ReferenceSource(Source source)
        {
            // this.Writer.Write(source.Path); // bad to write for every Node
            this.Writer.Write(source.SerialNumber);
        }
    }
}
