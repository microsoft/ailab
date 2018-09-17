#!/bin/bash

version=$(<../../version.txt)

brew cask install packages

cp -a ../../SnipInsight.Forms.GTK/bin/Release/. Release/SnipInsight.app/Contents/MacOS/
rm -r build
mkdir build
/usr/local/bin/packagesbuild --package-version $version -v snipInsightInstaller.pkgproj > log.txt
mv build/SnipInsights.pkg build/SnipInsights-$version.pkg
