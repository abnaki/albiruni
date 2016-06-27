using System;
using System.Collections.Generic;
using System.Linq;

using Abnaki.Albiruni.Tree.InputOutput;

namespace Abnaki.Albiruni.Tree
{
    class TreeBinaryWrite : IBinaryWrite
    {
        public System.IO.BinaryWriter Writer
        {
            get;
            private set;
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

        public void Init(System.IO.Stream stream)
        {
            this.Writer = new System.IO.BinaryWriter(stream);
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
