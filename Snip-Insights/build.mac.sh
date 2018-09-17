#!/bin/bash

nuget restore SnipInsights.GTK.MacOS.sln
msbuild /p:Configuration="Release" SnipInsights.GTK.MacOS.sln

pushd Installers/Mac
chmod +x GeneratePackage.sh
./GeneratePackage.sh
popd
