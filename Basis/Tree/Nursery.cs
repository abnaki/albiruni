using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Threading;

using Abnaki.Windows;
using Abnaki.Albiruni.Providers;
using Abnaki.Windows.Thread;

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

            public int? PotentialSourceFileCount { get; set; }
            public long? PotentialSourceFileBytes { get; set; }

            internal void CountPotentialSourceFiles(IEnumerable<FileInfo> files)
            {
                if ( files.Any() )
                {
                    PotentialSourceFileCount = (PotentialSourceFileCount ?? 0) + files.Count();
                    PotentialSourceFileBytes = (PotentialSourceFileBytes ?? 0) + files.Sum(f => f.Length);
                }
            }

            internal void CountSourceFile()
            {
                SourceFileCount++;
                var h = SourceFileCounted;
                if ( h != null )
                    h(SourceFileCount);
            }

            int SourceFileCount { get; set; }

            public event Action<int> SourceFileCounted;

            public bool StillAbleToWrite = true;

            //public readonly System.Threading.AutoResetEvent Done = new System.Threading.AutoResetEvent(false);
        }

        public static void SearchForFiles(DirectoryInfo disource, Guidance guidance)
        {
            var files = FindLocalFiles(disource, guidance).ToList();
            guidance.CountPotentialSourceFiles(files);

            foreach ( DirectoryInfo disub in disource.GetDirectories())
            {
                SearchForFiles(disub, guidance);
            }
        }

        static IEnumerable<FileInfo> FindLocalFiles(DirectoryInfo di, Guidance guidance)
        {
            return FileReader.FindFiles(di, guidance.Wildcard);
        }

        /// <summary>
        /// Descend to find source files and populate tree starting at root.
        /// </summary>
        /// <param name="ditarget">null means to forbid writing
        /// </param>
        public static void GrowTree(Node root, DirectoryInfo disource, DirectoryInfo ditarget, Guidance guidance)
        {
            GrowSub(root, disource, disource, ditarget, guidance);
        }

        static void GrowSub(Node root, DirectoryInfo disource, DirectoryInfo di, DirectoryInfo ditarget, Guidance guidance)
        {
            foreach ( FileInfo fi in FindLocalFiles(di, guidance) )
            {
                string relpath = AbnakiFile.RelativePath(di, disource);
                FileInfo outfile = null;
                if ( ditarget != null )
                    outfile = AbnakiFile.CombinedFilePath(ditarget, relpath, Path.ChangeExtension(fi.Name, FileExt));

                GrowByFile(root, fi,  disource, outfile, guidance);
            }

            foreach ( DirectoryInfo disub in di.GetDirectories())
            {
                GrowSub(root, disource, disub, ditarget, guidance);
            }
        }

        static void GrowByFile(Node root, FileInfo fi, DirectoryInfo disource, FileInfo outfile, Guidance guidance)
        {
            Node firoot = null;

            try
            {
                bool needWrite = (outfile != null);

                if (outfile != null && outfile.Exists && outfile.LastWriteTimeUtc > fi.LastWriteTimeUtc) // outfile from valid previously created Node
                {
                    //Debug.WriteLine("Existing " + outfile.FullName);
                    using (var fileTimer = new DiagnosticTimer(outfile, "reading"))
                    {
                        firoot = ReadNodeFile(outfile, guidance);
                    }

                    // firoot tree having excess detail (smaller minimum delta than guidance) should be fathomed and rewritten
                    // to avoid perpetual unwanted memory/CPU usage.
                    if (firoot != null)
                    {
                        Node.FathomResult fathom = firoot.Fathom(guidance.MinimumMesh.Delta);
                        needWrite = (fathom != Node.FathomResult.None);
                    }
                }

                if (firoot == null) // no existing file satisfies guidance
                {
                    //Debug.WriteLine("Reading " + fi.FullName);

                    firoot = Node.NewGlobalRoot();

                    try
                    {
                        Source source;
                        using (var fileTimer = new DiagnosticTimer(fi, "reading"))
                        {
                            source = new Source(fi, disource);
                        }

                        using (var fileTimer = new DiagnosticTimer(fi, "populating new Node"))
                        {
                            firoot.Populate(source, guidance.MinimumMesh);
                        }
                    }
                    catch (Exception ex)
                    {
                        // expected to be a sporadic bad file, not a systematic Albiruni bug.
                        // firoot will survive anyway, and may be childless, trivial, or incomplete,
                        // but want to avoid reading the exact source again, so needWrite remains true.

                        // AbnakiLog.Exception(ex, "Error due to " + fi.FullName);
                        guidance.FilesExceptions[fi.FullName] = ex;
                    }

                }

                if (needWrite && guidance.StillAbleToWrite)
                {
                    try
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
                    catch (Exception ex)
                    {
                        AbnakiLog.Exception(ex);
                        guidance.StillAbleToWrite = false;
                        guidance.FilesExceptions[fi.FullName] = ex;
                    }
                }
            }
            finally
            {
                guidance.CountSourceFile();
            }

            if (firoot != null)
                root.Graft(null, firoot);


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

        class DiagnosticTimer : Abnaki.Windows.Thread.PayloadTimer<FileInfo>
        {
            public DiagnosticTimer(FileInfo fi, string verb)
                : base(payload: fi, due: DueTimeSpan)
            {
                this.Expire += exfi => AbnakiLog.Comment(GetLogComment(verb), exfi.FullName);
            }

            static TimeSpan DueTimeSpan { get { return TimeSpan.FromMinutes(3);  } }

            static string GetLogComment(string verb)
            {
                return "Exceeded " 
                    + DueTimeSpan.ToString("G", System.Globalization.CultureInfo.InvariantCulture)
                    + " while " + verb;
            }
        }
    }
}
