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
        [Label("1.7 to 2.4 km", Detail = "1.1 to 1.5 mi \narea 4.2 to 6.0 km²")]
        DataMeshPower14 = 14,

        [Label("0.8 to 1.2 km", Detail = "0.5 to 0.75 mi \narea 1.0 to 1.5 km²")]
        DataMeshPower15 = 15,

        [Label("430 to 610 m", Detail = "0.3 mi \narea 0.3 km²")]
        DataMeshPower16 = 16,

        [Label("210 to 300 m", Detail = "700 to 1000 ft \narea 6 to 9 ha")]
        DataMeshPower17 = 17,

        [Label("100 to 150 m", Detail = "350 to 500 ft \narea 1.6 to 2.3 ha")]
        DataMeshPower18 = 18,

        [Label("50 to 75 m", Detail = "180 to 250 feet \narea 0.4 to 0.6 ha")]
        DataMeshPower19 = 19,

        Map, // parent

        [Label("Scale")]
        MapScale,

        [Label("Kilometers", Detail="SI or metric units, km, m")]
        MapScaleMetric,

        [Label("Miles", Detail="US customary or Imperial units")]
        MapScaleImperial,

        [Label("Cell Color")]
        MapCellColor,

        [Label("Red")]
        MapCellColorRed,

        [Label("Green")]
        MapCellColorGreen,

        [Label("Blue")]
        MapCellColorBlue,

        [Label("Graticule", Detail="lines of latitude and longitude")]
        MapGraticule,

        [Label("Sole Points", Detail=
            "When checked, show a symbol for every file having one single point.\n"
            + "For example, photos.")]
        MapDrawSolePoint,

        Detail,

        [Label("Time",
            Detail = "Time units from your Windows Region / Formats.\n"
                + "It uses 12 or 24 hour time as setup in Windows.\n"
                + "(Short date format is used regardless.)")]
        DetailTime,

        [Label("Minutes or Short", Detail="Your Windows Region / Formats / Short time format")]
        DetailTimeShort,

        [Label("Seconds or Long", Detail="Your Windows Region / Formats / Long time format")]
        DetailTimeLong
    }
}
