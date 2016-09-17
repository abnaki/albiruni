using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using System.Diagnostics;

using MapControl;
using CodeRed.Serialization;
using Abnaki.Albiruni.Message;
using Abnaki.Windows.Software.Wpf;
using Abnaki.Windows.Software.Wpf.Ultimate;
using Abnaki.Windows.Software.Wpf.Profile;

namespace Abnaki.Albiruni.TileHost
{
    /// <summary>
    /// Choices of tile hosts and caches; governs usage and TileImageLoader properties
    /// </summary>
    public class Governor
    {
        public Governor()
        {
            MessageTube.Subscribe<TileHostMessage>(HandleTileHost);
            MessageTube.Subscribe<FarewellMessage>(Farewell);

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

        TileHostSupply hostSupply;

        void RestoreTileHostSupply()
        {
            hostSupply = TileHostSupply.Read();
            if (hostSupply != null)
                hostSupply.SquareUpStatics();
        }

        static string WellBehavedAgent()
        {
            var tuple = Abnaki.Windows.AbnakiReflection.ApplicationNameVersionSplit();
            return tuple.Item1 + " " + tuple.Item2 + "." + tuple.Item3;
        }

        public void Complete()
        {
            if (completed)
                return;

            RestoreTileHostSupply();

            // no default; users might overwhelm it
            // InitializeLocator(LocatorTemplate.CartoLight);

            Pref pref = Preference.ReadClassPrefs<Governor, Pref>();
            if ( pref != null )
            {
                // note, invokes LocatorTemplate constructors
                var prefLocator = LocatorTemplate.Predefined().FirstOrDefault(loc => loc.FileKey == pref.CurrentLocatorKey);

                var joinOrg = from prefPair in pref.OrganizationUserKeys
                              join ogroup in LocatorTemplate.PredefinedOrganizationGroups() on prefPair.Key equals ogroup.Key.FileKey
                              select new { Org = ogroup.Key, UserKey = prefPair.Value };

                foreach ( var pair in joinOrg )
                {
                    pair.Org.UserKey = pair.UserKey;
                }

                if (prefLocator != null)
                    InitializeLocator(prefLocator);

            }

            completed = true;
        }

        bool completed = false;

        void InitializeLocator(LocatorTemplate locDefault)
        {
            ChangeCache(locDefault);
            TileHostMessage msg = new TileHostMessage(locDefault);
            MessageTube.Publish(msg);
        }

        MemoryCache defaultMemCache = null;
        LocatorTemplate CurrentLocator { get; set; }

        void HandleTileHost(TileHostMessage msg)
        {
            using (WaitCursor.InProgress())
            {
                Debug.WriteLine(GetType().Name + " handles " + msg.LocatorTemplate);

                ChangeCache(msg.LocatorTemplate);
            }
        }

        void ChangeCache(LocatorTemplate loctemp)
        {
            CurrentLocator = loctemp;

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
                // depends on commerical provider.  here.com said 30 days max.
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


        private void Farewell(FarewellMessage msg)
        {
            // in the future, may serialize LocatorTemplate where UserDefined=true
            Pref pref = new Pref();

            if (CurrentLocator != null)
                pref.CurrentLocatorKey = CurrentLocator.FileKey;

            foreach (var g in LocatorTemplate.PredefinedOrganizationGroups())
            {
                Organization org = g.Key;

                if (org.UserKey != null && org.UserKey != Organization.UndefinedKey
                    && org.FileKey != null )
                {
                    pref.OrganizationUserKeys[org.FileKey] = org.UserKey;
                }
            }

            Preference.WriteClassPrefs<Governor, Pref>(pref);
        }

        public class Pref
        {
            /// <summary>For lookup of a static LocatorTemplate</summary>
            public string CurrentLocatorKey { get; set; }

            public SerializableDictionary<string, string> OrganizationUserKeys = new SerializableDictionary<string, string>();
        }
    }
}
