using System;
using System.Collections.Generic;
using System.Linq;

using Abnaki.Windows.GUI;
using Abnaki.Windows.Software.Wpf.Menu;
using Abnaki.Windows.Software.Wpf.Ultimate;

namespace Abnaki.Albiruni.Menu
{
    class OptionMenuBus : ButtonBus<OptionMenuKey>
    {
        public OptionMenuBus(IMainMenu menu)
        {
            menu.AddCommandChild<OptionMenuKey>(TopMenuKey.Option, OptionMenuKey.Map, "Map");
            menu.AddCommand(new MenuSeed<OptionMenuKey>()
            {
                ParentKey = OptionMenuKey.Map,
                Key = OptionMenuKey.MapCellColor,
                Label = "Cell Color"
            });

            menu.AddExclusiveCommands(OptionMenuKey.MapCellColor, new MenuSeed<OptionMenuKey>[] {
                new MenuSeed<OptionMenuKey>(OptionMenuKey.MapCellColorRed, "Red", true),
                new MenuSeed<OptionMenuKey>(OptionMenuKey.MapCellColorGreen, "Green", false),
                new MenuSeed<OptionMenuKey>(OptionMenuKey.MapCellColorBlue,"Blue", false)
            });

        }



    }
}
