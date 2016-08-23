﻿using System;
using System.Collections.Generic;
using System.Linq;

using MapControl;

namespace Abnaki.Albiruni
{
    /// <summary>
    /// 
    /// </summary>
    class MapTiLayer : TileLayer
    {
        public MapTiLayer(Abnaki.Albiruni.TileHost.LocatorTemplate loctemp)
        {
            this.SourceName = loctemp.Org.Domain.Host + "/" + loctemp.Subdirectory;

            this.Description = "Maps © " + loctemp.Org.Copyright;

            this.TileSource = new TileSource() { UriFormat = loctemp.Template };

            if (loctemp.Org.Public)
            {
                this.MaxParallelDownloads = 1;
                this.MaxZoomLevel = 16;
                // Obey host's terms such as http://wiki.openstreetmap.org/wiki/Tile_usage_policy 
            }
            else
            {
                this.MaxParallelDownloads = 4;
                this.MaxZoomLevel = 19;
            }

        }

        internal void ClearUpdate()
        {
            UpdateTiles(clearTiles: true);
            InvalidateVisual();
        }
    }
}