using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Windows.Threading;

using MapControl.Caching;

using Abnaki.Albiruni.TileHost;
using Abnaki.Windows;
using Abnaki.Windows.Software.Wpf.Diplomat;

namespace Abnaki.Albiruni
{
    /// <summary>
    /// Creates a cache of tiles
    /// </summary>
    public class MapCache
    {
        /// <summary>Usage accounting
        /// </summary>
        public static readonly TimeSpan AgeCutoff = TimeSpan.FromDays(1);

        public MapCache(LocatorTemplate loctemp, bool testing)
        {
            try
            {
                // may want to be configurable; an organization could use a shared network location.
                DirectoryInfo diAppd = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));

                DirectoryInfo diSub = diAppd.CreateSubdirectory("Albiruni");

                DirectoryInfo diSpecific = diSub;
                foreach (string part in SiteQualifiers(loctemp))
                {
                    diSpecific = diSpecific.CreateSubdirectory(part);
                }

                Cache = new AlbiruniFileCache(diSub, diSpecific, loctemp, testing);

            }
            catch (Exception ex)
            {
                Cache = null;
                Notifier.Error(ex);
            }
        }

        /// <summary>
        /// Is a System.Runtime.Caching.ObjectCache, per TileImageLoader.Cache
        /// </summary>
        internal AlbiruniFileCache Cache { get; private set; }

        //public DirectoryInfo CacheDir { get; private set; }

        public static IEnumerable<string> SiteQualifiers(LocatorTemplate loctemp)
        {
            yield return loctemp.Org.Domain.Uri.Host; // sufficiently precise domain

            if (false == string.IsNullOrWhiteSpace(loctemp.Subdirectory))
                yield return loctemp.Subdirectory;

        }

        internal class AlbiruniFileCache : ImageFileCache
        {
            public AlbiruniFileCache(DirectoryInfo di, DirectoryInfo diSpecific, LocatorTemplate loctemp, bool testing)
                : base(di.Name, di.Parent.FullName)
            {
                RootDir = di;
                SiteDir = diSpecific;
                //  note ImageFileCache keys imply site-specific location relative to RootDir.
                //  SiteDir is needed for housekeeping here.

                CountTriggers = Enumerable.Empty<long>();

                RecountCache();

                // timer.Interval = AgeCutoff;
                recountTimer.Interval = testing ? TimeSpan.FromSeconds(20) : AgeCutoff;
                recountTimer.Tick += (s, e) => RecountCache();

                lazy = loctemp.Org.Public;
            }

            /// <summary>Need to do this after setting CountTriggers and CountSurpassed
            /// </summary>
            internal void Complete()
            {
                DoTriggers();
            }

            DirectoryInfo RootDir { get; set; }
            DirectoryInfo SiteDir { get; set; }

            /// <summary>
            /// How many files were cached in last AgeCutoff period
            /// </summary>
            public long Count { get; set; }

            public bool Enable
            {
                set
                {
                    recountTimer.IsEnabled = value;
                }
            }

            public IEnumerable<long> CountTriggers { get; set; }

            Dictionary<long, bool> countsTriggered = new Dictionary<long, bool>();

            public event Action<long> CountAchieved;

            DispatcherTimer recountTimer = new DispatcherTimer();

            //public override bool Add(string key, object value, System.Runtime.Caching.CacheItemPolicy policy, string regionName = null)
            //{
            //    bool b = base.Add(key, value, policy, regionName);
            //    return b;
            //}

            bool lazy = false;

            public override void Set(string key, object value, System.Runtime.Caching.CacheItemPolicy policy, string regionName = null)
            {
                base.Set(key, value, policy, regionName);  // heart of ImageFileCache

                if (lazy)
                    System.Threading.Thread.Sleep(50);

                Count++;
                DoTriggers();
            }

            void DoTriggers()
            {
                if (Count % 10 != 0)
                    return;
                // limited to every 10th change

                var han = CountAchieved;
                if (han == null)
                    return;

                Diagnostic();

                // raise event(s) newly triggered
                IEnumerable<long> surpCounts = 
                    CountTriggers.Where(k => Count >= k && false == countsTriggered.ContainsKey(k))
                    .OrderBy(k => k);

                foreach ( long k in surpCounts )
                {
                    countsTriggered[k] = true;
                    han(k);
                }

            }

            void RecountCache()
            {
                countsTriggered.Clear();

                DateTime unow = DateTime.UtcNow;

                IEnumerable<FileInfo> recentFiles = SiteDir.GetFiles("*", SearchOption.AllDirectories)
                    .Where(fi => unow - fi.LastWriteTimeUtc < AgeCutoff);

                this.Count = recentFiles.Count();

                Diagnostic();
            }

            void Diagnostic()
            {
                Debug.WriteLine(GetType().Name + " Count " + Count);
            }

            public override string ToString()
            {
                return GetType().Name + " " + RootDir.FullName;
            }
        }
    }
}
