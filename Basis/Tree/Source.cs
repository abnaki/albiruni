using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using Abnaki.Windows;
using Abnaki.Albiruni.Tree.InputOutput;
using Abnaki.Albiruni.Providers;

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
            this.Path = AbnakiFile.RelativePath(fi, dibase);
            RefreshIFileFromSource(dibase);
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

        public bool Draw { get; set; }

        public Providers.PointReader PointProvider { get; private set; }

        IFile IFile { get; set; }

        public Providers.IFile RefreshIFileFromSource(DirectoryInfo dibase)
        {
            if ( this.IFile == null )
            {
                FileInfo fi = AbnakiFile.CombinedFilePath(dibase, this.Path);
                Providers.FileReader filer = Providers.FileReader.SelectFileReader(fi);
                this.PointProvider = filer;
                this.IFile = filer.Deserialize(fi);

                IEnumerable<IPoint> points = this.IFile.WayPoints.Concat(
                    this.IFile.Tracks.SelectMany(t => t.Points)).Concat(
                    this.IFile.Routes.SelectMany(t => t.Points));

                IPoint[] couple = points.Take(2).ToArray();
                if (couple.Length == 1)
                {
                    this.SolePoint = new PurePoint(couple[0]);
                }
            }
            return this.IFile;
        }

        /// <summary>
        /// Non-null if there is one single point.
        /// Null if 0 or muliple points exist.
        /// </summary>
        public PurePoint SolePoint { get; private set; }

        public override string ToString()
        {
            return Path;
        }

        public int CompareTo(Source other)
        {
            return this.Path.CompareTo(other.Path);
        }

        const int filever = 3;

        public void Write(IBinaryWrite ibw)
        {
            ibw.Writer.Write(filever);
            ibw.Writer.Write(this.SerialNumber);
            ibw.Writer.Write(this.Path);

            ibw.Writer.Write(SolePoint != null);
            if (SolePoint != null)
            {
                SolePoint.Write(ibw.Writer);
            }
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

            if (v >= 3)
            {
                bool solePointExist = br.ReadBoolean();
                if (solePointExist)
                {
                    var p = new PurePoint();
                    p.Read(br);
                    SolePoint = p;
                }
            }
        }
    }
}
