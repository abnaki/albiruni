# File Conversion

## gpsbabel

Converts from many historic file formats into gpx.

With git I cloned
https://github.com/gpsbabel/gpsbabel

With Cygwin setup, I installed the appropriate packages (for my cpu architecture)
- mingw64-x86_64-gcc-core
- mingw64-x86_64-qt5-base
- autoconf
- gperf

Then in Cygwin bash, I did configure and make, as in
http://www.gpsbabel.org/htmldoc-development/Source.html

That created gpsbabel.exe.

Within bash, in a directory containing files, for example,
/appropriatepath/gpsbabel.exe -i gdb -o gpx -f hawaii.gdb -F hawaii.gpx
