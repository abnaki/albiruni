using System;
using System.Collections.Generic;
using System.IO;

using Abnaki.Windows;
using Abnaki.Albiruni.Tree.InputOutput;

namespace Abnaki.Albiruni.Tree
{
    /// <summary>
    /// Base class; may point to a file
    /// </summary>
    public class Source : IComparable<Source>
    {
        public Source()
        {
            
        }

        public Source(FileInfo fi)
        {
            this.GpxFile = new Providers.GpxFile();
            this.GpxFile.Deserialize(fi);

            DirectoryInfo dicur = new DirectoryInfo(Environment.CurrentDirectory); // may want to pass in

            this.Path = AbnakiFile.RelativePath(fi, dicur);
            
        }

        public string Path { get; private set; }

        public Providers.GpxFile GpxFile { get; private set; } // may want to generalize type

        public override string ToString()
        {
            return Path;
        }

        public int CompareTo(Source other)
        {
            return this.Path.CompareTo(other.Path);
        }

        const int filever = 1;

        public void Write(IBinaryWrite ibw)
        {
            ibw.Writer.Write(filever);
            ibw.Writer.Write(this.Path);
        }

        public void Read(BinaryReader br)
        {
            int v = br.ReadInt32();
            this.Path = br.ReadString();
            // leaving GpxFile uninitialized; not desired.
        }
    }
}
