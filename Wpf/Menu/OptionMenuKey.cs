using System.ComponentModel;

namespace Abnaki.Albiruni.Menu
{
    enum OptionMenuKey
    {
        Map, // parent

        [Description("Map Cell Color")]
        MapCellColor,

        MapCellColorRed,
        MapCellColorGreen,
        MapCellColorBlue
    }
}
