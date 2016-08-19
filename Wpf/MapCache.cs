using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Abnaki.Albiruni
{
    /// <summary>
    /// Creates an ImageFileCache for map
    /// </summary>
    class MapCache
    {
        public static void Init()
        {
            if (tried)
                return;

            tried = true;
            try
            {
                DirectoryInfo diAppd = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
                DirectoryInfo diSub = diAppd.CreateSubdirectory("Albiruni");
                string cacheName = "ImageCache";
                var imgCache = new MapControl.Caching.ImageFileCache(cacheName, diSub.FullName);

                Cache = imgCache;
                CacheDir = Abnaki.Windows.AbnakiFile.CombinedDirectoryPath(diSub, cacheName);
            }
            catch (Exception ex)
            {
                Cache = null;
                Abnaki.Windows.AbnakiLog.Exception(ex, "MapCache");
            }
        }

        public static System.Runtime.Caching.ObjectCache Cache { get; private set; }

        static bool tried = false;

        public static DirectoryInfo CacheDir { get; private set; }
    }
}
