#!/bin/bash

version=$(<../../version.txt)

perl -ple "s/Version:.*/Version: $version/g" SnipInsights/DEBIAN/control > SnipInsights/DEBIAN/control.tmp
mv SnipInsights/DEBIAN/control.tmp SnipInsights/DEBIAN/control

cp -a ../../SnipInsight.Forms.GTK/bin/Release/. SnipInsights/usr/lib/SnipInsights
dpkg-deb --build SnipInsights
rm -r build
mkdir build
mv SnipInsights.deb build/SnipInsights-$version.deb
