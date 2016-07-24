using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Abnaki.Albiruni.Providers
{
    /// <summary>
    /// Reads data and returns IFile, abstractly, depending on data type
    /// </summary>
    public abstract class FileReader
    {

        public PointDump Points { get; private set; }

        protected abstract IFile OpenFile(FileInfo fi);

        public void Deserialize(FileInfo fi)
        {
            if (false == fi.Exists)
                throw new FileNotFoundException("Nonexistent " + fi.FullName);

            IFile filedat = OpenFile(fi);
            this.Points = new PointDump(filedat);

        }

        public static FileReader SelectFileReader(FileInfo fi)
        {
            string x = fi.Extension.ToLower();

            if (x == GpxFile.Extension)
                return new GpxFile();

            if (x == Image.JpegFile.Extension)
                return new Image.JpegReader();

            throw new NotSupportedException("No way to read this type of file, " + fi.FullName);
        }

        static IEnumerable<string> ValidExtensions()
        {
            yield return GpxFile.Extension;
            yield return Image.JpegFile.Extension;
        }

        public static IEnumerable<FileInfo> FindFiles(DirectoryInfo di, string leadingWildcard)
        {
            IEnumerable<IEnumerable<FileInfo>> manySets = ValidExtensions().Select(x =>
                di.GetFiles(leadingWildcard + x)
                .Where(f => f.Name.ToLower().EndsWith(x)));
              // without Where, it is rather perturbing that GetFiles returns files ending with .x~

            return manySets.SelectMany(set => set);
        }
    }
}
