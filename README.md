# Albiruni

Windows desktop software to map large sets of GPS data, such as tracks and waypoints, in an efficient summarized style, rarely found in other open source software.

Named after the medieval Persian scientist Abū Rayḥān Muḥammad ibn Aḥmad Al-Bīrūnī.  Almost 1000 years ago, he accurately measured the Earth and tabulated coordinates of hundreds of locations.

Builds with Visual Studio 2013 and targets .NET Framework 4.5 in Windows.

Release planned for Summer 2016.

Practical applications for visualization:
- personal travel records
- drone flight logs
- scientific research
- business/sales
- real estate
- government/regulatory
- law enforcement
- criminal investigation
- disaster/search/rescue
- miltary/defense
- aviation
- agriculture/forestry
- political campaigns
- energy utilities and exploration

Some domains would require minor straightforward implementation.  Anything is possible, given a source of sample data, or file, and a documented specification/standard, better yet already coded in C#.

OpenStreetMaps will be displayed, as they are free.   Some popular alternative maps require licensing, yet they are feasible for a derivative work. 

## Ideas on the drawing board

The display may vary with dimensions of interest, such as date/time or any given property of the spatial points.  Color, shade, and hiding/filtering will help answer questions about these variables.  Users may want to know:
- All regions covered in a range of dates
- Dates when a region of interest was covered
- Regions that were never covered
- Correlations/intersections between multiple specific tracks, or between a GPS track and other date/location information

## Acknowledgments

Much harder if not for:

- [XAML Map Control](http://xamlmapcontrol.codeplex.com)
- [Geospatial Library](https://github.com/sibartlett/Geo.git)

Strongly appreciated:

- [gpsbabel](https://github.com/gpsbabel/gpsbabel)
- WPFFolderBrowser
- WpfScreenHelper
- Fody PropertyChanged
- Prism for WPF
- Extended WPF Toolkit Community Edition
