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
            menu.AddCommandChild<OptionMenuKey>(TopMenuKey.Option, OptionMenuKey.Data);
            menu.AddCommandChild<OptionMenuKey>(OptionMenuKey.Data, OptionMenuKey.DataMeshPower);
            
            AddExclusiveCommands(menu, OptionMenuKey.DataMeshPower, new[] {
                OptionMenuKey.DataMeshPower14,
                OptionMenuKey.DataMeshPower15,
                OptionMenuKey.DataMeshPower16,
                OptionMenuKey.DataMeshPower17,
                OptionMenuKey.DataMeshPower18,
                OptionMenuKey.DataMeshPower19
            });

            menu.AddCommandChild<OptionMenuKey>(TopMenuKey.Option, OptionMenuKey.Map);
            menu.AddCommandChild<OptionMenuKey>(OptionMenuKey.Map, OptionMenuKey.MapScale);
            AddExclusiveCommands(menu, OptionMenuKey.MapScale,
                new[] { OptionMenuKey.MapScaleMetric, OptionMenuKey.MapScaleImperial });

            menu.AddCommandChild<OptionMenuKey>(OptionMenuKey.Map, OptionMenuKey.MapCellColor);


            AddExclusiveCommands(menu, OptionMenuKey.MapCellColor, 
                new[]{OptionMenuKey.MapCellColorRed, OptionMenuKey.MapCellColorGreen, OptionMenuKey.MapCellColorBlue});

        }

        public static void GetMeshFromOption(OptionMenuKey key, Action<int> onpower)
        {
            switch (key)
            {
                case Menu.OptionMenuKey.DataMeshPower14:
                case Menu.OptionMenuKey.DataMeshPower15:
                case Menu.OptionMenuKey.DataMeshPower16:
                case Menu.OptionMenuKey.DataMeshPower17:
                case Menu.OptionMenuKey.DataMeshPower18:
                case Menu.OptionMenuKey.DataMeshPower19:
                    onpower((int)key); // requires enum int to agree
                    break;
            }

        }

    }
}
