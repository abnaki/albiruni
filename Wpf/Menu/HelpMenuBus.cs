using System;
using System.Collections.Generic;
using System.Linq;

using Abnaki.Windows;
using Abnaki.Windows.Software.Wpf.Menu;
using Abnaki.Windows.GUI;
using Abnaki.Windows.Software.Wpf.Ultimate;
using System.Diagnostics;

namespace Abnaki.Albiruni.Menu
{
    class HelpMenuBus : ButtonBus<HelpMenuKey>
    {
        public HelpMenuBus(IMainMenu menu)
        {
            menu.AddCommandChild(TopMenuKey.Help, HelpMenuKey.Wiki);
        }

        protected override void HandleButton(ButtonMessage<HelpMenuKey> m)
        {
            base.HandleButton(m);

            switch ( m.Key )
            {
                case HelpMenuKey.Wiki:
                    object uri = Properties.Settings.Default["WikiUri"];
                    if ( uri != null )
                    {
                        using ( var p = Process.Start(uri.ToString()) )
                        {
                            // ok
                        }
                    }

                    break;

            }
        }
    }

    enum HelpMenuKey
    {
        [Label("Reference...", Detail = "Browse web reference documentation")]
        Wiki
    }
}
