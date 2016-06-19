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

        //SortedSet<Source> sources = new SortedSet<Source>();

        /// <summary>
        /// Descend to find source files and populate tree starting at root.
        /// </summary>
        public static void GrowTree(Node root, DirectoryInfo disource, DirectoryInfo ditarget, string wildcard = "*")
        {
            GrowSub(root, disource, disource, ditarget, wildcard);
        }

        static void GrowSub(Node root, DirectoryInfo disource, DirectoryInfo di, DirectoryInfo ditarget, string wildcard)
        {
            foreach ( FileInfo fi in di.GetFiles(wildcard + GpxFile.Extension) )
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
                    firoot.Populate(source);

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
                GrowSub(root, disource, disub, ditarget, wildcard);
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
