#!/bin/bash

version=$(<../../version.txt)

brew update
brew cask upgrade
brew cask install packages

cp -a ../../SnipInsight.Forms.GTK/bin/Release/. "Release/Snip Insights.app/Contents/MacOS/"
rm -r build
mkdir build
/usr/local/bin/packagesbuild --package-version $version -v snipInsightInstaller.pkgproj > log.txt
mv "build/Snip Insights.pkg" "build/Snip Insights-$version.pkg"
