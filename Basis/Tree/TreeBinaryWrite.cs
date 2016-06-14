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
            SortedList<string, Source> mapPathSources = new SortedList<string, Source>();
            
            root.GetSources(mapPathSources);

            this.Writer.Write(mapPathSources.Count);

            foreach ( var pair in mapPathSources )
            {
                Writer.Write(pair.Key);
                pair.Value.Write(this);
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
            this.Writer.Write(source.Path);
        }
    }
}
