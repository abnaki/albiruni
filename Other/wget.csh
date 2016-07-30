#!/bin/csh -f
# Update copies of licenses of referenced components, necessary to build release
# Prefer Raw file link on github.  Note that /raw/ replaces customary /blob/

test -d OtherLicenses || mkdir OtherLicenses
cd OtherLicenses

set WG = 'wget -x -nv'

$WG http://wpftoolkit.codeplex.com/license

$WG https://github.com/sibartlett/Geo/raw/master/LICENSE

$WG https://github.com/ekonbenefits/impromptu-interface/raw/master/License.txt

$WG https://github.com/PrismLibrary/Prism/raw/master/LICENSE

# see https://www.nuget.org/packages/PropertyChanged.Fody
test -d PropertyChanged.Fody || mkdir PropertyChanged.Fody
$WG -P PropertyChanged.Fody https://opensource.org/licenses/mit-license.php 

$WG http://wpffolderbrowser.codeplex.com/license

$WG https://github.com/micdenny/WpfScreenHelper/raw/master/LICENSE

$WG http://xamlmapcontrol.codeplex.com/license

$WG https://github.com/LZorglub/TimeZone/raw/master/LICENSE

$WG https://github.com/mj1856/GeoTimeZone/raw/master/LICENSE

# Forks:

$WG https://github.com/abnaki/photo/raw/master/LICENSE

