using System;
using System.Collections.Generic;
using System.Linq;

using PropertyChanged;

namespace Abnaki.Albiruni.TileHost
{
    /// <summary>
    /// View of some properties for UI
    /// </summary>
    [ImplementPropertyChanged]
    class TiRecord
    {
        public TiRecord(LocatorTemplate loctemp)
        {
            this.LocatorTemplate = loctemp;
        }

        public LocatorTemplate  LocatorTemplate { get; private set; }

        public string Host { get { return this.LocatorTemplate.Org.Domain.Uri.Host;  } }

        public string Style { get { return LocatorTemplate.Style ?? LocatorTemplate.Subdirectory;  } }

        public bool Select { get; set; }

        public string PartialKey
        {
            get
            {
                string key = this.LocatorTemplate.Org.UserKey;
                if (string.IsNullOrEmpty(key))
                    return null;

                return string.Join("",key.Take(12)) + "..."; // confidential
            }
        }

        // want a tested sample image
    }
}
