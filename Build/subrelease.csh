#/bin/csh

# working directory should be root of module
echo albiruni tag is $1
sleep 5

pushd ../windows
source ../windows/Build/release.csh
popd

gitfetchtagbuild $1 Albiruni.sln

echo Now want to sign exe and dll files
echo Then inno setup
echo Then sign newest SetupOutput exe
