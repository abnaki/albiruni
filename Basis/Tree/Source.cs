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
            lock (m_serlock)
            {
                this.SerialNumber = m_serialNumbers++;
            }
        }

        public Source(FileInfo fi, DirectoryInfo dibase) : this()
        {
            Providers.FileReader filer = Providers.FileReader.SelectFileReader(fi);
            this.PointProvider = filer;
            filer.Deserialize(fi);

            this.Path = AbnakiFile.RelativePath(fi, dibase);
        }

        /// <summary>Less often used
        /// </summary>
        public Source(Providers.PointReader points) : this()
        {
            this.PointProvider = points;
        }

        public int SerialNumber { get; private set; }

        static object m_serlock = new object();
        static int m_serialNumbers = 0;

        public string Path { get; private set; }

        public Providers.PointReader PointProvider { get; private set; }

        public override string ToString()
        {
            return Path;
        }

        public int CompareTo(Source other)
        {
            return this.Path.CompareTo(other.Path);
        }

        const int filever = 2;

        public void Write(IBinaryWrite ibw)
        {
            ibw.Writer.Write(filever);
            ibw.Writer.Write(this.SerialNumber);
            ibw.Writer.Write(this.Path);
        }

        public void Read(BinaryReader br)
        {
            int v = br.ReadInt32();
            if (v < 2)
                throw new NotSupportedException("Old version");

            this.SerialNumber = br.ReadInt32();
            this.Path = br.ReadString();

            lock ( m_serlock )
            {
                m_serialNumbers = Math.Max(this.SerialNumber, m_serialNumbers);
            }

            // leaving GpxFile uninitialized; not desired.
        }
    }
}
