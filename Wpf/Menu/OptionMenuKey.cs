using Abnaki.Windows;

namespace Abnaki.Albiruni.Menu
{
    enum OptionMenuKey
    {
        Data,

        [Label("Best Precision", Detail="Smallest cell size, approximate,\nbetween equator and 45° latitude.\nSmaller cells use more memory.")]
        DataMeshPower,
        // May want to move to a control.
        // Ballpark of mesh size from equator to 45 latitude, keeping 2 sig figs.
        [Label("1.7 to 2.4 km")]
        DataMeshPower14 = 14,

        [Label("0.8 to 1.2 km")]
        DataMeshPower15 = 15,

        [Label("430 to 610 m")]
        DataMeshPower16 = 16,

        [Label("210 to 300 m")]
        DataMeshPower17 = 17,

        [Label("100 to 150 m")]
        DataMeshPower18 = 18,

        [Label("50 to 75 m")]
        DataMeshPower19 = 19,

        Map, // parent

        [Label("Cell Color")]
        MapCellColor,

        [Label("Red")]
        MapCellColorRed,

        [Label("Green")]
        MapCellColorGreen,

        [Label("Blue")]
        MapCellColorBlue
    }
}
