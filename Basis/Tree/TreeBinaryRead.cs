using System;
using System.Collections.Generic;
using System.Linq;

namespace Abnaki.Albiruni.Tree
{
    class TreeBinaryRead : InputOutput.IBinaryRead
    {
        public System.IO.BinaryReader Reader
        {
            get;
            private set;
        }

        public Source ReadSource()
        {
            string path = this.Reader.ReadString();
            return mapPathSources[path];
        }

        public void Dispose()
        {
            if (Reader != null)
                Reader.Dispose();
        }


        public void ReadSources()
        {
            int n = this.Reader.ReadInt32();

            for ( int i = 0; i < n; i++ )
            {
                string path = Reader.ReadString();
                Source source = new Source();
                source.Read(Reader);
                mapPathSources[path] = source;
            }
        }

        public void Init(System.IO.Stream stream)
        {
            Reader = new System.IO.BinaryReader(stream);
        }

        SortedList<string, Source> mapPathSources = new SortedList<string, Source>();
    }
}
