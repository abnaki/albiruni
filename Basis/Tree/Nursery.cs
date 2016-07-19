using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;

using Abnaki.Windows;
using Abnaki.Albiruni.Providers;

namespace Abnaki.Albiruni.Tree
{
    /// <summary>
    /// Finds sources to create Tree
    /// </summary>
    public class Nursery
    {
        public const string FileExt = ".abt";

        /// <summary>
        /// A hierarchy of files is created under this subdirectory
        /// </summary>
        public const string CacheDir = ".albiruni";

        public class Guidance
        {
            /// <summary>start of filenames</summary>
            public string Wildcard = "*";

            /// <summary>minimum threshold, in degrees; no nodes will have Delta below this</summary>
            /// <remarks>90 * 2^-12 is about 2.4 km at equator, and -15 would be 300 meters.
            /// </remarks>
            public decimal MinimumPrecision = 1;

            public SortedDictionary<string, Exception> FilesExceptions = new SortedDictionary<string, Exception>();
        }

        public static void SearchForFiles(DirectoryInfo disource, Guidance guidance, List<FileInfo> resultingFiles)
        {
            resultingFiles.AddRange(FindLocalFiles(disource, guidance));

            foreach ( DirectoryInfo disub in disource.GetDirectories())
            {
                SearchForFiles(disub, guidance, resultingFiles);
            }
        }

        static IEnumerable<FileInfo> FindLocalFiles(DirectoryInfo di, Guidance guidance)
        {
            return di.GetFiles(guidance.Wildcard + GpxFile.Extension)
                .Where(f => f.Name.ToLower().EndsWith(GpxFile.Extension));
            // without Where, it is rather perturbing that GetFiles returns files ending with .gpx~
        }

        /// <summary>
        /// Descend to find source files and populate tree starting at root.
        /// </summary>
        public static void GrowTree(Node root, DirectoryInfo disource, DirectoryInfo ditarget, Guidance guidance)
        {
            GrowSub(root, disource, disource, ditarget, guidance);
        }

        static void GrowSub(Node root, DirectoryInfo disource, DirectoryInfo di, DirectoryInfo ditarget, Guidance guidance)
        {
            foreach ( FileInfo fi in FindLocalFiles(di, guidance) )
            {
                string relpath = AbnakiFile.RelativePath(di, disource);
                FileInfo outfile = AbnakiFile.CombinedFilePath(ditarget, relpath, Path.ChangeExtension(fi.Name, FileExt));

                Node firoot = null;

                try
                {
                    if (outfile.Exists && outfile.LastWriteTimeUtc > fi.LastWriteTimeUtc) // outfile from valid previously created Node
                    {
                        Debug.WriteLine("Existing " + outfile.FullName);
                        firoot = ReadNodeFile(outfile);
                    }
                    else
                    {
                        Debug.WriteLine("Reading " + fi.FullName);

                        firoot = Node.NewGlobalRoot();

                        try
                        {
                            Source source = new Source(fi, disource);

                            firoot.Populate(source, guidance.MinimumPrecision);
                        }
                        catch (Exception ex)
                        {
                            // expected to be a sporadic bad file, not a systematic Albiruni bug.
                            // firoot will survive anyway, and may be childless, trivial, or incomplete,
                            // but want to avoid reading the exact source again.

                            // AbnakiLog.Exception(ex, "Error due to " + fi.FullName);
                            guidance.FilesExceptions[fi.FullName] = ex;
                        }


                        // write firoot to a relative file under ditarget
                        foreach (DirectoryInfo disub in AbnakiFile.DirectorySequence(outfile))
                        {
                            if (false == disub.Exists)
                                disub.Create();
                        }

                        using (Stream outstream = outfile.OpenWrite())
                        using (InputOutput.IBinaryWrite tbw = new TreeBinaryWrite())
                        {
                            tbw.Init(outstream);

                            tbw.WriteSources(firoot);

                            firoot.Write(tbw);

                            Debug.WriteLine("Wrote " + outfile + ", size " + outstream.Position);
                        }
                    }
                }
                finally
                {

                }

                if (firoot != null)
                    root.Graft(null, firoot);

            }

            foreach ( DirectoryInfo disub in di.GetDirectories())
            {
                GrowSub(root, disource, disub, ditarget, guidance);
            }
        }

        /// <summary>
        /// Practically only for testing
        /// </summary>
        public static void Read(Node root, DirectoryInfo ditarget, string wildcard)
        {
            //DirectoryInfo diStart = ditarget.GetDirectories()

            foreach (FileInfo fi in ditarget.GetFiles(wildcard))
            {
                Node firoot = ReadNodeFile(fi);
                root.Graft(null, firoot);
            }

            foreach ( DirectoryInfo disub in ditarget.GetDirectories())
            {
                Read(root, disub, wildcard);
            }

        }

        private static Node ReadNodeFile(FileInfo fi)
        {
            using (Stream stream = fi.OpenRead())
            using (InputOutput.IBinaryRead ibr = new TreeBinaryRead())
            {
                ibr.Init(stream);

                ibr.ReadSources();

                Node firoot = Node.NewGlobalRoot();
                firoot.Read(ibr);

                return firoot;
            }
        }

    }
}
