$env:PACKAGE_VERSION = Get-Content ./../../version.txt -Raw

if(Test-Path -Path \output ) { Remove-Item -Recurse -Force \output }
if(Test-Path -Path .\build ) { Remove-Item -Recurse -Force .\build }
if(Test-Path -Path .\Release ) { Remove-Item -Recurse -Force .\Release }

mkdir \output
mkdir .\build	
mkdir .\Release

Copy-item -Force -Recurse "../../bin/x64/Release/*" -Destination "./Release/"
pushd Release
DesktopAppConverter.exe -Installer .\ -AppExecutable "Snip Insights.exe" -PackageName "SnipInsights" -AppDisplayName "Snip Insights" -PackageDisplayName "Snip Insights" -Publisher "CN=Microsoft" -Destination "\output" -Version $env:PACKAGE_VERSION -MakeApp -Verbose -Sign
popd
Copy-item -Force -Recurse "/output/*" -Destination "./build/"
Compress-Archive "./build/SnipInsights" "./build/Snip Insights-$env:PACKAGE_VERSION.zip"
