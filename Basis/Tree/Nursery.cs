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

        public class Guidance
        {
            /// <summary>start of filenames</summary>
            public string Wildcard = "*";

            /// <summary>minimum threshold, in degrees; no nodes will have Delta below this</summary>
            /// <remarks>90 * 2^-12 is about 2.4 km at equator, and -15 would be 300 meters.
            /// </remarks>
            public double MinimumPrecision = 1;
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
            foreach ( FileInfo fi in di.GetFiles(guidance.Wildcard + GpxFile.Extension) )
            {
                string relpath = AbnakiFile.RelativePath(di, disource);
                FileInfo outfile = AbnakiFile.CombinedFilePath(ditarget, relpath, Path.ChangeExtension(fi.Name, FileExt));

                Node firoot;

                if (outfile.Exists && outfile.LastWriteTimeUtc > fi.LastWriteTimeUtc) // outfile from valid previously created Node
                {
                    firoot = ReadNodeFile(outfile);
                }
                else
                {
                    Source source = new Source(fi);
                    //sources.Add(source);

                    firoot = Node.NewGlobalRoot();
                    firoot.Populate(source, guidance.MinimumPrecision);

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
