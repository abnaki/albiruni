#!/bin/csh -f

# please see:
source ../../windows/Library/Software/WpfApplication/wgetlicense.csh

echo Shall call = $WG
$WG http://wiki.osmfoundation.org/wiki/Licence/Licence_and_Legal_FAQ
$WG http://opendatacommons.org/licenses/odbl/1.0/

$WG https://github.com/sibartlett/Geo/raw/master/LICENSE

$WG https://github.com/ekonbenefits/impromptu-interface/raw/master/License.txt

$WG http://wpffolderbrowser.codeplex.com/license

$WG http://xamlmapcontrol.codeplex.com/license

$WG https://github.com/LZorglub/TimeZone/raw/master/LICENSE

$WG https://github.com/mj1856/GeoTimeZone/raw/master/LICENSE

$WG http://writeablebitmapex.codeplex.com/license

# Forks:

$WG https://github.com/abnaki/photo/raw/master/LICENSE
