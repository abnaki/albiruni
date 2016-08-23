using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using System.Diagnostics;

using MapControl;
using Abnaki.Albiruni.Message;
using Abnaki.Windows.Software.Wpf;

namespace Abnaki.Albiruni.TileHost
{
    /// <summary>
    /// Choices of tile hosts and caches; governs usage and TileImageLoader properties
    /// </summary>
    class Governor
    {
        public Governor()
        {
            MessageTube.Subscribe<TileHostMessage>(HandleTileHost);

            if (testing)
            {
                for (int i = 0; i <= 500; i += 50)
                    mapCountAgent[i] = "";
            }
            else
            {
                // logic associated with MapCache.AgeCutoff timespan and Organization.Public
                mapCountAgent[0] = WellBehavedAgent();
                mapCountAgent[500] = "RoeteKaart weergawe 0.2"; // high usage
                mapCountAgent[1500] = "Sistema de Exibição de Mapa  5.A"; // excessive
            }
        }

        bool testing = false;

        static string WellBehavedAgent()
        {
            return "Albiruni " + Assembly.GetEntryAssembly().GetName().Version;
        }

        public void Complete()
        {
            if (completed)
                return;

            LocatorTemplate locDefault = LocatorTemplate.CartoLight;
            TileHostMessage msg = new TileHostMessage(locDefault);
            ChangeCache(locDefault);
            MessageTube.Publish(msg);
            completed = true;
        }

        bool completed = false;
        MemoryCache defaultMemCache = null;

        void HandleTileHost(TileHostMessage msg)
        {
            Debug.WriteLine(GetType().Name + " handles " + msg.LocatorTemplate);

            ChangeCache(msg.LocatorTemplate);
        }

        void ChangeCache(LocatorTemplate loctemp)
        {
            if (loctemp.Org.Public)
            {
                // Server usage is far more critical than timeliness, hence large timespan.
                // openstreetmaps.org requires 7 days at least.
                TileImageLoader.MinimumCacheExpiration = TimeSpan.FromDays(60);
                TileImageLoader.DefaultCacheExpiration = TimeSpan.FromDays(60);
            }
            else
            {
                TileImageLoader.MinimumCacheExpiration = TimeSpan.FromDays(15);
                TileImageLoader.DefaultCacheExpiration = TimeSpan.FromDays(30);
            }

            if (TileImageLoader.Cache is MemoryCache)
            {
                defaultMemCache = (MemoryCache)TileImageLoader.Cache;
            }
            else if (TileImageLoader.Cache is MapCache.AlbiruniFileCache)
            {
                ((MapCache.AlbiruniFileCache)TileImageLoader.Cache).Enable = false;
            }

            MapCache mc = new MapCache(loctemp, testing);

            if (mc.Cache == null)
            {  
                // unexpected.  restore a default.
                if (defaultMemCache != null)
                    TileImageLoader.Cache = defaultMemCache;

                TileImageLoader.HttpUserAgent = mapCountAgent.Values.LastOrDefault(); // worst case
            }
            else
            {
                mc.Cache.Enable = true;
                TileImageLoader.Cache = mc.Cache;

                Debug.WriteLine("TileImageLoader uses " + mc.Cache);

                if (loctemp.Org.Public)
                {
                    mc.Cache.CountTriggers = mapCountAgent.Keys;
                    mc.Cache.CountAchieved += Cache_CountAchieved;
                    mc.Cache.Complete();
                }
                else
                {   // user has relationship with host
                    TileImageLoader.HttpUserAgent = WellBehavedAgent();
                }
            }
        }

        void Cache_CountAchieved(long count)
        {
            if (mapCountAgent.Keys.Any(k => count >= k))
            {
                var pair = mapCountAgent.Last(p => count >= p.Key);

                Debug.WriteLine("Achieved tile count " + pair.Key + ", agent now " + pair.Value);

                TileImageLoader.HttpUserAgent = pair.Value;
            }
        }

        /// <summary>
        /// Keys are tile usage count relative to MapCache.AgeCutoff.
        /// Values are http user agent.
        /// Goes to server, a required courtesy.
        /// </summary>
        Dictionary<long, string> mapCountAgent = new Dictionary<long, string>();
    }
}
