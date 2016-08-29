#/bin/csh

# working directory should be root of module
echo albiruni tag is $1
sleep 5

pushd ../windows
source ../windows/Build/release.csh
popd

gitfetchtagbuild $1 Albiruni.sln

# maybe refactor into ../windows/Build

./Build/sign.bat Wpf/bin/Release/Abnaki*dll Wpf/bin/Release/Albiruni.exe

echo Then inno setup
pushd Wpf
test -d SetupOutput || mkdir SetupOutput
pushd SetupOutput
test -d old || mkdir old
mv *.exe old/
popd

/cygdrive/c/progra~2/innose~1/Compil32.exe /cc albiruni.iss

../Build/sign.bat SetupOutput/*.exe

popd
