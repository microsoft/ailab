#!/bin/bash

sudo apt install nuget
nuget restore SnipInsights.GTK.sln
msbuild /p:Configuration="Release" SnipInsights.GTK.sln

pushd Installers/Linux
chmod +x GeneratePackage.sh
./GeneratePackage.sh
popd
