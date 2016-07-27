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

            /// <summary>no nodes will have Delta less than this</summary>
            public Mesh MinimumMesh { get; set; }

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
            return FileReader.FindFiles(di, guidance.Wildcard);
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
                    bool needWrite = true;

                    if (outfile.Exists && outfile.LastWriteTimeUtc > fi.LastWriteTimeUtc) // outfile from valid previously created Node
                    {
                        Debug.WriteLine("Existing " + outfile.FullName);
                        firoot = ReadNodeFile(outfile, guidance);

                        // firoot tree having excess detail (smaller minimum delta than guidance) should be fathomed and rewritten
                        // to avoid perpetual unwanted memory/CPU usage.
                        Node.FathomResult fathom = firoot.Fathom(guidance.MinimumMesh.Delta);
                        needWrite = (fathom != Node.FathomResult.None);
                    }
                    
                    if ( firoot == null ) // no existing file satisfies guidance
                    {
                        Debug.WriteLine("Reading " + fi.FullName);

                        firoot = Node.NewGlobalRoot();

                        try
                        {
                            Source source = new Source(fi, disource);

                            firoot.Populate(source, guidance.MinimumMesh);
                        }
                        catch (Exception ex)
                        {
                            // expected to be a sporadic bad file, not a systematic Albiruni bug.
                            // firoot will survive anyway, and may be childless, trivial, or incomplete,
                            // but want to avoid reading the exact source again.

                            // AbnakiLog.Exception(ex, "Error due to " + fi.FullName);
                            guidance.FilesExceptions[fi.FullName] = ex;
                        }

                    }

                    if ( needWrite )
                    {
                        // write firoot to a relative file under ditarget
                        foreach (DirectoryInfo disub in AbnakiFile.DirectorySequence(outfile))
                        {
                            if (false == disub.Exists)
                                disub.Create();
                        }

                        using (Stream outstream = outfile.OpenWrite())
                        using (InputOutput.IBinaryWrite tbw = new TreeBinaryWrite())
                        {
                            tbw.Init(outstream, guidance.MinimumMesh);

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
        public static void Read(Node root, DirectoryInfo ditarget, Guidance guidance)
        {
            //DirectoryInfo diStart = ditarget.GetDirectories()

            foreach (FileInfo fi in ditarget.GetFiles(guidance.Wildcard + FileExt))
            {
                Node firoot = ReadNodeFile(fi, guidance);
                root.Graft(null, firoot);
            }

            foreach ( DirectoryInfo disub in ditarget.GetDirectories())
            {
                Read(root, disub, guidance);
            }

        }

        private static Node ReadNodeFile(FileInfo fi, Guidance guidance)
        {
            Node firoot = null;

            using (Stream stream = fi.OpenRead())
            using (InputOutput.IBinaryRead ibr = new TreeBinaryRead())
            {
                ibr.Init(stream);

                if (ibr.MeshPower.HasValue && ibr.MeshPower >= guidance.MinimumMesh.Power)
                {
                    ibr.ReadSources();

                    firoot = Node.NewGlobalRoot();
                    firoot.Read(ibr);
                }
                else
                {
                    string msg = string.Format("Disregarding file with mesh power {0} less than guidance {1}, {2}", ibr.MeshPower, guidance.MinimumMesh.Power, fi.FullName);
                    Debug.WriteLine(msg);
                    // firoot shall remain null
                }

            }
            return firoot;
        }

    }
}
