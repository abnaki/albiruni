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

        public int? ReadVersion { get; set; }

        public Source ReadSource()
        {
            //string path = this.Reader.ReadString();
            int ser = this.Reader.ReadInt32();
            return mapNumberSources[ser];
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
                Source source = new Source();
                source.Read(this.Reader);
                mapNumberSources[source.SerialNumber] = source;
            }
        }

        public void Init(System.IO.Stream stream)
        {
            Reader = new System.IO.BinaryReader(stream);
            
            ReadVersion = Reader.ReadInt32(); // written first in TreeBinaryWrite.Init()

            if (ReadVersion >= 3)
            {
                MeshPower = Reader.ReadInt32();
            }
        }

        public int? MeshPower { get; private set; }

        SortedList<int, Source> mapNumberSources = new SortedList<int, Source>();
    }
}
